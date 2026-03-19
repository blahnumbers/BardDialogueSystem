using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bard.Editor {
	[CustomPropertyDrawer(typeof(NPCDefinition))]
	public class NPCDefinitionDrawer : PropertyDrawer {
		private static readonly GUIStyle m_IdStyle;
		private float m_CachedHeight = EditorGUIUtility.singleLineHeight * 3f + 8f;
		static NPCDefinitionDrawer() {
			m_IdStyle = new(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var id = property.FindPropertyRelative("Id").intValue;
			if (id == 0) EditorGUI.BeginDisabledGroup(true);

			EditorGUI.LabelField(new Rect(position.x, position.y, 20f, EditorGUIUtility.singleLineHeight), id.ToString(), m_IdStyle);

			var name = property.FindPropertyRelative("InternalName");
			name.stringValue = EditorGUI.TextField(new Rect(position.x + 25f, position.y, position.width - 25f, EditorGUIUtility.singleLineHeight), name.stringValue);

			EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("EditorName"));
			EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2f + 4f, position.width, position.height - EditorGUIUtility.singleLineHeight * 2f - 6f), property.FindPropertyRelative("DisplayNames"));

			if (id == 0) EditorGUI.EndDisabledGroup();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if (Event.current.type == EventType.Layout) {
				m_CachedHeight = EditorGUIUtility.singleLineHeight * 2f + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("DisplayNames")) + 8f;
			}
			return m_CachedHeight;
		}
	}

	[CustomEditor(typeof(CharacterConfig))]
	public class CharacterConfigEditor : UnityEditor.Editor {
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
						((CharacterConfig)serializedObject.targetObject).ResetDefinitionCounters();
					}
				},
				elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(m_Definitions.GetArrayElementAtIndex(index)),
				drawElementCallback = (rect, index, active, focused) => {
					EditorGUI.PropertyField(rect, m_Definitions.GetArrayElementAtIndex(index));
				},
				onAddCallback = list => {
					((CharacterConfig)serializedObject.targetObject).AddDefinition();
				},
				onCanRemoveCallback = list => {
					if (list.count < 1) return false;
					var targetIdx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					return ((CharacterConfig)serializedObject.targetObject).Definitions[targetIdx].Id > 0;
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
