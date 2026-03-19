using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using XNode;
using XNodeEditor;
using UnityEditorInternal;
using UnityEngine.UI;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(DialogueMessageBlockNode))]
	public class DialogueMessageBlockNodeEditor : NodeEditor {
		private ColorBlock m_Colors;
		private DialogueMessageBlockNode m_Target;
		private ReorderableList m_List;
		private SerializedProperty m_Messages;
		private Action m_DeferredAction = null;

		private Color FromHEX(string hex) {
			if (!ColorUtility.TryParseHtmlString(hex, out var color)) {
				color = Color.white;
			}
			return color;
		}

		public override void OnCreate() {
			m_Colors = new ColorBlock() {
				normalColor = FromHEX("#16302dff"),
				selectedColor = FromHEX("#173734ff"),
				pressedColor = FromHEX("#1e413cff"),
				disabledColor = FromHEX("#650a0aff")
			};
			m_Target = target as DialogueMessageBlockNode;
			m_Messages = serializedObject.FindProperty("Messages");

			List<SerializedProperty> m_SerializedMessages = new(m_Messages.arraySize);
			void reloadMessages() {
				m_SerializedMessages.Clear();
				for (int i = 0; i < m_Messages.arraySize; i++) {
					m_SerializedMessages.Add(m_Messages.GetArrayElementAtIndex(i));
				}
			}

			reloadMessages();
			m_List = new(serializedObject, m_Messages) {
				drawHeaderCallback = rect => GUI.Label(rect, "Player Messages"),
				drawElementCallback = (rect, index, active, focused) => {
					if (m_DeferredAction != null) {
						m_DeferredAction.Invoke();
						m_DeferredAction = null;
					}
					if (!DialogueGraphUtils.Overlaps(rect)) return;

					GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "#" + (index + 1));
					EditorGUI.BeginProperty(rect, null, m_SerializedMessages[index]);
					EditorGUI.PropertyField(rect, m_SerializedMessages[index]);
					NodeEditorGUILayout.PortField(new Vector2(rect.x + rect.width + 8, rect.y + rect.height / 2), m_Target.GetOutputPort($"{index}"));
					EditorGUI.EndProperty();
				},
				elementHeightCallback = index => {
					if (m_DeferredAction != null) {
						m_DeferredAction.Invoke();
						m_DeferredAction = null;
					}
					return EditorGUI.GetPropertyHeight(m_SerializedMessages[index]);
				},
				drawElementBackgroundCallback = (rect, index, active, focused) => {
					var color = m_Colors.normalColor;
					if (m_Target.GetOutputPort($"{index}")?.Connection == null) {
						color = m_Colors.disabledColor;
					}
					else {
						if (active) color = m_Colors.selectedColor;
						if (focused) color = m_Colors.pressedColor;
					}
					DialogueNodeRequirementsDrawer.BackgroundColor = DialogueMessageNodeActionsDrawer.BackgroundColor = color * 3.5f;
					EditorGUI.DrawRect(rect, color);
				},
				onAddCallback = list => {
					m_DeferredAction = () => {
						m_Target.AddDynamicOutput(typeof(DialogueMessageBlockNode), fieldName: $"{list.serializedProperty.arraySize - 1}", typeConstraint: Node.TypeConstraint.Strict);
						reloadMessages();
					};
					DialogueMessageNodeDrawer.RemoveCache(m_Target.Id);
					Array.Resize(ref m_Target.Messages, list.serializedProperty.arraySize + 1);
				},
				onRemoveCallback = list => {
					m_DeferredAction = () => {
						m_Target.RemoveDynamicPort($"{list.serializedProperty.arraySize}");
						reloadMessages();
					};
					DialogueMessageNodeDrawer.RemoveCache(m_Target.Id);
					Array.Resize(ref m_Target.Messages, list.serializedProperty.arraySize - 1);
				},
				onReorderCallback = list => {
					Dictionary<string, NodePort> oldPorts = new();
					for (int i = 0; i < list.serializedProperty.arraySize; i++) {
						oldPorts.Add(m_Target.Messages[i].Id, m_Target.GetOutputPort($"{i}").Connection);
						DialogueMessageNodeDrawer.RemoveCache(m_Target.Messages[i].Id);
					}
					serializedObject.ApplyModifiedProperties();

					m_DeferredAction = () => {
						for (int i = 0; i < oldPorts.Count; i++) {
							m_Target.RemoveDynamicPort($"{i}");
						}
						for (int i = 0; i < m_Target.Messages.Length; i++) {
							var port = m_Target.AddDynamicOutput(typeof(DialogueMessageBlockNode), fieldName: $"{i}", typeConstraint: Node.TypeConstraint.Strict);
							if (oldPorts.TryGetValue(m_Target.Messages[i].Id, out var nodePort) && nodePort != null) {
								port.Connect(nodePort);
							}
						}
						serializedObject.Update();
						reloadMessages();
					};
				}
			};
		}

		public override void OnBodyGUI() {
			serializedObject.UpdateIfRequiredOrScript();
			NodeEditorGUILayout.PortField(target.GetInputPort(nameof(DialogueMessageBlockNode.Input)));
			m_List.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}

	[NodeWidth(380)]
	[NodeTint("#142b28")]
	public class DialogueMessageBlockNode : Node {
		[Input] public DialogueNode Input;
		public string Id = Guid.NewGuid().ToString("N");
		[Output(dynamicPortList = true)]
		public DialogueMessageNode[] Messages = new[] { DialogueMessageNode.Dummy };
		
		public override object GetValue(NodePort port) {
			if (int.TryParse(port.fieldName, out int id) && id >= 0 && Messages.Length < id) {
				return Messages[id];
			}
			return null;
		}

		new void OnEnable() {
			base.OnEnable();
			if (GetOutputPort("0") == null) {
				AddDynamicOutput(typeof(DialogueMessageBlockNode), fieldName: "0", typeConstraint: TypeConstraint.Strict);
			}
		}

		private void OnValidate() {
			Id = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString("N") : Id;
		}

		public void Setup(DialogueMessage[] msgs, string id) {
			if (!string.IsNullOrEmpty(id)) {
				Id = id;
			}
			Messages = new DialogueMessageNode[msgs.Length];
			for (int i = 0; i < msgs.Length; i++) {
				Messages[i] = DialogueMessageNode.FromMessage(msgs[i]);
				if (GetOutputPort($"{i}") == null) {
					AddDynamicOutput(typeof(DialogueMessageBlockNode), fieldName: $"{i}", typeConstraint: TypeConstraint.Strict);
				}
			}
		}

		public DialogueMessage[] Export() {
			var messages = new DialogueMessage[Messages.Length];
			for (int i = 0; i < Messages.Length; i++) {
				messages[i] = Messages[i].Export();
				var nextDialogue = GetOutputPort($"{i}").Connection?.node as DialogueNode;
				if (nextDialogue != null) {
					if (DialogueGraphUtils.ExporterCachedDialogue.ContainsKey(nextDialogue.Id)) {
						DialogueGraphUtils.ExporterUsedCachedDialogue.Add(nextDialogue.Id);
						messages[i].FollowUpDialogueID = nextDialogue.Id;
					}
					else {
						messages[i].FollowUpDialogue = nextDialogue.Export();
					}
				}
				if (nextDialogue == null || nextDialogue.GetOutputPort("Output").Connection?.node == null) {
					messages[i].MessageType = DialogueMessageType.EXIT;
				}
			}
			return messages;
		}
	}
}
