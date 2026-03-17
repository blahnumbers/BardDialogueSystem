using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bard.Editor {
	[CustomPropertyDrawer(typeof(QuestDefinition))]
	public class QuestDefinitionDrawer : PropertyDrawer {
		private static readonly GUIStyle m_Style;
		static QuestDefinitionDrawer() {
			m_Style = new(GUI.skin.label) {
				alignment = TextAnchor.MiddleRight
			};
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var id = property.FindPropertyRelative("Id").intValue;
			if (id == 0) EditorGUI.BeginDisabledGroup(true);
			EditorGUI.LabelField(new Rect(position.x, position.y, 25f, position.height), id.ToString(), m_Style);
			var name = property.FindPropertyRelative("Name");
			name.stringValue = EditorGUI.TextField(new Rect(position.x + 30f, position.y + 1f, position.width - 30f, position.height - 2f), name.stringValue);
			if (id == 0) EditorGUI.EndDisabledGroup();
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
	}

	[CustomEditor(typeof(QuestConfig))]
	public class QuestConfigEditor : UnityEditor.Editor {
		private ReorderableList m_DefsList;
		private SerializedProperty m_Definitions;

		private void OnEnable() {
			m_Definitions = serializedObject.FindProperty("m_Definitions");
			m_DefsList = new(serializedObject, m_Definitions, true, true, true, true) {
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
				onAddCallback = list => {
					((QuestConfig)serializedObject.targetObject).AddDefinition();
				},
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
