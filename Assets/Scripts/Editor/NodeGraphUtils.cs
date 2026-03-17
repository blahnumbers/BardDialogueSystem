
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using XNode;

namespace Bard.XNodeEditor {
	public class NodeGraphUtils {
		public static void SaveCurrent(NodeGraph graph) {
			EditorUtility.SetDirty(graph);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public static T CreateGraphAtPath<T>(string path) where T : NodeGraph {
			var graph = ScriptableObject.CreateInstance<T>();
			graph.name = Path.GetFileNameWithoutExtension(path);

			AssetDatabase.CreateAsset(graph, path);
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = graph;

			return graph;
		}

		public static string GetSelectedPath() {
			UnityEngine.Object obj = Selection.activeObject;
			if (obj == null) return null;

			string path = AssetDatabase.GetAssetPath(obj);

			if (AssetDatabase.IsValidFolder(path)) return path;

			if (!string.IsNullOrEmpty(path)) {
				return Path.GetDirectoryName(path).Replace("\\", "/");
			}
			return null;
		}

		public static void Clear(NodeGraph graph, bool save = true) {
			var path = AssetDatabase.GetAssetPath(graph);
			var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

			graph.Clear();
			foreach (var obj in subAssets) {
				if (obj != graph) UnityEngine.Object.DestroyImmediate(obj, true);
			}

			if (save) {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		public static bool BuildAddressables() {
			AddressableAssetSettings.BuildPlayerContent(out var result);
			if (!string.IsNullOrEmpty(result.Error)) {
				Debug.LogError("Error building addressables: " + result.Error);
				return false;
			}
			return true;
		}
	}
}
