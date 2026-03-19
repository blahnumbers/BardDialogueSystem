using System.IO;
using UnityEngine;

namespace Bard {
	public class DialogueProjectSettings : ScriptableObject {
		[Header("General Settings")]
		public string DefaultAssetPath = "Assets/Bard/GameAssets";
		public string DataGenerationPath = "Assets/Bard/GameAssets/GameData";
		public string QuestDataGenerationPath => Path.Combine(DataGenerationPath, "Quests.json");

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
