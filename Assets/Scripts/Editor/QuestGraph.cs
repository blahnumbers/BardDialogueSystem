using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using XNode;
using UnityEditor;
using XNodeEditor;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
/*
[CustomNodeGraphEditor(typeof(QuestGraph))]
public class QuestGraphContextMenu : NodeGraphEditor {
	public override void AddContextMenuItems(GenericMenu menu, Type[] types) {
		base.AddContextMenuItems(menu, new Type[] { typeof(QuestNode), typeof(QuestStepNode), typeof(QuestConditionsNode) });

		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Export/Quest data (this graph)"), false, () => QuestGraphUtils.ExportQuestData(target));
		menu.AddItem(new GUIContent("Export/Quest data (all graphs)"), false, () => QuestGraphUtils.ExportQuestDataAll(target));
		menu.AddSeparator("Export/");
		menu.AddItem(new GUIContent("Export/Definitions (this graph)"), false, () => QuestGraphUtils.GenererateDefinitions(target as QuestGraph));
		menu.AddItem(new GUIContent("Export/Definitions (all graphs)"), false, () => QuestGraphUtils.GenerateAllDefinitions());
		menu.AddSeparator("Export/");
		menu.AddItem(new GUIContent("Export/Complete (this graph)"), false, () => {
			QuestGraphUtils.ExportQuestData(target);
			QuestGraphUtils.GenererateDefinitions(target as QuestGraph);
		});
		menu.AddItem(new GUIContent("Export/Complete (all graphs)"), false, () => {
			QuestGraphUtils.ExportQuestDataAll(target);
			QuestGraphUtils.GenerateAllDefinitions();
		});
		menu.AddItem(new GUIContent("Build Addressables"), false, () => NodeGraphUtils.BuildAddressables());
	}
}

public static class QuestGraphCreator {
	[MenuItem("Assets/Create/Bard Game/Quest Graph", false, 10)]
	public static void CreateDialogueGraph() {
		string selectedPath = NodeGraphUtils.GetSelectedPath();
		if (string.IsNullOrEmpty(selectedPath)) {
			selectedPath = "Assets/GameAssets/QuestGraphs/";
		}
		string path = EditorUtility.SaveFilePanelInProject(
			"Creating New Quest Graph",
			"",
			"asset",
			"Enter a name for the new quest graph.",
			selectedPath
		);

		if (string.IsNullOrEmpty(path)) return;
		NodeGraphUtils.CreateGraphAtPath<QuestGraph>(path);
	}

	public static QuestGraph CreateGraphAtPath(string path) {
		var graph = ScriptableObject.CreateInstance<QuestGraph>();
		graph.name = Path.GetFileNameWithoutExtension(path);

		AssetDatabase.CreateAsset(graph, path);
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();
		Selection.activeObject = graph;

		return graph;
	}
}

public class QuestGraph : NodeGraph { }

public class QuestGraphUtils {
	private const string BasePath = "Assets/Scripts/Generated";
	private const string DefinitionsPath = "Assets/Scripts/Generated/BardQuestDefinitions.cs";
	public static BardQuest Export(QuestGraph graph) {
		var rootNode = graph.nodes.OfType<QuestNode>().First();
		BardQuest quest = new() {
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
		string path = "Assets/GameAssets/GameData/Quests.json";
		List<BardQuest> quests = new();
		var graphData = new List<BardQuest>(1) { Export((QuestGraph)target) };

		if (File.Exists(path)) {
			var existingData = File.ReadAllText(path);
			quests = JsonConvert.DeserializeObject<List<BardQuest>>(existingData);
			for (int i = 0; i < quests.Count; i++) {
				var replacement = graphData.Find(t => t.Id == quests[i].Id);
				if (replacement != null) {
					quests[i] = replacement;
					graphData.Remove(replacement);
					Debug.Log("Replacing quest: " + replacement.Id);
				}
			}
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
		string path = "Assets/GameAssets/GameData/Quests.json";
		if (File.Exists(path)) {
			if (!EditorUtility.DisplayDialog("Warning", "This action will rewrite all data in the Quests data file.\nAre you sure you want to continue?", "Continue", "Cancel")) {
				return;
			}
		}

		List<BardQuest> quests = new();
		var graphPath = AssetDatabase.GetAssetPath(target);
		var graphDirectory = Path.GetDirectoryName(graphPath).Replace("\\", "/").Split("/").Last();
		foreach (var file in Directory.GetFiles(Path.GetDirectoryName(graphPath), "*.asset")) {
			var graph = AssetDatabase.LoadAssetAtPath(file, typeof(QuestGraph)) as QuestGraph;
			Debug.Log("Exporting " + graph.name);
			quests.Add(Export(graph));
		}
		quests.Sort((a, b) => (int)a.Type - (int)b.Type);
		var json = JsonConvert.SerializeObject(quests, new JsonSerializerSettings() {
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.Indented
		});
		File.WriteAllText(path, json);

		Debug.Log($"Exported {quests.Count} graphs to {path}");
		AssetDatabase.Refresh();
	}

	public static void Import(BardQuest quest, QuestGraph graph) {
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
		GenerateQuestIdEnum(quests);
		GenerateQuestDetails(quests);
		AssetDatabase.Refresh();
	}

	public enum GenerateMode {
		All,
		QuestIds,
		QuestDetails
	}

	public static void GenererateDefinitions(QuestGraph graph, GenerateMode mode = GenerateMode.All) {
		var quests = new[] { graph };
		if (mode == GenerateMode.All || mode == GenerateMode.QuestIds) {
			GenerateQuestIdEnum(quests);
		}
		if (mode == GenerateMode.All || mode == GenerateMode.QuestDetails) {
			GenerateQuestDetails(quests);
		}
		AssetDatabase.Refresh();
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

	private static void GenerateQuestIdEnum(QuestGraph[] quests) {
		string existing = File.Exists(DefinitionsPath) ? File.ReadAllText(DefinitionsPath) : "";

		var match = Regex.Match(existing, @"enum\s+BardQuestId\s*{([^}]*)}");
		var existingBlock = match.Success ? match.Groups[1].Value : "";
		var existingEntries = new Dictionary<string, string>();
		foreach (Match m in Regex.Matches(existingBlock, @"(\[InspectorName\("".+""\)\] )?([A-Za-z_][A-Za-z0-9_]*)")) {
			existingEntries.Add(m.Groups[2].Value, m.Groups[1].Value + m.Groups[2].Value);
		}

		var sb = new StringBuilder();
		sb.AppendLine("// This is an auto-generated file. DO NOT MODIFY.");
		sb.AppendLine("using UnityEngine;");
		sb.AppendLine("namespace Bard.Quests {");
		sb.AppendLine("	public enum BardQuestId {");

		// Fill with existing enum values - quests are immutable once they're in the system to prevent broken references
		foreach (var e in existingEntries) {
			sb.AppendLine($"		{e.Value},");
		}

		// Append any new values
		int addedDefinitions = 0;
		foreach (var q in quests) {
			string name = q.name;
			if (!existingEntries.ContainsKey(name)) {
				var questNode = q.nodes.Find(node => node is QuestNode) as QuestNode;
				string inspectorName = questNode != null ? $"[InspectorName(\"{questNode.Type}/{questNode.name}\")] " : string.Empty;
				sb.AppendLine($"		{inspectorName}{name},");
				addedDefinitions++;
			}
		}
		sb.AppendLine("	}");
		sb.AppendLine("}");

		if (existing != sb.ToString()) {
			File.WriteAllText(DefinitionsPath, sb.ToString(), Encoding.UTF8);
			Debug.Log($"Updated BardQuestId definitions with {addedDefinitions} new value(s).");
		}
	}

	private static void GenerateQuestDetails(QuestGraph[] quests) {
		var sb = new StringBuilder();
		foreach (var q in quests) {
			sb.Clear();
			sb.AppendLine("// This is an auto-generated file. DO NOT MODIFY.");
			sb.AppendLine("namespace Bard.Quests {");
			string questName;
			try {
				questName = Sanitize(q.name);
			}
			catch (Exception e) {
				Debug.LogError($"Quest name {q.name} export error: " + e.Message);
				return;
			}
			sb.AppendLine($"	public static class {questName} {{");

			sb.AppendLine("		public enum Steps {");
			sb.AppendLine("			Default = 0,");
			foreach (var s in q.nodes.OfType<QuestStepNode>().OrderBy(s => s.position.y)) {
				try {
					sb.AppendLine($"			{Sanitize(s.Name)} = {s.Id},");
				}
				catch (Exception e) {
					Debug.LogError($"{q.name} step {s.Id} export error: " + e.Message);
					return;
				}
			}
			sb.AppendLine("		}");

			var cNodes = q.nodes.OfType<QuestConditionsNode>();
			sb.AppendLine("		public enum Conditions {");
			foreach (var cNode in cNodes) {
				foreach (var c in cNode.Conditions.OrderBy(c => c.Id)) {
					try {
						sb.AppendLine($"			{Sanitize(c.Id)},");
					}
					catch (Exception e) {
						Debug.LogError($"{q.name} condition {c.Id} export error: " + e.Message);
						return;
					}
				}
			}
			sb.AppendLine("		}");

			sb.AppendLine("	}");
			sb.AppendLine("}");

			var path = Path.Combine(BasePath, q.name + ".cs");
			File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
			Debug.Log("Exported quest details for " + q.name);
		}
	}

	private static string Sanitize(string s) {
		if (string.IsNullOrEmpty(s)) {
			throw new Exception("Value cannot be empty");
		}
		s = Regex.Replace(s, @"[^A-Za-z0-9_]", "");
		return char.IsDigit(s[0]) ? "_" + s : s;
	}

	private static int Hash(string s) => s.GetHashCode();
}
*/
