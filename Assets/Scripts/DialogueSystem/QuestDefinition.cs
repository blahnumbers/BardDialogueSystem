

using System;

namespace Bard {
	[Serializable]
	public class QuestDefinition {
		public int Id;
		public string InternalName;
#if UNITY_EDITOR
		public string EditorName;
#endif
		public bool Enabled;
		public QuestDefinition(int id, string name, string displayName = null) {
			Id = id;
			InternalName = name;
			EditorName = displayName ?? name;
			Enabled = true;
		}
	}
}
