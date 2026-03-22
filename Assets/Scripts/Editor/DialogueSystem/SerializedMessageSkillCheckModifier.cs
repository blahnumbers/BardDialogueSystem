using UnityEditor;
using System.Collections.Generic;

namespace Bard.XNodeEditor {
	public class SerializedMessageSkillCheckModifier {
		private static readonly Dictionary<string, SerializedMessageSkillCheckModifier> m_Cache = new();

		public readonly string Id;
		public SerializedProperty Message;
		public SerializedProperty Modifier;
		public SerializedProperty Quest;
		public SerializedProperty TargetStep;
		public SerializedProperty TargetCondition;

		public SerializedMessageSkillCheckModifier(SerializedProperty property, string id) {
			Id = id;
			Message = property.FindPropertyRelative("Message");
			Modifier = property.FindPropertyRelative("Modifier");
			Quest = property.FindPropertyRelative("Quest");
			TargetStep = property.FindPropertyRelative("TargetStep");
			TargetCondition = property.FindPropertyRelative("TargetCondition");
		}

		public static SerializedMessageSkillCheckModifier GetValue(SerializedProperty property) {
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
