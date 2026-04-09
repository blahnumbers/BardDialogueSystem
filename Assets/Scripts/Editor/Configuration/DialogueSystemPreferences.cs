using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bard.Configuration.Editor {
	[InitializeOnLoad]
	public static class DialogueSystemPreferences {
		private const string DefaultFolder = "Assets/Bard";
		private const string SettingsAssetName = "DialogueProjectSettings.asset";
		private const string FirstInstallKey = "BardDialogue.Installed";

		private static DialogueProjectSettings settings;
		private static readonly GUIContent guiContent = new("");

		static DialogueSystemPreferences() {
			EditorApplication.delayCall += () => {
				var settings = FindSettingsAsset();
				GetOrCreateSettings();

				if (settings == null) {
					SettingsService.OpenProjectSettings("Project/Bard Dialogue");
				}
			};
		}

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

			EditorGUI.BeginChangeCheck();
			var defaultAssetPath = so.FindProperty("DefaultAssetPath");
			var oldDefaultAssetPath = defaultAssetPath.stringValue;
			EditorGUILayout.DelayedTextField(defaultAssetPath);
			if (EditorGUI.EndChangeCheck()) {
				var newPath = defaultAssetPath.stringValue;
				Directory.CreateDirectory(newPath);

				if (!Directory.Exists(oldDefaultAssetPath)) {
					AssetDatabase.Refresh();
					return;
				}

				var hasGraphsDir = Directory.Exists(Path.Combine(oldDefaultAssetPath, "DialogueGraphs"));
				var hasQuestsDir = Directory.Exists(Path.Combine(oldDefaultAssetPath, "QuestGraphs"));
				if (hasGraphsDir || hasQuestsDir) {
					if (EditorUtility.DisplayDialog("Changing default assets path", "Would you like to move your existing graphs data to the new location?", "Yes", "No")) {
						if (hasGraphsDir) {
							Directory.Move(Path.Combine(oldDefaultAssetPath, "DialogueGraphs"), Path.Combine(newPath, "DialogueGraphs"));
						}
						if (hasQuestsDir) {
							Directory.Move(Path.Combine(oldDefaultAssetPath, "QuestGraphs"), Path.Combine(newPath, "QuestGraphs"));
						}
					}
				}
				if (Directory.GetDirectories(oldDefaultAssetPath).Length == 0 && Directory.GetFiles(oldDefaultAssetPath).Length == 0) {
					Directory.Delete(oldDefaultAssetPath);
				}
				AssetDatabase.Refresh();
			}
			EditorGUI.BeginChangeCheck();
			var dataGenPath = so.FindProperty("DataGenerationPath");
			var oldDataGenPath = dataGenPath.stringValue;
			EditorGUILayout.PropertyField(dataGenPath);
			if (EditorGUI.EndChangeCheck()) {
				if (!Directory.Exists(oldDataGenPath)) {
					AssetDatabase.Refresh();
					return;
				}

				if (Directory.GetDirectories(oldDataGenPath).Length > 0) {
					if (EditorUtility.DisplayDialog("Changing data generation path", "Would you like to move your existing data to the new location?", "Yes", "No")) {
						Directory.Move(oldDataGenPath, dataGenPath.stringValue);
					}
				}
				if (Directory.GetDirectories(oldDataGenPath).Length == 0 && Directory.GetFiles(oldDataGenPath).Length == 0) {
					Directory.Delete(oldDataGenPath);
				}
				AssetDatabase.Refresh();
			}

			guiContent.text = "Types Asset";
			EditorGUILayout.PropertyField(so.FindProperty("Messages"), guiContent);
			guiContent.text = "Actions Asset";
			EditorGUILayout.PropertyField(so.FindProperty("MessageActions"), guiContent);

			guiContent.text = "Definitions Asset";
			EditorGUILayout.PropertyField(so.FindProperty("Quests"), guiContent);
			guiContent.text = "Class Generation Path";
			EditorGUILayout.PropertyField(so.FindProperty("QuestClassGenerationPath"), guiContent);

			guiContent.text = "Characters Asset";
			EditorGUILayout.PropertyField(so.FindProperty("Characters"), guiContent);

			guiContent.text = "Localization Asset";
			EditorGUILayout.PropertyField(so.FindProperty("Localization"), guiContent);

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

		private static DialogueProjectSettings FindSettingsAsset() {
			string[] guids = AssetDatabase.FindAssets("t:DialogueProjectSettings");

			if (guids.Length == 0) return null;

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			return AssetDatabase.LoadAssetAtPath<DialogueProjectSettings>(path);
		}

		private static DialogueProjectSettings CreateSettingsAsset() {
			EnsureFolder(DefaultFolder);

			string path = Path.Combine(DefaultFolder, SettingsAssetName);

			var asset = ScriptableObject.CreateInstance<DialogueProjectSettings>();

			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();

			Debug.Log("Created DialogueProjectSettings at " + path);

			return asset;
		}

		private static void EnsureDefaultRegistries(DialogueProjectSettings settings) {
			bool dirty = false;
			var settingsPath = AssetDatabase.GetAssetPath(settings);
			var configPath = Path.Combine(Path.GetDirectoryName(settingsPath), "Config");

			dirty |= CreateOrInitializeRegistry(ref settings.Messages, configPath, "MessageTypes.asset");
			dirty |= CreateOrInitializeRegistry(ref settings.MessageActions, configPath, "MessageActions.asset");
			dirty |= CreateOrInitializeRegistry(ref settings.Quests, configPath, "QuestDefinitions.asset");
			dirty |= CreateOrInitializeRegistry(ref settings.Characters, configPath, "CharacterDefinitions.asset");
			dirty |= CreateOrInitializeRegistry(ref settings.Localization, configPath, "Localization.asset");

			if (dirty) {
				EditorUtility.SetDirty(settings);
				AssetDatabase.SaveAssets();
			}
		}

		private static bool CreateOrInitializeRegistry<T>(
			ref T registry, string configPath, string name)
			where T : ScriptableConfig {
			if (registry == null) {
				registry = CreateRegistry<T>(configPath, name);
				return true;
			}
			if (registry.Initialize()) {
				EditorUtility.SetDirty(registry);
				return true;
			}
			return false;
		}

		private static T CreateRegistry<T>(string configPath, string name) where T : ScriptableConfig {
			EnsureFolder(configPath);

			string path = Path.Combine(configPath, name);
			var asset = ScriptableObject.CreateInstance<T>();

			AssetDatabase.CreateAsset(asset, path);
			asset.Initialize();

			EditorUtility.SetDirty(asset);
			return asset;
		}

		private static void EnsureFolder(string path) {
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
