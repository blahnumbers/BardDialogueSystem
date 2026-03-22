using UnityEngine;
using System;

namespace Bard.DialogueSystem.Actions {
	public partial class SkillCheckAction : DialogueAction {
		public DialogueMessageSkillCheck SkillCheck;
		[SerializeField] private string _Name = "Skill Check";
		public override string Name => _Name;
		public override string ActionString => null;
		public override string ToString() => null;
		public override void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor) {
			executor?.Invoke(this, context);
		}
#if UNITY_EDITOR
		public override bool ExportCustom(DialogueMessageActionBase data, DialogueMessage message, ILocalizationCache localization) {
			if (data.SkillCheck != null) {
				message.MessageSkillCheck = data.SkillCheck.ToString();
				foreach (var modifier in data.SkillCheck.Modifiers) {
					localization.Caches["SkillChecks"].Add(modifier.Id, modifier.Message);
				}
			}
			return true;
		}
#endif
	}
}
