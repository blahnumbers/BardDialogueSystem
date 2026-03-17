using System;
using UnityEngine;

namespace Bard {
	public partial class QuestConditionAction : DialogueAction {
		public int Id;
		public string Condition;
		[SerializeField] private string _Name = "Quests/Set Condition";
		public override string Name => _Name;
		public override void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor) {
			executor?.Invoke(this, context);
		}
	}
}
