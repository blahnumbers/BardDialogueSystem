using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Bard.DialogueSystem {
	public class DialogueTree : DialogueBase {
		private static JsonSerializerSettings m_SerializerSettings;
		public static JsonSerializerSettings SerializerSettings { 
			get {
				m_SerializerSettings ??= new JsonSerializerSettings() { MaxDepth = null };
				return m_SerializerSettings;
			}
		}

		[JsonIgnore] public bool Initialized { get; private set; }
		[JsonIgnore] public List<DialogueTree> SharedTrees = null;
		public string LinkedTreeId;
		public bool ShouldSerializeLinkedTreeId() => !string.IsNullOrEmpty(LinkedTreeId);
		[JsonIgnore] public DialogueTree LinkedTree = null;
		public string[] NpcMessage;
		[JsonIgnore] public List<string> NpcMessageLocalized;
		public bool ShouldSerializeNpcMessage() => NpcMessage != null && NpcMessage.Length > 0;
		public int NpcSpeaker;
		public bool ShouldSerializeNpcSpeaker() => NpcSpeaker != 0;
		public string NpcShortMessage;
		public bool ShouldSerializeNpcShortMessage() => !string.IsNullOrEmpty(NpcShortMessage);
		public string SharedID;
		public bool ShouldSerializeSharedID() => !string.IsNullOrEmpty(SharedID);
		[JsonIgnore] public Dictionary<string, DialogueTree> SharedDialogue { get; private set; }
		public string SharedMessagesID;
		public bool ShouldSerializeSharedMessagesID() => !string.IsNullOrEmpty(SharedMessagesID);
		public string MessagesID;
		public bool ShouldSerializeMessagesID() => !string.IsNullOrEmpty(MessagesID);
		[SerializeReference] public DialogueMessage[] Messages;
		public bool ShouldSerializeMessages() => Messages != null && Messages.Length > 0;
		[JsonIgnore] public Dictionary<string, DialogueMessage[]> SharedMessages { get; private set; }
		[JsonIgnore] public DialogueTree Root { get; private set; }

		public void Initialize(List<LocalizationString> localization, ILocalizationProvider localizationProvider = null, DialogueTree rootTree = null) {
			Initialized = true;

			if (rootTree == null) {
				rootTree = this;
				// Only root trees initialize shared banks
				if (string.IsNullOrEmpty(LinkedTreeId)) {
					SharedDialogue = new();
					SharedMessages = new();
				}
				else {
					// These are 'secondary' root nodes which majorly refer to another tree's dialogue
					LinkedTree = SharedTrees.Find(t => t.SharedID == LinkedTreeId);
					SharedDialogue = LinkedTree.SharedDialogue;
					SharedMessages = LinkedTree.SharedMessages;
				}

				// Also only root trees have an overworld message
				if (!string.IsNullOrEmpty(NpcShortMessage)) {
					NpcShortMessage = localization.Find(s => s.Id == NpcShortMessage)?.String;
				}
			}

			if (SharedID != null) {
				rootTree.SharedDialogue.Add(SharedID, this);
			}
			if (SharedMessagesID != null) {
				rootTree.SharedMessages.Add(SharedMessagesID, Messages);
			}
			/*if (NpcSpeaker != BardNPCId.Undefined) {
				TargetNPC = BardNPC.Get(NpcSpeaker);
			}*/
			if (NpcMessage != null) {
				NpcMessageLocalized = new(NpcMessage.Length);
				for (int i = 0; i < NpcMessage.Length; i++) {
					NpcMessageLocalized.Add(localization.Find(s => s.Id == NpcMessage[i])?.String);
				}
			}

			if (Messages != null) {
				foreach (var message in Messages) {
					message.PlayerMessage = localization.Find(s => s.Id == message.Id)?.String;
					message.SetRequirementsFromActions();
					message.SetupSkillCheck(localizationProvider);
					message.RootDialogue = rootTree;
					if (message.FollowUpDialogue != null) {
						//message.FollowUpDialogue.TargetNPC = rootTree.TargetNPC;
						message.FollowUpDialogue.Initialize(localization, localizationProvider, rootTree);
					}
				}
			}
			if (MessagesID != null) {
				Messages = rootTree.SharedMessages[MessagesID];
			}

			Root = rootTree;
		}
	}
}
