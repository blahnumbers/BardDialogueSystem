using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using XNode;
using UnityEditor;
using XNodeEditor;
using System.Linq;

namespace Bard.XNodeEditor {
	[CustomNodeGraphEditor(typeof(DialogueGraph))]
	public class DialogueGraphContextMenu : NodeGraphEditor {
		public override void AddContextMenuItems(GenericMenu menu, Type[] types) {
			base.AddContextMenuItems(menu, new Type[] { typeof(DialogueNodeGroup), typeof(DialogueRootNode), typeof(DialogueNode), typeof(DialogueMessagesNode) });

			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Export/This graph only"), false, () => {
				var graphPath = AssetDatabase.GetAssetPath(target);
				var directories = Path.GetDirectoryName(graphPath).Replace("\\", "/").Split("/").ToList();
				var graphDirectory = directories.Last();
				if (directories.Count > 4 && directories[0] == "Assets" && directories[1] == "GameAssets" && directories[2] == "DialogueGraphs") {
					directories.RemoveRange(0, 3);
					graphDirectory = string.Join('_', directories);
				}
				string path = $"Assets/GameAssets/GameData/Dialogue/{graphDirectory}.json";
				string locPath = $"Assets/GameAssets/GameData/Localization/en/{graphDirectory}.json";

				DialogueGraphUtils.LocalizationCache.Clear();

				var trees = new List<DialogueTree>();
				var graphData = DialogueGraphUtils.Export((DialogueGraph)target);

				if (File.Exists(path)) {
					var existingData = File.ReadAllText(path);
					trees = JsonConvert.DeserializeObject<List<DialogueTree>>(existingData, DialogueTree.SerializerSettings);
					for (int i = 0; i < trees.Count; i++) {
						var replacement = graphData.Find(t => t.SharedID == trees[i].SharedID);
						if (replacement != null) {
							trees[i] = replacement;
							graphData.Remove(replacement);
							Debug.Log("Replacing dialogue tree: " + replacement.SharedID);
						}
					}
				}
				graphData.ForEach(t => {
					trees.Add(t);
					Debug.Log("Adding new dialogue tree: " + t.SharedID);
				});

				var json = JsonConvert.SerializeObject(trees, new JsonSerializerSettings() {
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = Formatting.Indented
				});
				File.WriteAllText(path, json);

				Debug.Log($"Exported graph to {path}");

				var localization = new List<LocalizationString>();
				if (File.Exists(locPath)) {
					var existingData = File.ReadAllText(locPath);
					localization = JsonConvert.DeserializeObject<List<LocalizationString>>(existingData);
					for (int i = 0; i < localization.Count; i++) {
						if (DialogueGraphUtils.LocalizationCache.TryGetValue(localization[i].Id, out var str)) {
							localization[i].String = str;
							DialogueGraphUtils.LocalizationCache.Remove(localization[i].Id);
						}
					}
				}
				foreach (var pair in DialogueGraphUtils.LocalizationCache) {
					localization.Add(new() { Id = pair.Key, String = pair.Value });
				}
				DialogueGraphUtils.LocalizationCache.Clear();

				json = JsonConvert.SerializeObject(localization, new JsonSerializerSettings() {
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = Formatting.Indented
				});
				File.WriteAllText(locPath, json);

				Debug.Log($"Exported localizations to {locPath}");

				DialogueGraphUtils.SaveGlobalLocalizations();

				AssetDatabase.Refresh();
			});
			menu.AddItem(new GUIContent("Export/All graphs in folder"), false, () => {
				var graphPath = AssetDatabase.GetAssetPath(target);
				var directories = Path.GetDirectoryName(graphPath).Replace("\\", "/").Split("/").ToList();
				var graphDirectory = directories.Last();
				if (directories.Count > 4 && directories[0] == "Assets" && directories[1] == "GameAssets" && directories[2] == "DialogueGraphs") {
					directories.RemoveRange(0, 3);
					graphDirectory = string.Join('_', directories);
				}

				string path = $"Assets/GameAssets/GameData/Dialogue/{graphDirectory}.json";
				string locPath = $"Assets/GameAssets/GameData/Localization/en/{graphDirectory}.json";
				if (File.Exists(path)) {
					if (!EditorUtility.DisplayDialog("Warning", "This action will rewrite all data in the specified file.\nAre you sure you want to continue?", "Continue", "Cancel")) {
						return;
					}
				}

				DialogueGraphUtils.LocalizationCache.Clear();
				var trees = new List<DialogueTree>();
				foreach (var file in Directory.GetFiles(Path.GetDirectoryName(graphPath), "*.asset")) {
					var graph = AssetDatabase.LoadAssetAtPath(file, typeof(DialogueGraph)) as DialogueGraph;
					Debug.Log("Exporting " + graph.name);
					trees.AddRange(DialogueGraphUtils.Export(graph));
				}
				var json = JsonConvert.SerializeObject(trees, new JsonSerializerSettings() {
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = Formatting.Indented
				});
				File.WriteAllText(path, json);

				List<LocalizationString> localization = new(DialogueGraphUtils.LocalizationCache.Count);
				foreach (var pair in DialogueGraphUtils.LocalizationCache) {
					localization.Add(new() { Id = pair.Key, String = pair.Value });
				}
				json = JsonConvert.SerializeObject(localization, new JsonSerializerSettings() {
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = Formatting.Indented
				});
				File.WriteAllText(locPath, json);
				Debug.Log($"Exported localizations to {locPath}");
				DialogueGraphUtils.LocalizationCache.Clear();

				DialogueGraphUtils.SaveGlobalLocalizations();


				Debug.Log($"Exported {trees.Count} graphs to {path}");
				AssetDatabase.Refresh();
			});
			menu.AddItem(new GUIContent("Build Addressables"), false, () => NodeGraphUtils.BuildAddressables());
		}
	}

	public static class DialogueGraphCreator {
		[MenuItem("Assets/Create/Bard Game/Dialogue Graph", false, 10)]
		public static void CreateDialogueGraph() {
			string selectedPath = NodeGraphUtils.GetSelectedPath();
			if (string.IsNullOrEmpty(selectedPath)) {
				selectedPath = "Assets/GameAssets/DialogueGraphs/";
			}

			string path = EditorUtility.SaveFilePanelInProject(
				"Creating New Dialogue Graph",
				"",
				"asset",
				"Enter a name for the new dialogue graph.",
				selectedPath
			);

			if (string.IsNullOrEmpty(path)) return;
			NodeGraphUtils.CreateGraphAtPath<DialogueGraph>(path);
		}
	}

	public class DialogueGraph : NodeGraph { }
}
