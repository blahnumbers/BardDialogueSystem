using System.Collections.Generic;
using UnityEditor;

namespace Bard.DialogueSystem.Editor {
	public class SerializedMessageAction {
		private static readonly Dictionary<string, SerializedMessageAction> m_Cache = new();
		public readonly string Id;
		public float CachedHeight = 0f;
		public readonly SerializedProperty Type;
		public readonly SerializedProperty SValue;
		public readonly SerializedProperty IValue;
		public readonly SerializedProperty CValue;
		public readonly SerializedProperty SkillCheck;

		public SerializedMessageAction(SerializedProperty property, string id) {
			Id = id;
			Type = property.FindPropertyRelative("Type");
			SValue = property.FindPropertyRelative("SValue");
			IValue = property.FindPropertyRelative("IValue");
			CValue = property.FindPropertyRelative("CValue");
			SkillCheck = property.FindPropertyRelative("SkillCheck");
		}

		public static SerializedMessageAction GetValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var value)) {
				value = new(property, id);
				m_Cache[id] = value;
			}
			return value;
		}

		public static void Remove(string id) {
			m_Cache.Remove(id);
		}
	}
}
