using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Bard.Editor;

namespace Bard.XNodeEditor {
	public class SerializedMessageNodeProperties {
		public SerializedProperty IsOneOff;
		public SerializedProperty Message;
		public SerializedProperty Requirements;
		public SerializedProperty Actions;
		public SerializedProperty Type;
		public float CachedHeight = 0f;

		public SerializedMessageNodeProperties(SerializedProperty property) {
			IsOneOff = property.FindPropertyRelative("IsOneOff");
			Message = property.FindPropertyRelative("Message");
			Requirements = property.FindPropertyRelative("Requirements");
			Actions = property.FindPropertyRelative("Actions");
			Type = property.FindPropertyRelative("Type");
		}
	}

	[CustomPropertyDrawer(typeof(DialogueMessageNode))]
	public class DialogueMessageNodeDrawer : PropertyDrawer {
		private static readonly Dictionary<string, SerializedMessageNodeProperties> m_Cache = new();
		private static readonly GUIStyle m_TextAreaStyle;
		private static readonly GUIContent m_MessageContent;
		private static Rect m_Rect;

		static DialogueMessageNodeDrawer() {
			m_TextAreaStyle = new(GUI.skin.textArea) { wordWrap = true };
			m_MessageContent = new GUIContent("");
			m_Rect = new(0, EditorGUIUtility.singleLineHeight, 0, 18f);
		}

		private SerializedMessageNodeProperties GetCachedValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var target) || target.Message == null) {
				target = new(property);
				m_Cache[id] = target;
			}
			return target;
		}

		public static void RemoveCache(string id) {
			if (m_Cache.TryGetValue(id, out var value)) {
				if (value.Requirements != null) {
					SerializedDialogueRequirement.RemoveCache(value.Requirements);
				}
				if (value.Actions != null) {
					SerializedDialogueAction.RemoveCache(value.Actions);
				}
				m_Cache.Remove(id);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var data = GetCachedValue(property);
			m_MessageContent.text = "One-off:";
			m_Rect.x = position.x + position.width - 100;
			m_Rect.width = 100;
			m_Rect.y = position.y;
			m_Rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(m_Rect, data.IsOneOff, m_MessageContent);

			m_MessageContent.text = data.Message.stringValue;
			m_Rect.x = position.x;
			m_Rect.width = position.width;
			m_Rect.y = position.y + EditorGUIUtility.singleLineHeight + 2f;
			m_Rect.height = Mathf.Max(EditorGUIUtility.singleLineHeight * 2, m_TextAreaStyle.CalcHeight(m_MessageContent, position.width)) + 8f;
			data.Message.stringValue = EditorGUI.TextArea(m_Rect, data.Message.stringValue, EditorStyles.textArea);
			m_Rect.y += m_Rect.height + 4f;
			m_Rect.height = EditorGUIUtility.singleLineHeight;

			var messageTypeNames = DialogueSystemPreferences.GetOrCreateSettings().Messages.TypeNames;
			data.Type.intValue = EditorGUI.Popup(m_Rect, data.Type.intValue, messageTypeNames);
			m_Rect.y += m_Rect.height + 4f;
			m_Rect.height = EditorGUI.GetPropertyHeight(data.Requirements);
			EditorGUI.PropertyField(m_Rect, data.Requirements);
			m_Rect.y += m_Rect.height + 4f;
			m_Rect.height = EditorGUI.GetPropertyHeight(data.Actions);
			EditorGUI.PropertyField(m_Rect, data.Actions);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			float height = EditorGUIUtility.singleLineHeight * 2 + 16f;
			if (property != null) {
				var data = GetCachedValue(property);
				if (data.Message != null && m_TextAreaStyle != null) {
					height += Mathf.Max(EditorGUIUtility.singleLineHeight * 2, m_TextAreaStyle.CalcHeight(new GUIContent(data.Message.stringValue), 320)) + 8f;
				}
				if (data.Requirements != null) {
					height += EditorGUI.GetPropertyHeight(data.Requirements);
				}
				if (data.Actions != null) {
					height += EditorGUI.GetPropertyHeight(data.Actions);
				}
			}
			return height;
		}
	}

	[Serializable]
	public class DialogueMessageNode {
		public bool IsOneOff;
		public DialogueNodeRequirements Requirements;
		public DialogueMessageNodeActions Actions;
		[TextArea] public string Message;
		public int Type;
		public static DialogueMessageNode Dummy => new() { Message = null, Type = default, Actions = new(), Requirements = new() };
		public string Id = Guid.NewGuid().ToString("N");

		public static DialogueMessageNode FromMessage(DialogueMessage msg) {
			var node = new DialogueMessageNode() {
				IsOneOff = msg.IsOneOff,
				Message = msg.PlayerMessage,
				Type = msg.MessageType,
				Actions = new DialogueMessageNodeActions() {
					AttitudeChange = msg.AttitudeChange,
					Custom = new()
				},
				Requirements = new DialogueNodeRequirements() {
					Quests = msg.Requirements,
					Attitude = new Vector2Int(msg.MinAttitude, msg.MaxAttitude),
					Custom = msg.CustomRequirements?.Split(';').ToList()
				}
			};
			if (msg.MessageAction != null) {
				foreach (var action in msg.MessageAction.Split(';')) {
					node.Actions.Custom.Add(new(action));
				}
			}
			return node;
		}

		public DialogueMessage Export() {
			Requirements.Custom.RemoveAll(c => string.IsNullOrEmpty(c));
			Actions.Custom.RemoveAll(c => !c.IsValid);

			var actionPrefs = DialogueSystemPreferences.GetOrCreateSettings().MessageActions;
			if (!string.IsNullOrEmpty(Message)) {
				DialogueGraphUtils.DefaultLocalization.Add(Id, Message);
			}

			DialogueMessage exportedMessage = new() {
				Id = Id,
				IsOneOff = IsOneOff,
				PlayerMessage = Message,
				MessageType = Type,
				Requirements = Requirements.Quests,
				MinAttitude = Requirements.Attitude.x,
				MaxAttitude = Requirements.Attitude.y,
				CustomRequirements = string.Join(';', Requirements.Custom),
				AttitudeChange = Actions.AttitudeChange
			};

			List<DialogueMessageNodeAction> customActions = new();
			Actions.Custom.ForEach(c => {
				if (!actionPrefs.TryGetById(c.Type, out var action) || !action.ExportCustom(c, exportedMessage, DialogueGraphUtils.Localization)) {
					customActions.Add(c);
				}
			});

			return exportedMessage;
		}
	}
}
