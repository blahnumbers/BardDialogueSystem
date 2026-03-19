

using System;

namespace Bard {
	[Serializable]
	public class NPCDefinition {
		public int Id;
		public string InternalName;
#if UNITY_EDITOR
		public string EditorName;
#endif
		public string[] DisplayNames;
		public NPCDefinition(int id, string name, string displayName = null) {
			Id = id;
			InternalName = name;
			EditorName = displayName ?? name;
			DisplayNames = new[] { name };
		}
	}
}
