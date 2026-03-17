using UnityEngine;
using UnityEditor;
using XNode;
using System.Collections.Generic;
using XNodeEditor;
using UnityEditorInternal;
using Bard.Editor;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(QuestConditionsNode))]
	public class QuestConditionsNodeEditor : NodeEditor {
		private QuestConditionsNode m_Target;
		private SerializedProperty m_Conditions;
		private ReorderableList m_List;

		public override void OnCreate() {
			m_Target = target as QuestConditionsNode;
			m_Conditions = serializedObject.FindProperty("Conditions");
			m_List = new(serializedObject, m_Conditions) {
				drawHeaderCallback = (rect) => {
					GUI.Label(rect, m_Conditions.arraySize + " additional conditions");
				},
				drawElementCallback = (rect, index, active, focus) => {
					EditorGUI.PropertyField(rect, m_Conditions.GetArrayElementAtIndex(index));
				},
				elementHeightCallback = i => {
					return EditorGUI.GetPropertyHeight(m_Conditions.GetArrayElementAtIndex(i));
				},
				onAddCallback = list => {
					m_Target.Conditions.Add(new());
				},
				onRemoveCallback = list => {
					if (list.selectedIndices.Count == 0) {
						QuestConditionDrawer.RemoveCache(m_Target.Conditions[^1].GUID);
						m_Target.Conditions.RemoveAt(m_Target.Conditions.Count - 1);
					}
					else {
						foreach (var i in list.selectedIndices) {
							QuestConditionDrawer.RemoveCache(m_Target.Conditions[i].GUID);
							m_Target.Conditions[i] = null;
						}
						m_Target.Conditions.RemoveAll(c => c == null);
					}
					serializedObject.Update();
				},
				onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					foreach (var c in m_Target.Conditions) {
						QuestConditionDrawer.RemoveCache(c.GUID);
					}

					var condition = m_Target.Conditions[oldIndex];
					m_Target.Conditions.Remove(condition);
					m_Target.Conditions.Insert(newIndex, condition);
					serializedObject.Update();
				}
			};
		}

		public override void OnBodyGUI() {
			serializedObject.Update();
			m_List.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}

	[NodeWidth(380)]
	public class QuestConditionsNode : Node {
		//[Input] public QuestNode Input;

		public List<QuestCondition> Conditions = new();
		public override object GetValue(NodePort port) => null;

		public void Setup(List<QuestCondition> conditions) {
			name = "Quest Conditions";
			Conditions = new(conditions);
		}
	}
}
