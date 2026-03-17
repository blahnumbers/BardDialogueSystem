using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bard.Editor {
	[CustomEditor(typeof(DialogueMessageConfig))]
	public class DialogueMessageConfigEditor : UnityEditor.Editor {
		private ReorderableList m_TypesList;
		private SerializedProperty m_Types;

		private void OnEnable() {
			m_Types = serializedObject.FindProperty("Types");
			m_TypesList = new(serializedObject, m_Types, true, true, true, true) {
				drawHeaderCallback = rect => {
					var buttonWidth = Mathf.Clamp(rect.width * 0.4f, 70f, 120f);
					rect.width -= buttonWidth;
					EditorGUI.LabelField(rect, "Message Types");
					rect.x += rect.width;
					rect.width = buttonWidth;
					if (GUI.Button(rect, "Reset ids")) {
						((DialogueMessageConfig)serializedObject.targetObject).ResetTypeCounters();
					}
				},
				drawElementCallback = (rect, index, active, focused) => {
					EditorGUI.PropertyField(rect, m_Types.GetArrayElementAtIndex(index));
				},
				onCanRemoveCallback = list => {
					if (list.count < 2) return false;
					var targetIdx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					return ((DialogueMessageConfig)serializedObject.targetObject).Types[targetIdx].Id != 0;
				},
				onAddCallback = list => {
					((DialogueMessageConfig)serializedObject.targetObject).AddType();
					serializedObject.Update();
				},
				onCanAddCallback = list => ((DialogueMessageConfig)serializedObject.targetObject).NextTypeId < 1000,
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
