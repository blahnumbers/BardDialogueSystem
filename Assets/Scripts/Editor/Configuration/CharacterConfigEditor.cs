using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bard.Configuration.Editor {
	[CustomEditor(typeof(CharacterConfig))]
	public class CharacterConfigEditor : UnityEditor.Editor {
		private CharacterConfig m_TargetConfig;
		private SerializedProperty m_Groups;
		private readonly List<ReorderableList> m_GroupLists = new();
		private bool m_StylesInitialized = false;
		private GUIStyle m_IdOffsetStyle;
		private bool m_RequireRebuild = true;

		private void OnEnable() {
			m_TargetConfig = (CharacterConfig)serializedObject.targetObject;
			m_Groups = serializedObject.FindProperty("m_DefinitionGroups");
			RebuildLists();
		}

		private void RebuildLists() {
			m_GroupLists.Clear();
			for (int i = 0; i < m_Groups.arraySize; i++) {
				m_GroupLists.Add(BuildGroupList(i));
			}
			m_RequireRebuild = false;
		}

		private ReorderableList BuildGroupList(int groupIndex) {
			var groupProp = m_Groups.GetArrayElementAtIndex(groupIndex);
			var defsProp = groupProp.FindPropertyRelative("m_Definitions");
			var group = m_TargetConfig.DefinitionGroups[groupIndex];
			
			return new(serializedObject, defsProp, true, true, true, true) {
				drawHeaderCallback = rect => {
					var initialX = rect.x;
					var fullWidth = rect.width;
					rect.width = 30f;
					if (groupIndex == 0) EditorGUI.BeginDisabledGroup(true);
					if (GUI.Button(rect, "▲")) {
						m_TargetConfig.SwapGroups(groupIndex, groupIndex - 1);
						m_RequireRebuild = true;
					}
					if (groupIndex == 0) EditorGUI.EndDisabledGroup();
					rect.x += rect.width;
					if (groupIndex == m_Groups.arraySize - 1) EditorGUI.BeginDisabledGroup(true);
					if (GUI.Button(rect, "▼")) {
						m_TargetConfig.SwapGroups(groupIndex, groupIndex + 1);
						m_RequireRebuild = true;
					}
					EditorGUI.EndDisabledGroup();

					rect.x += rect.width + 2f;
					var buttonWidth = Mathf.Clamp(fullWidth * 0.15f, 50f, 120f);
					rect.width = fullWidth - buttonWidth * 3f - rect.width * 2f - 10f;
					var groupName = groupProp.FindPropertyRelative("GroupName");
					groupName.stringValue = EditorGUI.TextField(rect, groupName.stringValue);
					rect.x += rect.width + 2f;
					rect.width = buttonWidth;

					// Initial group has immutable ID offset at 0
					if (groupIndex > 0) {
						EditorGUI.LabelField(rect, "ID offset", m_IdOffsetStyle);
						rect.x += rect.width + 2f;
						var startIndex = groupProp.FindPropertyRelative("StartIndex");
						EditorGUI.BeginChangeCheck();
						startIndex.intValue = EditorGUI.DelayedIntField(rect, startIndex.intValue);
						if (EditorGUI.EndChangeCheck()) {
							serializedObject.ApplyModifiedProperties();
							m_TargetConfig.VerifyIdIntegrity();
						}
					}

					rect.x = initialX + fullWidth - buttonWidth;
					if (GUI.Button(rect, "Reset ids")) {
						group.ResetDefinitionCounters();
					}
				},
				elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(defsProp.GetArrayElementAtIndex(index)),
				drawElementCallback = (rect, index, active, focused) => {
					var def = group.Definitions[index];
					var shouldDisable = groupIndex == 0 && def.Id == 0;
					if (shouldDisable) EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(rect, defsProp.GetArrayElementAtIndex(index), GUIContent.none);
					if (shouldDisable) EditorGUI.EndDisabledGroup();
				},
				onAddCallback = list => {
					group.AddDefinition("NPC-" + Guid.NewGuid().ToString("N"));
				},
				onCanRemoveCallback = list => {
					var idx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					return group.Definitions[idx].Id > 0;
				},
				onRemoveCallback = list => {
					var idx = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;
					group.DeleteDefinitionAtIndex(idx);
				}
			};
		}

		public override void OnInspectorGUI() {
			if (!m_StylesInitialized) {
				m_IdOffsetStyle = new(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
				m_StylesInitialized = true;
			}

			serializedObject.Update();

			if (m_RequireRebuild || m_GroupLists.Count != m_Groups.arraySize) {
				RebuildLists();
			}
			for (int i = 0; i < m_GroupLists.Count; i++) {
				EditorGUILayout.Space(4);
				m_GroupLists[i].DoLayoutList();
			}
			
			EditorGUILayout.Space();
			if (GUILayout.Button("Add Group")) {
				m_TargetConfig.AddGroup("New Group", m_TargetConfig.DefinitionGroups.Max(dg => dg.StartIndex) + 100);
				serializedObject.Update();
				RebuildLists();
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
