using System;

namespace Bard.QuestSystem {
	[Serializable]
	public class QuestDefinitionBase {
		public int Id;
		public string InternalName;
#if UNITY_EDITOR
		public string EditorName;
#endif
		public QuestDefinitionBase(int id, string name, string displayName = null) {
			Id = id;
			InternalName = name;
			EditorName = displayName ?? name;
		}
	}

	[Serializable]
	public class QuestDefinition : QuestDefinitionBase {
		public bool Enabled;
		public QuestDefinition(int id, string name, string displayName = null) : base(id, name, displayName) {
			Enabled = true;
		}
	}

	[Serializable]
	public class QuestTypeDefinition : QuestDefinitionBase {
		public QuestTypeDefinition(int id, string name, string displayName = null) : base(id, name, displayName) { }
	}
}
