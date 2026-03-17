using System;
using UnityEngine;

namespace Bard {
	public partial class QuestProgressAction : DialogueAction {
		public int Id;
		public int Step;
		[SerializeField] private string _Name = "Quests/Progress Quest";
		public override string Name => _Name;
		public override void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor) {
			executor?.Invoke(this, context);
		}
	}
}
