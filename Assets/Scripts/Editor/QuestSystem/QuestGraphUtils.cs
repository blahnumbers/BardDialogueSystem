using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json;
using XNode;
using Bard.QuestSystem;
using Bard.Configuration.Editor;

namespace Bard.XNodeEditor {
	public class QuestGraphUtils {
		public static string ExportPath => DialogueSystemPreferences.GetOrCreateSettings().QuestDataGenerationPath;
		private static string ClassPath => DialogueSystemPreferences.GetOrCreateSettings().QuestClassGenerationPath;

		public static Quest Export(QuestGraph graph) {
			var rootNode = graph.nodes.OfType<QuestNode>().First();
			Quest quest = new() {
				Id = graph.name,
				Name = rootNode.name,
				Type = rootNode.Type,
				Steps = new(),
				Conditions = new()
			};
			var steps = graph.nodes.OfType<QuestStepNode>().OrderBy(s => s.position.y).ToList();
			foreach (var step in steps) {
				quest.Steps.Add(new() {
					Id = step.Id,
					Description = step.Description,
					Notification = step.Notification,
					IsFinal = step.IsFinal
				});
			}
			var conditionsNodes = graph.nodes.OfType<QuestConditionsNode>();
			foreach (var node in conditionsNodes) {
				foreach (var c in node.Conditions) {
					quest.Conditions.Add(new() {
						Id = c.Id,
						Description = c.Description,
						ShowsNotification = c.ShowsNotification
					});
				}
			}
			return quest;
		}

		public static void ExportQuestData(NodeGraph target) {
			string path = ExportPath;
			List<Quest> quests = new();
			var graphData = new List<Quest>() { Export((QuestGraph)target) };

			if (File.Exists(path)) {
				var existingData = File.ReadAllText(path);
				quests = JsonConvert.DeserializeObject<List<Quest>>(existingData);
				for (int i = 0; i < quests.Count; i++) {
					var replacement = graphData.Find(t => t.Id == quests[i].Id);
					if (replacement != null) {
						quests[i] = replacement;
						graphData.Remove(replacement);
						Debug.Log("Replacing quest: " + replacement.Id);
					}
				}
			}
			else {
				// Ensure directory exists
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			graphData.ForEach(t => {
				quests.Add(t);
				Debug.Log("Adding new Quest: " + t.Id);
			});

			var json = JsonConvert.SerializeObject(quests, new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			});
			File.WriteAllText(path, json);

			Debug.Log($"Exported graph to {path}");
			AssetDatabase.Refresh();
		}

		public static void ExportQuestDataAll(NodeGraph target) {
			string path = ExportPath;
			if (File.Exists(path)) {
				if (!EditorUtility.DisplayDialog("Warning", "This action will rewrite all data in the Quests data file.\nAre you sure you want to continue?", "Continue", "Cancel")) {
					return;
				}
			}
			else {
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			List<Quest> quests = new();
			var graphPath = AssetDatabase.GetAssetPath(target);
			var graphDirectory = Path.GetDirectoryName(graphPath).Replace("\\", "/").Split("/").Last();
			foreach (var file in Directory.GetFiles(Path.GetDirectoryName(graphPath), "*.asset")) {
				var graph = AssetDatabase.LoadAssetAtPath(file, typeof(QuestGraph)) as QuestGraph;
				Debug.Log("Exporting " + graph.name);
				quests.Add(Export(graph));
			}
			quests.Sort((a, b) => a.Type - b.Type);
			var json = JsonConvert.SerializeObject(quests, new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			});
			File.WriteAllText(path, json);

			Debug.Log($"Exported {quests.Count} graphs to {path}");
			AssetDatabase.Refresh();
		}

