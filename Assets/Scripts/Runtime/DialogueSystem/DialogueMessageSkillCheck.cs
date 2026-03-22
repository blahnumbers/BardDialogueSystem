using System;
using System.Globalization;
using System.Collections.Generic;

namespace Bard.DialogueSystem {
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
