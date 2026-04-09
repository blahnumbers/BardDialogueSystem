using UnityEngine;
using System.IO;

namespace Bard.Configuration.Editor {
	public class DialogueProjectSettings : ScriptableObject {
		[Header("General Settings")]
		public string DefaultAssetPath = "Assets/Bard";
		public string DataGenerationPath = "Assets/Bard/GameData";
		public string QuestDataGenerationPath => Path.Combine(DataGenerationPath, "Quests.json");
		public string DialogueGraphsPath => Path.Combine(DefaultAssetPath, "DialogueGraphs");
		public string QuestsGraphsPath => Path.Combine(DefaultAssetPath, "QuestGraphs");

		[Header("Dialogue Messages Settings")]
		public DialogueMessageConfig Messages;
		public DialogueActionConfig MessageActions;

		[Header("Quests Settings")]
		public QuestConfig Quests;
		public string QuestClassGenerationPath = "Assets/Scripts/Bard/Generated";

		[Header("NPC Settings")]
		public CharacterConfig Characters;

		[Header("Localization Settings")]
		public string LocalizationBasePath => Path.Combine(DataGenerationPath, "Localization");
		public LocalizationConfig Localization;
	}
}
