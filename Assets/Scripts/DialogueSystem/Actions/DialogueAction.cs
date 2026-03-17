using System;
using UnityEngine;

namespace Bard {
	public abstract class DialogueAction : ScriptableObject {
		public abstract string Name { get; }
		public abstract void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor);
	}
}
