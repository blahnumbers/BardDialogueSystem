using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bard.Editor {
	public static class DialogueSystemPreferences {
		const string DefaultFolder = "Assets/Bard";
		const string SettingsAssetName = "DialogueProjectSettings.asset";

		static DialogueProjectSettings settings;

		[SettingsProvider]
		public static SettingsProvider CreateProvider() {
			return new SettingsProvider("Project/Bard Dialogue", SettingsScope.Project) {
				label = "Bard Dialogue",
				guiHandler = DrawGUI,
				keywords = new HashSet<string>() { "Bard", "dialogue", "npc", "quest", "message" }
			};
		}

		static void DrawGUI(string searchContext) {
			var settings = GetOrCreateSettings();

			SerializedObject so = new(settings);

			EditorGUILayout.PropertyField(so.FindProperty("DefaultAssetPath"));
			EditorGUILayout.PropertyField(so.FindProperty("DataGenerationPath"));
		
			EditorGUILayout.PropertyField(so.FindProperty("Messages"), new GUIContent("Types Asset"));
			EditorGUILayout.PropertyField(so.FindProperty("MessageActions"), new GUIContent("Actions Asset"));

			EditorGUILayout.PropertyField(so.FindProperty("Quests"), new GUIContent("Definitions Asset"));
			EditorGUILayout.PropertyField(so.FindProperty("QuestClassGenerationPath"), new GUIContent("Class Generation Path"));

			so.ApplyModifiedProperties();
		}

		public static DialogueProjectSettings GetOrCreateSettings() {
			if (settings != null) return settings;

			settings = FindSettingsAsset();
			if (settings == null) {
				settings = CreateSettingsAsset();
			}

			EnsureDefaultRegistries(settings);
			return settings;
		}

		static DialogueProjectSettings FindSettingsAsset() {
			string[] guids = AssetDatabase.FindAssets("t:DialogueProjectSettings");

			if (guids.Length == 0) return null;

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			return AssetDatabase.LoadAssetAtPath<DialogueProjectSettings>(path);
		}

		static DialogueProjectSettings CreateSettingsAsset() {
			EnsureFolder(DefaultFolder);

			string path = Path.Combine(DefaultFolder, SettingsAssetName);

			var asset = ScriptableObject.CreateInstance<DialogueProjectSettings>();

			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();

			Debug.Log("Created DialogueProjectSettings at " + path);

			return asset;
		}

		static void EnsureDefaultRegistries(DialogueProjectSettings settings) {
			bool dirty = false;

			if (settings.Messages == null) {
				settings.Messages = CreateRegistry<DialogueMessageConfig>("MessageTypes.asset");
				dirty = true;
			}
			if (settings.MessageActions == null) {
				settings.MessageActions = CreateRegistry<DialogueActionConfig>("MessageActions.asset");
				dirty = true;
			}
			if (settings.Quests == null) {
				settings.Quests = CreateRegistry<QuestConfig>("QuestDefinitions.asset");
				dirty = true;
			}

			if (dirty) {
				EditorUtility.SetDirty(settings);
				AssetDatabase.SaveAssets();
			}
		}

		static T CreateRegistry<T>(string name) where T : ScriptableConfig {
			EnsureFolder(DefaultFolder + "/Config");
			string path = DefaultFolder + "/Config/" + name;
			var asset = ScriptableObject.CreateInstance<T>();

			AssetDatabase.CreateAsset(asset, path);
			asset.Initialize(path);

			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();

			return asset;
		}

		static void EnsureFolder(string path) {
			if (!AssetDatabase.IsValidFolder(path)) {
				string parent = Path.GetDirectoryName(path);
				string folder = Path.GetFileName(path);

				if (!AssetDatabase.IsValidFolder(parent)) {
					EnsureFolder(parent);
				}

				AssetDatabase.CreateFolder(parent, folder);
			}
		}
	}
}
