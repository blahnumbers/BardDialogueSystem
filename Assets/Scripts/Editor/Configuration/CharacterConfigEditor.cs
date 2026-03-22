using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace Bard.Configuration.Editor {
	[CustomEditor(typeof(CharacterConfig))]
	public class CharacterConfigEditor : UnityEditor.Editor {
		private CharacterConfig m_TargetConfig;
		private ReorderableList m_DefsList;
		private SerializedProperty m_Definitions;

		private void OnEnable() {
			m_TargetConfig = (CharacterConfig)serializedObject.targetObject;
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
					if (m_TargetConfig.Definitions[index].Id == 0) EditorGUI.BeginDisabledGroup(true);
					EditorGUI.PropertyField(rect, m_Definitions.GetArrayElementAtIndex(index));
					if (m_TargetConfig.Definitions[index].Id == 0) EditorGUI.EndDisabledGroup();
				},
				onAddCallback = list => {
					m_TargetConfig.AddDefinition("NPC-" + Guid.NewGuid().ToString("N"));
				},
				onCanRemoveCallback = list => {
					var targetIdx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					return m_TargetConfig.Definitions[targetIdx].Id > 0;
				},
				onRemoveCallback = list => {
					m_TargetConfig.DeleteDefinitionAtIndex(list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1);
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
