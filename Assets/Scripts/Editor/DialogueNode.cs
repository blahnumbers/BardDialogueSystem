using UnityEngine;
using UnityEditor;
using System;
using XNode;
using XNodeEditor;
using System.Text.RegularExpressions;
using UnityEditorInternal;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(DialogueNode))]
	public class DialogueNodeEditor : NodeEditor {
		private DialogueNode m_Target;
		private SerializedProperty m_NpcMessages;
		private SerializedProperty m_Speaker;
		private NodePort m_Output;
		private Color m_TailNodeColor = new(0.5f, 0f, 0f);
		private ReorderableList m_MessagesList;
		private GUIContent m_ListContent;

		public override Color GetTint() {
			if (target.GetOutputPort("Output").Connection == null) {
				return m_TailNodeColor;
			}
			return base.GetTint();
		}

		public override void OnBodyGUI() {
			if (m_Target == null) {
				m_Target = target as DialogueNode;
				m_NpcMessages = serializedObject.FindProperty("NpcMessages");
				//m_Speaker = serializedObject.FindProperty("Speaker");
				m_Output = m_Target.GetOutputPort("Output");

				m_ListContent = new GUIContent();
				m_MessagesList = new(serializedObject, m_NpcMessages) {
					drawHeaderCallback = rect => {
						m_ListContent.text = "NPC Messages: " + m_NpcMessages.arraySize;
						EditorGUI.LabelField(rect, m_ListContent);
					},
					drawElementCallback = (rect, index, active, focus) => {
						var message = m_NpcMessages.GetArrayElementAtIndex(index);
						rect.y += 1;
						rect.height -= 2;
						rect.width = 320;
						message.stringValue = EditorGUI.TextArea(rect, message.stringValue, EditorStyles.textArea);
					},
					elementHeightCallback = i => {
						m_ListContent.text = m_NpcMessages.GetArrayElementAtIndex(i).stringValue;
						return Mathf.Max(EditorGUIUtility.singleLineHeight * 2, GUI.skin.textArea.CalcHeight(m_ListContent, 320)) + 8f;
					},
					onAddCallback = list => {
						Array.Resize(ref m_Target.NpcMessages, m_Target.NpcMessages.Length + 1);
						m_Target.NpcMessages[^1] = string.Empty;
						serializedObject.ApplyModifiedProperties();
					}
				};
			}

			serializedObject.UpdateIfRequiredOrScript();

			NodeEditorGUILayout.PortField(target.GetInputPort(nameof(DialogueNode.Input)));
			/*GUILayout.BeginHorizontal();
			var color = GUI.contentColor;
			if (m_Speaker.intValue != 0) {
				GUI.contentColor = Color.yellow;
			}
			GUILayout.Label("Speaker Override", GUILayout.Width(120));
			m_Speaker.intValue = (int)(BardNPCId)EditorGUILayout.EnumPopup((BardNPCId)m_Speaker.intValue);
			GUI.contentColor = color;
			GUILayout.EndHorizontal();*/

			m_MessagesList.DoLayoutList();
			NodeEditorGUILayout.PortField(m_Output);

			serializedObject.ApplyModifiedProperties();
		}
	}

	[NodeWidth(380)]
	[NodeTint("#301c36")]
	public class DialogueNode : DialogueNodeBase {
		[Input] public DialogueMessageBlockNode Input;

//		public BardNPCId Speaker;

		public override object GetValue(NodePort port) => null;

		private void OnValidate() {
			Id = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString("N") : Id;
			for (int i = NpcMessageIds.Count; i < NpcMessages.Length; i++) {
				NpcMessageIds.Add(Guid.NewGuid().ToString("N"));
			}
			SetName();
		}

		public DialogueNode GetConnectedNode(int index) {
			var port = GetOutputPort("PlayerMessages " + index);
			if (port != null && port.ConnectionCount > 0) {
				return port.GetConnection(0).node as DialogueNode;
			}
			return null;
		}

		private void SetName() {
//			name = (Speaker != BardNPCId.Undefined ? $"[ {Speaker} ] " : "") + ((NpcMessages != null && NpcMessages.Length > 0) ? NpcMessages[0] : "< EMPTY >");
			name = Regex.Replace(name, @"</?\w+>", "");
			if (name.Length > 40) {
				name = name[..37] + "...";
			}
		}

		public override void Setup(DialogueTree dialogue) {
			base.Setup(dialogue);

//			Speaker = dialogue.NpcSpeaker;
			SetName();
		}

		public DialogueTree Export() {
			var tree = new DialogueTree() {
//				NpcSpeaker = Speaker,
				NpcMessage = new string[NpcMessages.Length],
				SharedID = Id
			};
			ExportMessages(tree);
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
				// Same logic as for root node, add an intermediate messages node and continue
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
