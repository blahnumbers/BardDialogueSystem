using UnityEngine;
using System;

namespace Bard.DialogueSystem.Actions {
	public partial class QuestProgressAction : DialogueAction {
		public int Id;
		public int Step;
		[SerializeField] private string _Name = "Quests/Progress Quest";
		public override string Name => _Name;
		public override string ActionString => "ProgressQuest";
		public override string ToString() => $"{ActionString}:{Id}:{Step}";
		public override void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor) {
			executor?.Invoke(this, context);
		}
	}
}