		public static void Import(Quest quest, QuestGraph graph) {
			if (graph == null) {
				Debug.LogError("Please create a valid graph before importing data into it.");
				return;
			}

			var n = graph.AddNode<QuestNode>();
			AssetDatabase.AddObjectToAsset(n, graph);
			n.Setup(quest);

			NodePort port = n.GetOutputPort("Conditions");

			var cn = graph.AddNode<QuestConditionsNode>();
			AssetDatabase.AddObjectToAsset(cn, graph);
			cn.Setup(quest.Conditions);
			cn.position = new Vector2(600, 0);
			port.Connect(cn.GetInputPort("Input"));
			
			port = n.GetOutputPort("Steps");
			Vector2 position = new(0, 250);
			foreach (var step in quest.Steps) {
				if (step.Id == 0) continue;
				var sn = graph.AddNode<QuestStepNode>();
				AssetDatabase.AddObjectToAsset(sn, graph);
				sn.Setup(step);
				sn.position = position;
				position += Vector2.up * 250;
				port.Connect(sn.GetInputPort("Input"));
				port = sn.GetOutputPort("Output");
			}
		}

		public static void GenerateAllDefinitions() {
			var quests = LoadAllQuestGraphs();
			foreach (var q in quests) {
				GenerateQuestDetails(q);
			}
			AssetDatabase.Refresh();
		}

		public static void GenererateDefinitions(QuestGraph graph) {
			try {
				GenerateQuestDetails(graph);
				AssetDatabase.Refresh();
			}
			catch (Exception e) {
				Debug.LogError($"{graph.name} quest graph export error: {e.Message}");
			}
		}

		private static QuestGraph[] LoadAllQuestGraphs() {
			string[] guids = AssetDatabase.FindAssets("t:QuestGraph");
			var list = new List<QuestGraph>();
			foreach (var g in guids) {
				var path = AssetDatabase.GUIDToAssetPath(g);
				var q = AssetDatabase.LoadAssetAtPath<QuestGraph>(path);
				if (q) list.Add(q);
			}
			return list.ToArray();
		}

		private static void GenerateQuestDetails(QuestGraph quest) {
			var sb = new StringBuilder();
			sb.AppendLine("// This is an auto-generated file. DO NOT MODIFY.");
			sb.AppendLine("namespace Bard.Quests {");
			string questName;
			try {
				questName = Sanitize(quest.name);
			}
			catch (Exception e) {
				Debug.LogError($"Quest name {quest.name} export error: " + e.Message);
				throw;
			}
			sb.AppendLine($"	public static class {questName} {{");

			sb.AppendLine("		public enum Steps {");
			sb.AppendLine("			Default = 0,");
			foreach (var s in quest.nodes.OfType<QuestStepNode>().OrderBy(s => s.position.y)) {
				try {
					sb.AppendLine($"			{Sanitize(s.Name)} = {s.Id},");
				}
				catch (Exception e) {
					Debug.LogError($"{quest.name} step {s.Id} export error: " + e.Message);
					throw;
				}
			}
			sb.AppendLine("		}");

			var cNodes = quest.nodes.OfType<QuestConditionsNode>();
			sb.AppendLine("		public enum Conditions {");
			foreach (var cNode in cNodes) {
				foreach (var c in cNode.Conditions.OrderBy(c => c.Id)) {
					try {
						sb.AppendLine($"			{Sanitize(c.Id)},");
					}
					catch (Exception e) {
						Debug.LogError($"{quest.name} condition {c.Id} export error: " + e.Message);
						throw;
					}
				}
			}
			sb.AppendLine("		}");

			sb.AppendLine("	}");
			sb.AppendLine("}");

			var path = Path.Combine(ClassPath, quest.name + ".cs");
			bool requireDefinitionInsert = !File.Exists(path);

			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

			if (requireDefinitionInsert) {
				var prefs = DialogueSystemPreferences.GetOrCreateSettings();
				prefs.Quests.AddDefinitionIfMissing(questName);
				EditorUtility.SetDirty(prefs.Quests);
				AssetDatabase.SaveAssets();
			}

			Debug.Log("Exported quest details for " + quest.name);
		}

		private static string Sanitize(string s) {
			if (string.IsNullOrEmpty(s)) {
				throw new Exception("Value cannot be empty");
			}
			s = Regex.Replace(s, @"[^A-Za-z0-9_]", "");
			return char.IsDigit(s[0]) ? "_" + s : s;
		}
	}
}
