using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using XNode;

namespace Bard.XNodeEditor {
	public static class DialogueGraphUtils {
		public static Dictionary<string, DialogueTree> ExporterCachedDialogue = new();
		public static Dictionary<string, DialogueTree> ExporterCachedMessages = new();
		public static Dictionary<string, string> LocalizationCache = new();
		public static Dictionary<string, string> LocalizationCache_SkillChecks = new();
		public static List<string> ExporterUsedCachedMessages = new();
		public static List<string> ExporterUsedCachedDialogue = new();

		public static void Import(DialogueTree tree, DialogueGraph graph) {
			if (graph == null) {
				Debug.LogError("Please create a valid graph before importing data into it.");
				return;
			}
			var treesLookup = new Dictionary<string, DialogueNode>();
			var messagesLookup = new Dictionary<string, DialogueMessageBlockNode>();

			FillEmptyTreeFields(tree);
			var rootNode = CreateNodesRecursive(graph, tree, treesLookup, messagesLookup, true);
			PositionNodesRecursive(rootNode, 0, 0);
		}

		private static void FillEmptyTreeFields(DialogueTree tree) {
			if (string.IsNullOrEmpty(tree.SharedID)) {
				tree.SharedID = Guid.NewGuid().ToString("N");
			}
			if (string.IsNullOrEmpty(tree.SharedMessagesID)) {
				tree.SharedMessagesID = Guid.NewGuid().ToString("N");
			}
		}

		private static Node CreateNodesRecursive(
			DialogueGraph graph,
			DialogueTree tree,
			Dictionary<string, DialogueNode> trees,
			Dictionary<string, DialogueMessageBlockNode> messages,
			bool isRoot = false
		) {
			if (trees.ContainsKey(tree.SharedID)) return null;

			Node node;
			if (isRoot) {
				var n = graph.AddNode<DialogueRootNode>();
				AssetDatabase.AddObjectToAsset(n, graph);
				n.Setup(tree);
				node = n;
			}
			else {
				var n = graph.AddNode<DialogueNode>();
				AssetDatabase.AddObjectToAsset(n, graph);
				n.Setup(tree);
				trees[tree.SharedID] = n;
				node = n;
			}

			if (!string.IsNullOrEmpty(tree.MessagesID)) {
				if (messages.TryGetValue(tree.MessagesID, out var msgs)) {
					node.GetOutputPort("Output").Connect(msgs.GetInputPort("Input"));
				}
				else {
					Debug.LogError("Error finding Messages node with ID " + tree.MessagesID);
				}
			}
			else if (tree.Messages != null) {
				var messagesNode = graph.AddNode<DialogueMessageBlockNode>();
				AssetDatabase.AddObjectToAsset(messagesNode, graph);
				messagesNode.Setup(tree.Messages, tree.SharedMessagesID);
				messages[tree.SharedMessagesID] = messagesNode;
				node.GetOutputPort("Output").Connect(messagesNode.GetInputPort("Input"));
				for (int i = 0; i < tree.Messages.Length; i++) {
					if (tree.Messages[i].FollowUpDialogue != null) {
						FillEmptyTreeFields(tree.Messages[i].FollowUpDialogue);
						var newNode = CreateNodesRecursive(graph, tree.Messages[i].FollowUpDialogue, trees, messages);
						messagesNode.GetOutputPort($"{i}").Connect(newNode.GetInputPort("Input"));
						tree.Messages[i].FollowUpDialogueID = tree.Messages[i].FollowUpDialogue.SharedID;
					}
					else if (tree.Messages[i].FollowUpDialogueID != null && trees.TryGetValue(tree.Messages[i].FollowUpDialogueID, out var followUp)) {
						messagesNode.GetOutputPort($"{i}").Connect(followUp.GetInputPort("Input"));
					}
				}
			}
			return node;
		}

		private static float PositionNodesRecursive(Node node, int depth, float startY, float xSpacing = 550, float ySpacing = 450) {
			if (node == null || node.position != Vector2.zero) {
				return startY;
			}
			node.position = new Vector2(depth++ * xSpacing, startY);
			float nextY = startY;

			var port = node.GetOutputPort("Output");
			if (port != null && port.Connection != null) {
				var messagesNode = port.Connection.node as DialogueMessageBlockNode;
				if (messagesNode.position != Vector2.zero) {
					return nextY;
				}
				messagesNode.position = new Vector2(depth++ * xSpacing, startY);
				if (messagesNode.Messages.Length == 0) {
					return nextY + ySpacing;
				}
				for (int i = 0; i < messagesNode.Messages.Length; i++) {
					var nextPort = messagesNode.GetOutputPort($"{i}");
					if (nextPort != null) {
						nextY = PositionNodesRecursive(nextPort.Connection?.node, depth, nextY + (i * 300), xSpacing, ySpacing);
					}
				}
			}

			return nextY;
		}

		private static void PrepareExport() {
			ExporterCachedDialogue.Clear();
			ExporterCachedMessages.Clear();
			ExporterUsedCachedMessages.Clear();
			ExporterUsedCachedDialogue.Clear();
		}

		public static List<DialogueTree> Export(DialogueGraph graph) {
			PrepareExport();

			var rootNodes = graph.nodes.OfType<DialogueRootNode>().ToArray();
			if (rootNodes.Length == 0) {
				throw new Exception("Dialogue Graph export error: at least one root dialogue node must be present!");
			}

			List<DialogueTree> dialogueTrees = new(rootNodes.Length);
			foreach (var root in rootNodes) { 
				dialogueTrees.Add(root.Export());
				ExporterUsedCachedDialogue.Add(root.Id); // Root node always retains its id
			}
			for (int i = 1; i < dialogueTrees.Count; i++) {
				dialogueTrees[i].LinkedTreeId = dialogueTrees[0].SharedID;
			}

			// Perform clenaup for unused ids
			foreach (var dialogue in ExporterCachedDialogue) {
				if (ExporterUsedCachedDialogue.Contains(dialogue.Key)) continue;
				dialogue.Value.SharedID = null;
			}
			foreach (var message in ExporterCachedMessages) {
				if (ExporterUsedCachedMessages.Contains(message.Key)) continue;
				message.Value.SharedMessagesID = null;
			}
			return dialogueTrees;
		}

		private static void SaveLocalizationSingle(Dictionary<string, string> cache, string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));

			var localization = new List<LocalizationString>();
			if (File.Exists(path)) {
				var existingData = File.ReadAllText(path);
				localization = JsonConvert.DeserializeObject<List<LocalizationString>>(existingData);
				for (int i = 0; i < localization.Count; i++) {
					if (cache.TryGetValue(localization[i].Id, out var str)) {
						localization[i].String = str;
						cache.Remove(localization[i].Id);
					}
				}
			}
			foreach (var pair in cache) {
				localization.Add(new() { Id = pair.Key, String = pair.Value });
			}
			cache.Clear();

			var json = JsonConvert.SerializeObject(localization, new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			});
			File.WriteAllText(path, json);

			Debug.Log("Exported localization data to " + path);
		}

		public static void SaveGlobalLocalizations() {
			SaveLocalizationSingle(LocalizationCache_SkillChecks, "Assets/GameAssets/GameData/Localization/en/SkillChecks.json");
		}
	}
}
