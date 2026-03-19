using UnityEngine;
using UnityEditor;
using System;
using XNode;
using XNodeEditor;
using System.Text.RegularExpressions;
using System.Linq;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(DialogueRootNode))]
	public class DialogueRootNodeEditor : NodeEditor {
		private DialogueRootNode m_Target;
		private SerializedDialogueRequirement m_Requirements;
		private SerializedProperty m_OverworldMessage;
		private SerializedProperty m_NpcMessages;
		private SerializedProperty m_Id;
		private NodePort m_Output;
		private Color m_BackgroundColor;

		private bool m_FoldoutInitialized;

		public override void OnCreate() {
			m_Target = target as DialogueRootNode;
			m_FoldoutInitialized = false;

			m_Requirements = new(serializedObject.FindProperty("Requirements"));
			m_OverworldMessage = serializedObject.FindProperty("OverworldMsg");
			m_NpcMessages = serializedObject.FindProperty("NpcMessages");
			m_Id = serializedObject.FindProperty("Id");
			m_Output = m_Target.GetOutputPort("Output");
			m_BackgroundColor = new Color(0.25f, 0.3f, 1f);

			target.name = m_Id.stringValue;
		}

		public override void OnBodyGUI() {
			serializedObject.Update();
			Draw();

			serializedObject.ApplyModifiedProperties();
		}

		public override void OnRename() {
			m_Target.Id = target.name;
			var path = AssetDatabase.GetAssetPath(m_Target.graph);
			var error = AssetDatabase.RenameAsset(path, target.name + ".asset");
			if (!string.IsNullOrEmpty(error)) {
				Debug.Log("Rename asset error: " + error);
			}
		}

		private bool Draw() {
			m_Requirements.Property.isExpanded = !m_FoldoutInitialized ? !m_Requirements.IsDefault : m_Requirements.Property.isExpanded;
			m_FoldoutInitialized = true;

			GUILayout.Label("Overworld Message");
			m_OverworldMessage.stringValue = EditorGUILayout.TextArea(m_OverworldMessage.stringValue);
			EditorGUILayout.PropertyField(m_NpcMessages);
			DialogueNodeRequirementsDrawer.BackgroundColor = m_BackgroundColor;
			EditorGUILayout.PropertyField(m_Requirements.Property);
			NodeEditorGUILayout.PortField(m_Output);

			return false;
		}
	}

	[NodeWidth(380)]
	[NodeTint("#14142b")]
	public class DialogueRootNode : DialogueNodeBase {

		public DialogueNodeRequirements Requirements;
		public string OverworldMsg;
		public string OverworldMsgId = Guid.NewGuid().ToString("N");

		public override object GetValue(NodePort port) => null;

		public DialogueNode GetConnectedNode(int index) {
			var port = GetOutputPort("PlayerMessages " + index);
			if (port != null && port.ConnectionCount > 0) {
				return port.GetConnection(0).node as DialogueNode;
			}
			return null;
		}

		private void OnValidate() {
			for (int i = NpcMessageIds.Count; i < NpcMessages.Length; i++) {
				NpcMessageIds.Add(Guid.NewGuid().ToString("N"));
			}
			SetName();
		}

		private void SetName() {
			name = !string.IsNullOrEmpty(OverworldMsg) ? OverworldMsg : ((NpcMessages != null && NpcMessages.Length > 0) ? NpcMessages[0] : "< EMPTY >");
			name = Regex.Replace(name, @"</?\w+>", "");
			if (name.Length > 40) {
				name = name[..37] + "...";
			}
		}

		public override void Setup(DialogueTree dialogue) {
			base.Setup(dialogue);

			OverworldMsg = dialogue.NpcShortMessage;
			Requirements = new() {
				Quests = dialogue.Requirements ?? new(),
				Attitude = new(dialogue.MinAttitude, dialogue.MaxAttitude),
				Custom = dialogue.CustomRequirements?.Split(';').ToList()
			};

			SetName();
		}

		public DialogueTree Export() {
			Requirements.Custom.RemoveAll(c => string.IsNullOrEmpty(c));
			var tree = new DialogueTree() {
				NpcMessage = new string[NpcMessages.Length],
				Requirements = Requirements.Quests,
				MinAttitude = Requirements.Attitude.x,
				MaxAttitude = Requirements.Attitude.y,
				CustomRequirements = string.Join(';', Requirements.Custom),
				SharedID = Id
			};
			ExportMessages(tree);
			if (!string.IsNullOrEmpty(OverworldMsg)) {
				tree.NpcShortMessage = OverworldMsgId;
				DialogueGraphUtils.DefaultLocalization.Add(OverworldMsgId, OverworldMsg);
			}
			DialogueGraphUtils.ExporterCachedDialogue.Add(Id, tree);

			var output = GetOutputPort("Output").Connection?.node;
			if (output == null) return tree;
			if (output is DialogueMessageBlockNode messages) {
				if (DialogueGraphUtils.ExporterCachedMessages.ContainsKey(messages.Id)) {
					DialogueGraphUtils.ExporterUsedCachedMessages.Add(messages.Id);
					tree.MessagesID = messages.Id;
				}
				else {
					DialogueGraphUtils.ExporterCachedMessages.Add(messages.Id, tree);
					tree.SharedMessagesID = messages.Id;
					tree.Messages = messages.Export();
				}
			}
			else if (output is DialogueNode nextDialogue) {
				// We need to insert an intermediate message node
				var message = DialogueMessage.EmptyWithFollowUp(nextDialogue.Id);
				if (DialogueGraphUtils.ExporterCachedDialogue.ContainsKey(nextDialogue.Id)) {
					DialogueGraphUtils.ExporterUsedCachedDialogue.Add(nextDialogue.Id);
					message.FollowUpDialogueID = nextDialogue.Id;
				}
				else {
					message.FollowUpDialogue = nextDialogue.Export();
				}
				tree.Messages = new DialogueMessage[] { message };
			}

			return tree;
		}
	}
}
