using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Bard {
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
			/*var check = SerializedMessageSkillCheckModifier.GetValue(property);
			if (check == null) return;
			SetRects(position);

			EditorGUI.LabelField(m_MessageLabelRect, "Description");
			check.Message.stringValue = EditorGUI.TextField(m_MessageRect, check.Message.stringValue);

			EditorGUI.LabelField(m_ModifierLabelRect, "Value");
			check.Modifier.intValue = EditorGUI.IntSlider(m_ModifierRect, check.Modifier.intValue, -20, 20);

			EditorGUI.LabelField(m_QuestLabelRect, "Target Quest");
			var newQuest = (int)(BardQuestId)EditorGUI.EnumPopup(m_QuestRect, (BardQuestId)check.Quest.intValue);
			if (newQuest != check.Quest.intValue) {
				check.Quest.intValue = newQuest;
			}
			
			var targetClass = QuestReflection.Get(((BardQuestId)check.Quest.intValue).ToString());
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
			}*/
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 5 + 8f;
		}
	}
}
#endif

namespace Bard {
	[Serializable]
	public class DialogueMessageSkillCheckModifier {
		public string Id = Guid.NewGuid().ToString("N");
		public int Quest;
		public long TargetStep;
		public bool ShouldSerializeTargetStep() => TargetStep > 0;
		public string TargetCondition;
		public bool ShouldSerializeTargetCondition() => !string.IsNullOrEmpty(TargetCondition);
		/*[JsonIgnore] public bool IsMet {
			get {
				return string.IsNullOrEmpty(TargetCondition) ? BardGame.Quests.HasProgress(Quest, TargetStep) : BardGame.Quests.HasCondition(Quest, TargetCondition);
			}
		}*/
		public int Modifier;
		public string Message;

		public static bool TryParse(string str, ILocalizationProvider localization, out DialogueMessageSkillCheckModifier modifier) {
			var segments = str.Split(":");
			modifier = new() { Id = segments[0] };
			if (localization == null) return false;

			if (!Enum.TryParse(segments[1], out modifier.Quest)) {
				return false;
			}
			if (!long.TryParse(segments[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out modifier.TargetStep)) {
				modifier.TargetCondition = segments[2];
			}
			if (!int.TryParse(segments[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out modifier.Modifier)) {
				return false;
			}
			if (!localization.TryGetString(modifier.Id, out modifier.Message)) {
				return false;
			}
			return true;
		}

		public override string ToString() {
			var stepCondition = TargetStep > 0 ? TargetStep.ToString() : TargetCondition;
			return $"{Id}:{Quest}:{stepCondition}:{Modifier}";
		}
	}

	[Serializable]
	public class DialogueMessageSkillCheck {
		public string Id = Guid.NewGuid().ToString("N");
		public List<DialogueMessageSkillCheckModifier> Modifiers = new();
		/*public List<DialogueMessageSkillCheckModifier> ValidModifiers {
			get {
				var modifiers = Modifiers.FindAll(m => m.IsMet);
				switch (BardGame.Player.HungerLevel) {
					case BardPlayerHungerLevel.Starving:
						modifiers.Add(new() { Message = "You are starving", Modifier = -4 });
						break;
					case BardPlayerHungerLevel.Hungry:
						modifiers.Add(new() { Message = "You are hungry", Modifier = -2 });
						break;
				}
				return modifiers;
			}
		}*/
		public int Complexity;
		/*public int TargetComplexity {
			get {
				int target = Complexity;
				Modifiers.ForEach(m => target -= m.IsMet ? m.Modifier : 0);
				if (BardGame.Player.HungerLevel < BardPlayerHungerLevel.Satiated) {
					target += 2 * (BardPlayerHungerLevel.Satiated - BardGame.Player.HungerLevel);
				}
				return Mathf.Clamp(target, 1, 20);
			}
		}*/

		public static bool TryParse(string str, ILocalizationProvider localization, out DialogueMessageSkillCheck check) {
			check = new();
			if (localization == null) return false;

			var segments = str.Split("_");
			if (!int.TryParse(segments[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out check.Complexity)) {
				return false;
			}
			for (int i = 1; i < segments.Length; i++) {
				if (!DialogueMessageSkillCheckModifier.TryParse(segments[i], localization, out var modifier)) {
					return false;
				}
				check.Modifiers.Add(modifier);
			}
			return true;
		}

		public override string ToString() {
			List<string> segments = new() { Complexity.ToString() };
			foreach (var mod in Modifiers) {
				segments.Add(mod.ToString());
			}
			return string.Join('_', segments);
		}
	}
}
