using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Bard.XNodeEditor {
	public class DialogueNodeBase : Node {
		[Output(connectionType = ConnectionType.Override)]
		public DialogueMessagesNode Output;
		[TextArea] public string[] NpcMessages = new string[1] { "" };
		public List<string> NpcMessageIds = new() { Guid.NewGuid().ToString("N") };
		public string Id = Guid.NewGuid().ToString("N");

		public virtual void Setup(DialogueTree dialogue) {
			NpcMessages = new string[dialogue.NpcMessage.Length];
			Id = dialogue.SharedID;

			for (int i = 0; i < dialogue.NpcMessage.Length; i++) {
				NpcMessages[i] = dialogue.NpcMessage[i];
			}
		}

		protected void ExportMessages(DialogueTree tree) {
			int skipped = 0;
			for (int i = 0; i < NpcMessages.Length; i++) {
				if (string.IsNullOrEmpty(NpcMessages[i])) {
					skipped++;
					continue;
				}
				tree.NpcMessage[i - skipped] = NpcMessageIds[i];
				DialogueGraphUtils.LocalizationCache.Add(NpcMessageIds[i], NpcMessages[i]);
			}
			if (skipped > 0) {
				Array.Resize(ref tree.NpcMessage, tree.NpcMessage.Length - skipped);
			}
		}
	}
}
