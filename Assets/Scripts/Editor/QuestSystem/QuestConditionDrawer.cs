using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace Bard.QuestSystem.Editor {
	[CustomPropertyDrawer(typeof(QuestCondition))]
	public class QuestConditionDrawer : PropertyDrawer {
		private class SerializedQuestConditionProperties {
			public SerializedProperty Id;
			public SerializedProperty ShowsNotification;
			public SerializedProperty Description;
			public float CachedHeight = 0f;

			public SerializedQuestConditionProperties(SerializedProperty property) {
				Id = property.FindPropertyRelative("Id");
				ShowsNotification = property.FindPropertyRelative("ShowsNotification");
				Description = property.FindPropertyRelative("Description");
			}
		}

		private static readonly Dictionary<string, SerializedQuestConditionProperties> m_Cache = new();
		private static readonly GUIContent m_MessageContent = new("");
		private static Rect m_Rect = new();

		private SerializedQuestConditionProperties GetCachedValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("GUID").stringValue;
			if (!m_Cache.TryGetValue(id, out var target) || target.Id == null) {
				target = new(property);
				m_Cache[id] = target;
			}
			return target;
		}
		
		public static void RemoveCache(string id) {
			if (m_Cache.ContainsKey(id)) {
				m_Cache.Remove(id);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var data = GetCachedValue(property);
			m_Rect.x = position.x;
			m_Rect.width = 60;
			m_Rect.y = position.y + 3;
			m_Rect.height = EditorGUIUtility.singleLineHeight;
			GUI.Label(m_Rect, "Id");

			m_Rect.x += m_Rect.width;
			m_Rect.width = position.width - m_Rect.width;
			data.Id.stringValue = EditorGUI.TextField(m_Rect, data.Id.stringValue);

			m_Rect.x = position.x;
			m_Rect.width = position.width - 60;
			m_Rect.y += m_Rect.height + 2;
			GUI.Label(m_Rect, "Shows Notification");
			m_Rect.x = position.x + position.width - 24;
			m_Rect.width = 24;
			data.ShowsNotification.boolValue = EditorGUI.Toggle(m_Rect, data.ShowsNotification.boolValue);

			m_Rect.x = position.x;
			m_Rect.width = position.width;
			m_Rect.y += m_Rect.height + 2;
			GUI.Label(m_Rect, "Description");

			m_Rect.y += m_Rect.height;
			m_MessageContent.text = data.Description.stringValue;
			m_Rect.height = Mathf.Max(EditorGUIUtility.singleLineHeight * 2, GUI.skin.textArea.CalcHeight(m_MessageContent, position.width) + 8f);
			data.Description.stringValue = EditorGUI.TextArea(m_Rect, data.Description.stringValue, EditorStyles.textArea);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if (property == null) return EditorGUIUtility.singleLineHeight * 3 + 8f;
	
			var data = GetCachedValue(property);
			if (Event.current.type == EventType.Layout) {
				data.CachedHeight = EditorGUIUtility.singleLineHeight * 3 + 8f;
				if (data.Description != null) {
					m_MessageContent.text = data.Description.stringValue;
					data.CachedHeight += Mathf.Max(EditorGUIUtility.singleLineHeight * 2, GUI.skin.textArea.CalcHeight(m_MessageContent, 320) + 8f) + 2f;
				}
			}
			return data.CachedHeight;
		}
	}
}
