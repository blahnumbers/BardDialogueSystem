using System;
using UnityEngine;

namespace Bard {
	public abstract class DialogueAction : ScriptableObject {
		public abstract string Name { get; }
		public abstract string ActionString { get; }
		new public abstract string ToString();
		public abstract void Execute(DialogueMessage context, Action<DialogueAction, DialogueMessage> executor);
#if UNITY_EDITOR
		public virtual bool ExportCustom(DialogueMessageActionBase data, DialogueMessage message, ILocalizationCache localizationCache) => false;
#endif
	}
}
