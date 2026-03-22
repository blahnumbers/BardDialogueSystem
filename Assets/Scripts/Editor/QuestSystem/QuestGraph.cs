using UnityEngine;
using System;
using System.IO;
using XNode;
using UnityEditor;
using XNodeEditor;

namespace Bard.XNodeEditor {
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
		[MenuItem("Assets/Create/Bard/New Quest Graph", false, 1)]
		public static void CreateDialogueGraph() {
			string selectedPath = NodeGraphUtils.GetSelectedPath();
			if (string.IsNullOrEmpty(selectedPath)) {
				selectedPath = QuestGraphUtils.ExportPath;
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
}
