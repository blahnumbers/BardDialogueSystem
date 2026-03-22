using UnityEngine;
using System;

namespace Bard.DialogueSystem.Actions {
	public partial class AttitudeChangeAction : DialogueAction {
		public int NpcId;
		public int Value;
		[SerializeField] private string _Name = "NPCs/Attitude Change";
		public override string Name => _Name;
		public override string ActionString => "AttitudeChange";
		public override string ToString() => $"{ActionString}:{NpcId}:{Value}";
		public override void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor) {
			executor?.Invoke(this, context);
		}
	}
}
