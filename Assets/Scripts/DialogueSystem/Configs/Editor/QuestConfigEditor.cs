using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bard.Editor {
	[CustomPropertyDrawer(typeof(QuestDefinition))]
	public class QuestDefinitionDrawer : PropertyDrawer {
		private static readonly GUIStyle m_IdStyle;
		private static readonly GUIStyle m_NameStyle;
		static QuestDefinitionDrawer() {
			m_IdStyle = new(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			m_NameStyle = new(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var id = property.FindPropertyRelative("Id").intValue;
			var enabled = property.FindPropertyRelative("Enabled");
			var state = enabled.boolValue;
			if (id == 0 || !state) EditorGUI.BeginDisabledGroup(true);

			EditorGUI.LabelField(new Rect(position.x, position.y, 20f, position.height), id.ToString(), m_IdStyle);

			var name = property.FindPropertyRelative("InternalName");
			EditorGUI.LabelField(new Rect(position.x + 25f, position.y, 100f, position.height), name.stringValue, m_NameStyle);

			var dispName = property.FindPropertyRelative("EditorName");
			dispName.stringValue = EditorGUI.TextField(new Rect(position.x + 130f, position.y + 1f, position.width - 160f, position.height - 2f), dispName.stringValue);

			if (id == 0 || !state) EditorGUI.EndDisabledGroup();
			if (id > 0) {
				enabled.boolValue = EditorGUI.Toggle(new Rect(position.x + position.width - 25f, position.y, 20f, position.height), enabled.boolValue);
			}
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
	}

	[CustomEditor(typeof(QuestConfig))]
	public class QuestConfigEditor : UnityEditor.Editor {
		private ReorderableList m_DefsList;
		private SerializedProperty m_Definitions;

		private void OnEnable() {
			m_Definitions = serializedObject.FindProperty("m_Definitions");
			m_DefsList = new(serializedObject, m_Definitions, true, true, false, true) {
				drawHeaderCallback = rect => {
					var buttonWidth = Mathf.Clamp(rect.width * 0.4f, 70f, 120f);
					rect.width -= buttonWidth;
					EditorGUI.LabelField(rect, "Quest Definitions");
					rect.x += rect.width;
					rect.width = buttonWidth;
					if (GUI.Button(rect, "Reset ids")) {
						((QuestConfig)serializedObject.targetObject).ResetDefinitionCounters();
					}
				},
				drawElementCallback = (rect, index, active, focused) => {
					EditorGUI.PropertyField(rect, m_Definitions.GetArrayElementAtIndex(index));
				},
				// We currently only allow adding quests via graph export
				/*onAddCallback = list => {
					((QuestConfig)serializedObject.targetObject).AddDefinition();
				},*/
				onCanRemoveCallback = list => {
					if (list.count < 1) return false;
					var targetIdx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					return ((QuestConfig)serializedObject.targetObject).Definitions[targetIdx].Id > 0;
				},
				onRemoveCallback = list => {
					m_Definitions.DeleteArrayElementAtIndex(list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1);
				}
			};
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			m_DefsList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}
