using UnityEngine;
using System;

namespace Bard.DialogueSystem.Actions {
	public partial class QuestAddAction : DialogueAction {
		public int Id;
		[SerializeField] private string _Name = "Quests/Add Quest";
		public override string Name => _Name;
		public override string ActionString => "AddQuest";
		public override string ToString() => $"{ActionString}:{Id}";
		public override void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor) {
			executor?.Invoke(this, context);
		}
	}
}
