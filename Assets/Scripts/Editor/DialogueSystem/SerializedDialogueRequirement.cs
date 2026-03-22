using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Bard.XNodeEditor {
	public class SerializedDialogueRequirement {
		private static readonly Dictionary<string, SerializedDialogueRequirement> m_Cache = new();
		public SerializedProperty Property;
		public SerializedProperty Quests;
		public SerializedProperty Attitude;
		public SerializedProperty Custom;
		public bool AttitudeActive;
		public Vector2Int LastAttitude;
		public float CachedHeight = 0f;


		public bool IsAttitudeDefault => Attitude.vector2IntValue.x == int.MinValue || Attitude.vector2IntValue.y == int.MaxValue;
		public bool IsCustomDefault {
			get {
				bool isDefault = true;
				for (int i = 0; i < Custom.arraySize && isDefault; i++) {
					isDefault = string.IsNullOrEmpty(Custom.GetArrayElementAtIndex(i).stringValue);
				}
				return isDefault;
			}
		}
		public bool IsQuestsDefault {
			get {
				bool isDefault = true;
				for (int i = 0; i < Quests.arraySize && isDefault; i++) {
					var quest = Quests.GetArrayElementAtIndex(i);
					isDefault = quest.FindPropertyRelative("Id").intValue == 0;
				}
				return isDefault;
			}
		}
		public bool IsDefault => IsAttitudeDefault && IsCustomDefault && IsQuestsDefault;

		public SerializedDialogueRequirement(SerializedProperty prop) {
			Property = prop;
			Quests = prop.FindPropertyRelative("Quests");
			Attitude = prop.FindPropertyRelative("Attitude");
			Custom = prop.FindPropertyRelative("Custom");

			LastAttitude = Attitude.vector2IntValue;
			AttitudeActive = Attitude.vector2IntValue.x != int.MinValue || Attitude.vector2IntValue.y != int.MaxValue;

			Property.isExpanded = !IsDefault;
			Quests.isExpanded = !IsQuestsDefault;
			if (Quests.isExpanded) {
				for (int i = 0; i < Quests.arraySize; i++) {
					Quests.GetArrayElementAtIndex(i).isExpanded = true;
				}
			}
			Custom.isExpanded = !IsCustomDefault;
		}

		public static SerializedDialogueRequirement GetValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var value)) {
				value = new(property);
				m_Cache[id] = value;
			}
			return value;
		}

		public static void RemoveCache(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (m_Cache.ContainsKey(id)) {
				m_Cache.Remove(id);
			}
		}
	}
}
