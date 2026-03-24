using UnityEngine;
using UnityEditor;
using System;
using Bard.Configuration.Editor;
using Bard.Editor;

namespace Bard.QuestSystem.Editor {
	[CustomPropertyDrawer(typeof(QuestRequirementTyped))]
	public class BardQuestRequirementTypedDrawer : PropertyDrawer {
		private static Rect m_Rect = new();
		private static readonly GUIContent m_IsFailureLabel = new("Is Failure");
		private static readonly string[] m_RequirementTypes = { "Step", "Condition" };

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			m_Rect.x = position.x;
			m_Rect.y = position.y + 2;
			m_Rect.width = position.width / 3;
			m_Rect.height = EditorGUIUtility.singleLineHeight;
			GUI.Label(m_Rect, "Id");

			var questPrefs = DialogueSystemPreferences.GetOrCreateSettings().Quests;
			m_Rect.x += m_Rect.width;
			m_Rect.width = position.width - m_Rect.width;
			var id = property.FindPropertyRelative("Id");
			id.intValue = BardEditorGUI.QuestPopup(m_Rect, id.intValue, questPrefs);

			m_Rect.x = position.x;
			m_Rect.y += m_Rect.height + 2;
			m_Rect.width = position.width / 3 - 4;
			var step = property.FindPropertyRelative("Step");
			var condition = property.FindPropertyRelative("Condition");
			var targetClass = QuestReflection.Get(questPrefs.GetById(id.intValue));
			if (targetClass == null || (targetClass.ConditionsNames.Length == 0 && targetClass.StepsNames.Length < 2)) {
				return;
			}

			if (targetClass.ConditionsNames.Length == 0 || targetClass.StepsNames.Length < 2) {
				if (targetClass.StepsNames.Length > 1 && step.intValue == 0) {
					step.intValue = 1;
					condition.stringValue = "";
				}
				else if (targetClass.ConditionsNames.Length > 0 && step.intValue > 0) {
					step.intValue = 0;
					condition.stringValue = targetClass.GetCondition(1);
				}
				GUI.enabled = false;
			}
			var currentChoice = (step.intValue == -1 || step.intValue > 0) ? 0 : 1;
			var newChoice = EditorGUI.Popup(m_Rect, currentChoice, m_RequirementTypes);
			GUI.enabled = true;

			if (currentChoice != newChoice) {
				if (newChoice == 0) {
					step.intValue = 1;
					condition.stringValue = "";
				}
				else {
					step.intValue = 0;
					condition.stringValue = targetClass.GetCondition(1);
				}
			}
			m_Rect.x += m_Rect.width + 4;
			m_Rect.width = position.width - m_Rect.width - 4;
			if (newChoice == 0) {
				var stepValue = targetClass.GetStepValue(step.intValue);
				step.intValue = Convert.ToInt32(EditorGUI.EnumFlagsField(m_Rect, stepValue));
			}
			else {
				var conditionValue = targetClass.GetConditionIndex(condition.stringValue);
				conditionValue = EditorGUI.Popup(m_Rect, conditionValue, targetClass.ConditionsNamesDisplay);
				condition.stringValue = targetClass.ConditionsNames[conditionValue];
			}

			var isFailure = property.FindPropertyRelative("IsFailure");
			m_Rect.x = position.x;
			m_Rect.y += m_Rect.height + 2;
			m_Rect.width = position.width;
			isFailure.boolValue = EditorGUI.ToggleLeft(m_Rect, m_IsFailureLabel, isFailure.boolValue);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 3 + 8;
		}
	}
}
