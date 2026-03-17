using UnityEngine;

namespace Bard {
	public class DialogueProjectSettings : ScriptableObject {
		public DialogueMessageConfig Messages;
		public DialogueActionConfig MessageActions;
		public QuestConfig Quests;
	}
}
