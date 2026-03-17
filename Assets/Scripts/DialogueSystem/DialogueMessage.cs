using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace Bard {
	[Serializable]
	public class DialogueMessage : DialogueBase {
		public string Id;
		public bool IsOneOff = false;
		public bool ShouldSerializeIsOneOff() => IsOneOff;
		[JsonIgnore] public string PlayerMessage;
		public int AttitudeChange = 0;
		public bool ShouldSerializeAttitudeChange() => AttitudeChange != 0;
		public int MessageType = 0;
		public bool ShouldSerializeMessageType() => MessageType != 0;
		public string MessageAction;
		public bool ShouldSerializeMessageAction() => !string.IsNullOrEmpty(MessageAction);
		public string MessageSkillCheck;
		public bool ShouldSerializeMessageSkillCheck() => !string.IsNullOrEmpty(MessageSkillCheck);
		public string FollowUpDialogueID;
		public bool ShouldSerializeFollowUpDialogueID() => !string.IsNullOrEmpty(FollowUpDialogueID);
		[SerializeReference] public DialogueTree FollowUpDialogue;
		[JsonIgnore] public DialogueTree NextDialogue {
			get {
				if (FollowUpDialogueID != null) {
					if (RootDialogue.SharedDialogue.TryGetValue(FollowUpDialogueID, out DialogueTree tree)) {
						return tree;
					}
				}
				return FollowUpDialogue;
			}
		}
		[JsonIgnore] public DialogueTree RootDialogue { get; set; }
		private DialogueMessageSkillCheck m_SkillCheck = null;
		[JsonIgnore] public DialogueMessageSkillCheck SkillCheck => m_SkillCheck;

		public static DialogueMessage EmptyWithFollowUp(string followUpId) {
			return new() { FollowUpDialogueID = followUpId };
		}

		public void SetRequirementsFromActions() {
			if (string.IsNullOrEmpty(MessageAction)) return;

			Match m;
			string[] actions = MessageAction.Split(";");
			foreach (var action in actions) {
				m = Regex.Match(action, @"^UpdateBalance:-(\d+)$"); // ONLY negative balance changes
				if (m.Success) {
					string minBalance = "MinBalance:" + m.Groups[1].Value;
					CustomRequirements = string.IsNullOrEmpty(CustomRequirements) ? minBalance : string.Join(';', new[] { CustomRequirements, minBalance });
					continue;
				}
			}
		}

		public void SetupSkillCheck(ILocalizationProvider localization) {
			if (m_SkillCheck != null || string.IsNullOrEmpty(MessageSkillCheck)) return;
			if (DialogueMessageSkillCheck.TryParse(MessageSkillCheck, localization, out var check)) {
				m_SkillCheck = check;
			}
		}

		public void PerformActions((Regex pattern, Action<Match> handler)[] actionHandlers) {
			/*if (AttitudeChange != 0) {
				RootDialogue.TargetNPC.AttitudeLevel += AttitudeChange;
			}*/

			if (string.IsNullOrEmpty(MessageAction)) return;
			string[] actions = MessageAction.Split(";");
			foreach (var action in actions) {
				foreach (var (regex, handler) in actionHandlers) {
					var m = regex.Match(action);
					if (m.Success) {
						handler(m);
						break;
					}
				}
			}
		}
	}
}
