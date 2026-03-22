using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Bard.DialogueSystem;

namespace Bard.Configuration.Editor {
	[CustomPropertyDrawer(typeof(DialogueActionConfigEntry))]
	public class DialogueActionConfigEntryDrawer : PropertyDrawer {
		private static readonly GUIStyle m_Style;
		static DialogueActionConfigEntryDrawer() {
			m_Style = new(GUI.skin.label) {
				alignment = TextAnchor.MiddleRight
			};
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var id = property.FindPropertyRelative("Id").intValue;
			if (id == 0) EditorGUI.BeginDisabledGroup(true);
			EditorGUI.LabelField(new Rect(position.x, position.y, 25f, position.height), id.ToString(), m_Style);
			EditorGUI.ObjectField(new Rect(position.x + 30f, position.y + 1f, position.width - 30f, position.height - 2f), property.FindPropertyRelative("Action"), typeof(DialogueAction), GUIContent.none);
			if (id == 0) EditorGUI.EndDisabledGroup();
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
	}

	[CustomEditor(typeof(DialogueActionConfig))]
	public class DialogueActionConfigEditor : UnityEditor.Editor {
		private ReorderableList m_TypesList;
		private SerializedProperty m_Types;

		private void OnEnable() {
			m_Types = serializedObject.FindProperty("m_Types");
			m_TypesList = new(serializedObject, m_Types, true, true, true, true) {
				drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Action Types"),
				drawElementCallback = (rect, index, active, focused) => {
					EditorGUI.PropertyField(rect, m_Types.GetArrayElementAtIndex(index));
				},
				onAddCallback = list => {
					((DialogueActionConfig)serializedObject.targetObject).AddType();
				},
				onCanRemoveCallback = list => {
					if (list.count < 4) return false;
					var targetIdx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					return ((DialogueActionConfig)serializedObject.targetObject).Types[targetIdx].Id > 3;
				},
				onRemoveCallback = list => {
					m_Types.DeleteArrayElementAtIndex(list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1);
				}
			};
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			m_TypesList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}
