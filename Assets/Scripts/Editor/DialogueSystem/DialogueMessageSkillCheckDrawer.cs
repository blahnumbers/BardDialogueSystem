using UnityEngine;
using UnityEditor;
using System;
using Bard.Configuration.Editor;
using Bard.QuestSystem.Editor;
using Bard.XNodeEditor;
using Bard.Editor;

namespace Bard.DialogueSystem.Editor {
	[CustomPropertyDrawer(typeof(DialogueMessageSkillCheckModifier))]
	public class DialogueMessageSkillCheckModifierDrawer : PropertyDrawer {
		private static readonly string[] m_RequirementTypes = { "Step", "Condition" };
		private static Rect m_MessageRect;
		private static Rect m_MessageLabelRect;
		private static Rect m_ModifierRect;
		private static Rect m_ModifierLabelRect;
		private static Rect m_QuestRect;
		private static Rect m_QuestLabelRect;
		private static Rect m_TargetTypeRect;
		private static Rect m_TargetInputRect;

		static DialogueMessageSkillCheckModifierDrawer() {
			m_MessageRect = new(0, EditorGUIUtility.singleLineHeight, 0, EditorGUIUtility.singleLineHeight);
			m_MessageLabelRect = new(0, 0, 0, EditorGUIUtility.singleLineHeight);
			m_ModifierRect = new(82f, EditorGUIUtility.singleLineHeight * 2 + 2f, 0, EditorGUIUtility.singleLineHeight);
			m_ModifierLabelRect = new(0, EditorGUIUtility.singleLineHeight * 2 + 2f, 80f, EditorGUIUtility.singleLineHeight);
			m_QuestRect = new(82f, EditorGUIUtility.singleLineHeight * 3 + 4f, 0, EditorGUIUtility.singleLineHeight);
			m_QuestLabelRect = new(0, EditorGUIUtility.singleLineHeight * 3 + 4f, 80f, EditorGUIUtility.singleLineHeight);
			m_TargetTypeRect = new(0, EditorGUIUtility.singleLineHeight * 4 + 6f, 80f, EditorGUIUtility.singleLineHeight);
			m_TargetInputRect = new(82f, EditorGUIUtility.singleLineHeight * 4 + 6f, 0, EditorGUIUtility.singleLineHeight);
		}

		private void SetRects(Rect position) {
			m_MessageLabelRect.height = m_MessageRect.height = m_ModifierRect.height = m_ModifierLabelRect.height = m_QuestLabelRect.height = m_QuestRect.height = m_TargetTypeRect.height = m_TargetInputRect.height = EditorGUIUtility.singleLineHeight;

			m_MessageRect.x = m_MessageLabelRect.x = m_ModifierLabelRect.x = m_QuestLabelRect.x = m_TargetTypeRect.x = position.x;
			m_MessageRect.width = m_MessageLabelRect.width = position.width;

			m_ModifierLabelRect.width = m_QuestLabelRect.width = m_TargetTypeRect.width = 80f;
			m_ModifierRect.width = m_QuestRect.width = m_TargetInputRect.width = position.width - m_ModifierLabelRect.width - 2f;
			m_ModifierRect.x = m_QuestRect.x = m_TargetInputRect.x = position.x + m_ModifierLabelRect.width + 2f;

			m_MessageLabelRect.y = position.y;
			m_MessageRect.y = m_MessageLabelRect.y + m_MessageLabelRect.height;
			m_ModifierLabelRect.y = m_ModifierRect.y = m_MessageRect.y + m_MessageRect.height + 2f;
			m_QuestLabelRect.y = m_QuestRect.y = m_ModifierRect.y + m_ModifierRect.height + 2f;
			m_TargetInputRect.y = m_TargetTypeRect.y = m_QuestRect.y + m_QuestRect.height + 2f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var check = SerializedMessageSkillCheckModifier.GetValue(property);
			if (check == null) return;
			SetRects(position);

			EditorGUI.LabelField(m_MessageLabelRect, "Description");
			check.Message.stringValue = EditorGUI.TextField(m_MessageRect, check.Message.stringValue);

			EditorGUI.LabelField(m_ModifierLabelRect, "Value");
			check.Modifier.intValue = EditorGUI.IntSlider(m_ModifierRect, check.Modifier.intValue, -20, 20);

			var questPrefs = DialogueSystemPreferences.GetOrCreateSettings().Quests;
			EditorGUI.LabelField(m_QuestLabelRect, "Target Quest");
			check.Quest.intValue = BardEditorGUI.QuestPopup(m_QuestRect, check.Quest.intValue, questPrefs);
			
			var targetClass = QuestReflection.Get(questPrefs.GetById(check.Quest.intValue));
			if (targetClass == null || (targetClass.ConditionsNames.Length == 0 && targetClass.StepsNames.Length < 2)) {
				return;
			}

			var step = check.TargetStep;
			var condition = check.TargetCondition;
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
			var newChoice = EditorGUI.Popup(m_TargetTypeRect, currentChoice, m_RequirementTypes);
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

			if (newChoice == 0) {
				var stepValue = targetClass.GetStepValue(step.intValue);
				step.intValue = Convert.ToInt32(EditorGUI.EnumFlagsField(m_TargetInputRect, stepValue));
			}
			else {
				var conditionValue = targetClass.GetConditionIndex(condition.stringValue);
				conditionValue = EditorGUI.Popup(m_TargetInputRect, conditionValue, targetClass.ConditionsNamesDisplay);
				condition.stringValue = targetClass.ConditionsNames[conditionValue];
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 5 + 8f;
		}
	}

	[CustomPropertyDrawer(typeof(DialogueMessageSkillCheck))]
	public class DialogueMessageSkillCheckDrawer : PropertyDrawer {
		private static Rect m_ComplexityLabelRect;
		private static Rect m_ComplexityRect;
		private static Rect m_ModifiersRect;

		private static readonly GUIContent m_ComplexityLabel = new("Complexity");
		static DialogueMessageSkillCheckDrawer() {
			m_ComplexityLabelRect = new(0, 0, 80f, EditorGUIUtility.singleLineHeight);
			m_ComplexityRect = new(80f, 0, 0, EditorGUIUtility.singleLineHeight);
			m_ModifiersRect = new(0, EditorGUIUtility.singleLineHeight + 2f, 0, 0);
		}

		private void SetRects(Rect position, SerializedProperty modifiers) {
			m_ComplexityLabelRect.x = m_ModifiersRect.x = position.x;
			m_ComplexityLabelRect.y = m_ComplexityRect.y = position.y;
			m_ComplexityLabelRect.height = m_ComplexityRect.height = EditorGUIUtility.singleLineHeight;

			m_ComplexityLabelRect.width = 80f;

			m_ComplexityRect.x = m_ComplexityLabelRect.x + m_ComplexityLabelRect.width;
			m_ComplexityRect.width = position.width - m_ComplexityLabelRect.width;

			m_ModifiersRect.width = position.width;
			m_ModifiersRect.y = m_ComplexityRect.y + m_ComplexityRect.height + 2f;
			m_ModifiersRect.height = EditorGUI.GetPropertyHeight(modifiers);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var check = SerializedMessageSkillCheck.GetValue(property);
			if (check == null) return;
			SetRects(position, check.Modifiers);

			EditorGUI.LabelField(m_ComplexityLabelRect, m_ComplexityLabel);
			check.Complexity.intValue = EditorGUI.IntSlider(m_ComplexityRect, check.Complexity.intValue, 1, 20);
			check.ModifiersList.DoList(m_ModifiersRect);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var check = SerializedMessageSkillCheck.GetValue(property);
			if (check == null) return EditorGUIUtility.singleLineHeight;

			if (Event.current.type == EventType.Layout) {
				var height = EditorGUIUtility.singleLineHeight + (check.Modifiers.arraySize > 0 ? 50f : 70f);
				for (int i = 0; i < check.Modifiers.arraySize; i++) {
					height += EditorGUI.GetPropertyHeight(check.Modifiers.GetArrayElementAtIndex(i));
				}
				check.CachedHeight = height;
			}
			return check.CachedHeight;
		}
	}
}
