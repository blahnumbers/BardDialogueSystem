using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using XNode;
using XNodeEditor;
using System.Linq;
using UnityEditorInternal;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using Bard.Editor;
using UnityEngine.EventSystems;

namespace Bard.XNodeEditor {
	public class SerializedDialogueRequirement {
		public SerializedProperty Property;
		public SerializedProperty Quests;
		public SerializedProperty Attitude;
		public SerializedProperty Custom;
		public bool AttitudeActive;
		public Vector2Int LastAttitude;


		public bool IsAttitudeDefault => Attitude.vector2IntValue.x == int.MinValue || Attitude.vector2IntValue.y == int.MaxValue;
		public bool IsCustomDefault {
			get {
				bool isDefault = true;
				for (int i = 0; i < Custom.arraySize && isDefault; i++) {
					isDefault = string.IsNullOrEmpty(Custom.GetArrayElementAtIndex(i).stringValue);
				}
				return isDefault;
			}
		}
		public bool IsQuestsDefault {
			get {
				bool isDefault = true;
				for (int i = 0; i < Quests.arraySize && isDefault; i++) {
					var quest = Quests.GetArrayElementAtIndex(i);
					isDefault = quest.FindPropertyRelative("Id").enumValueIndex == 0;
				}
				return isDefault;
			}
		}
		public bool IsDefault => IsAttitudeDefault && IsCustomDefault && IsQuestsDefault;

		public SerializedDialogueRequirement(SerializedProperty prop) {
			Property = prop;
			Quests = prop.FindPropertyRelative("Quests");
			Attitude = prop.FindPropertyRelative("Attitude");
			Custom = prop.FindPropertyRelative("Custom");

			LastAttitude = Attitude.vector2IntValue;
			AttitudeActive = Attitude.vector2IntValue.x != int.MinValue || Attitude.vector2IntValue.y != int.MaxValue;

			Property.isExpanded = !IsDefault;
			Quests.isExpanded = !IsQuestsDefault;
			if (Quests.isExpanded) {
				for (int i = 0; i < Quests.arraySize; i++) {
					Quests.GetArrayElementAtIndex(i).isExpanded = true;
				}
			}
			Custom.isExpanded = !IsCustomDefault;
		}
	}

	public class SerializedDialogueAction {
		public readonly DialogueMessageNodeActions Target = null;
		public SerializedProperty Property;
		public SerializedProperty Attitude;
		public SerializedProperty Custom;
		public ReorderableList CustomList;

		public bool IsAttitudeDefault => Attitude.intValue == 0;
		public bool IsCustomDefault {
			get {
				bool isDefault = true;
				for (int i = 0; i < Custom.arraySize && isDefault; i++) {
					isDefault = Custom.GetArrayElementAtIndex(i).FindPropertyRelative("Type").intValue == 0;
				}
				return isDefault;
			}
		}
		public bool IsDefault => IsAttitudeDefault && IsCustomDefault;

		public SerializedDialogueAction(SerializedProperty prop) {
			Property = prop;
			Attitude = prop.FindPropertyRelative("AttitudeChange");
			Custom = prop.FindPropertyRelative("CustomA");
			
			Custom.isExpanded = !IsCustomDefault;
			Property.isExpanded = !IsDefault;
			
			var id = prop.FindPropertyRelative("Id");
			var messageNode = prop.serializedObject.targetObject as DialogueMessageBlockNode;
			foreach (var message in messageNode.Messages) {
				if (message.Actions.Id == id.stringValue) {
					Target = message.Actions;
					break;
				}
			}

			CustomList = new(prop.serializedObject, Custom, false, true, true, true) {
				drawHeaderCallback = (rect) => {
					EditorGUI.LabelField(rect, $"{Custom.arraySize} action(s)");
				},
				drawElementCallback = (rect, index, active, focused) => {
					EditorGUI.PropertyField(rect, Custom.GetArrayElementAtIndex(index));
				},
				elementHeightCallback = index => {
					return EditorGUI.GetPropertyHeight(Custom.GetArrayElementAtIndex(index));
				},
				onAddCallback = list => {
					Target.CustomA.Add(default);
					prop.serializedObject.Update();
				},
				onRemoveCallback = list => {
					Target.CustomA.RemoveAt(list.selectedIndices.Count > 0 ? list.selectedIndices[0] : 0);
					prop.serializedObject.Update();
				}
			};
		}
	}

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
				for (int i = 0; i < m_Messages.arraySize; i++)
				{
					m_SerializedMessages.Add(m_Messages.GetArrayElementAtIndex(i));
				}
			}
			var type = typeof(GUI).Assembly.GetType("UnityEngine.GUIClip");
			var visibleRect = type.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			reloadMessages();
			m_List = new(serializedObject, m_Messages) {
				drawHeaderCallback = rect => {
					GUI.Label(rect, "Player Messages");
				},
				drawElementCallback = (rect, index, active, focused) => {
					if (m_DeferredAction != null) {
						m_DeferredAction.Invoke();
						m_DeferredAction = null;
					}
					Rect visible = (Rect)visibleRect.GetValue(null);
					if (!visible.Overlaps(rect)) return;

					GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "#" + (index + 1));
					EditorGUI.BeginProperty(rect, null, m_SerializedMessages[index]);
					EditorGUI.PropertyField(rect, m_SerializedMessages[index]);
					NodeEditorGUILayout.PortField(new Vector2(rect.x + rect.width + 8, rect.y + rect.height / 2), m_Target.GetOutputPort($"{index}"));
					EditorGUI.EndProperty();
				},
				elementHeightCallback = index => {
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
							if (oldPorts.TryGetValue(m_Target.Messages[i].Id, out var nodePort)) {
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
					DialogueNodeRequirementsDrawer.RemoveCache(value.Requirements);
				}
				if (value.Actions != null) {
					DialogueMessageNodeActionsDrawer.RemoveCache(value.Actions);
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
			EditorGUI.PropertyField(m_Rect, data.Type);
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

	public class SerializedMessageSkillCheckModifier {
		private static readonly Dictionary<string, SerializedMessageSkillCheckModifier> m_Cache = new();

		public readonly string Id;
		public SerializedProperty Message;
		public SerializedProperty Modifier;
		public SerializedProperty Quest;
		public SerializedProperty TargetStep;
		public SerializedProperty TargetCondition;

		public SerializedMessageSkillCheckModifier(SerializedProperty property, string id) {
			Id = id;
			Message = property.FindPropertyRelative("Message");
			Modifier = property.FindPropertyRelative("Modifier");
			Quest = property.FindPropertyRelative("Quest");
			TargetStep = property.FindPropertyRelative("TargetStep");
			TargetCondition = property.FindPropertyRelative("TargetCondition");
		}

		public static SerializedMessageSkillCheckModifier GetValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var value)) {
				value = new(property, id);
				m_Cache[id] = value;
			}
			return value;
		}

		public static void Remove(string id) {
			m_Cache.Remove(id);
		}
	}


	[CustomPropertyDrawer(typeof(DialogueMessageSkillCheckModifier))]
	public class DialogueMessageSkillCheckModifierDrawer : PropertyDrawer {
		private static readonly string[] m_RequirementTypes = { "Step", "Condition" };
		private static Rect m_MessageRect;
		private static Rect m_MessageLabelRect;
		private static Rect m_ModifierRect;
		private static Rect m_ModifierLabelRect;
		private static Rect m_QuestRect;
		private static Rect m_QuestLabelRect;
		private static Rect m_TargetTypeRect;
		private static Rect m_TargetInputRect;

		static DialogueMessageSkillCheckModifierDrawer() {
			m_MessageRect = new(0, EditorGUIUtility.singleLineHeight, 0, EditorGUIUtility.singleLineHeight);
			m_MessageLabelRect = new(0, 0, 0, EditorGUIUtility.singleLineHeight);
			m_ModifierRect = new(82f, EditorGUIUtility.singleLineHeight * 2 + 2f, 0, EditorGUIUtility.singleLineHeight);
			m_ModifierLabelRect = new(0, EditorGUIUtility.singleLineHeight * 2 + 2f, 80f, EditorGUIUtility.singleLineHeight);
			m_QuestRect = new(82f, EditorGUIUtility.singleLineHeight * 3 + 4f, 0, EditorGUIUtility.singleLineHeight);
			m_QuestLabelRect = new(0, EditorGUIUtility.singleLineHeight * 3 + 4f, 80f, EditorGUIUtility.singleLineHeight);
			m_TargetTypeRect = new(0, EditorGUIUtility.singleLineHeight * 4 + 6f, 80f, EditorGUIUtility.singleLineHeight);
			m_TargetInputRect = new(82f, EditorGUIUtility.singleLineHeight * 4 + 6f, 0, EditorGUIUtility.singleLineHeight);
		}

		private void SetRects(Rect position) {
			m_MessageLabelRect.height = m_MessageRect.height = m_ModifierRect.height = m_ModifierLabelRect.height = m_QuestLabelRect.height = m_QuestRect.height = m_TargetTypeRect.height = m_TargetInputRect.height = EditorGUIUtility.singleLineHeight;

			m_MessageRect.x = m_MessageLabelRect.x = m_ModifierLabelRect.x = m_QuestLabelRect.x = m_TargetTypeRect.x = position.x;
			m_MessageRect.width = m_MessageLabelRect.width = position.width;

			m_ModifierLabelRect.width = m_QuestLabelRect.width = m_TargetTypeRect.width = 80f;
			m_ModifierRect.width = m_QuestRect.width = m_TargetInputRect.width = position.width - m_ModifierLabelRect.width - 2f;
			m_ModifierRect.x = m_QuestRect.x = m_TargetInputRect.x = position.x + m_ModifierLabelRect.width + 2f;

			m_MessageLabelRect.y = position.y;
			m_MessageRect.y = m_MessageLabelRect.y + m_MessageLabelRect.height;
			m_ModifierLabelRect.y = m_ModifierRect.y = m_MessageRect.y + m_MessageRect.height + 2f;
			m_QuestLabelRect.y = m_QuestRect.y = m_ModifierRect.y + m_ModifierRect.height + 2f;
			m_TargetInputRect.y = m_TargetTypeRect.y = m_QuestRect.y + m_QuestRect.height + 2f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			/*var check = SerializedMessageSkillCheckModifier.GetValue(property);
			if (check == null) return;
			SetRects(position);

			EditorGUI.LabelField(m_MessageLabelRect, "Description");
			check.Message.stringValue = EditorGUI.TextField(m_MessageRect, check.Message.stringValue);

			EditorGUI.LabelField(m_ModifierLabelRect, "Value");
			check.Modifier.intValue = EditorGUI.IntSlider(m_ModifierRect, check.Modifier.intValue, -20, 20);

			EditorGUI.LabelField(m_QuestLabelRect, "Target Quest");
			var newQuest = (int)(BardQuestId)EditorGUI.EnumPopup(m_QuestRect, (BardQuestId)check.Quest.intValue);
			if (newQuest != check.Quest.intValue) {
				check.Quest.intValue = newQuest;
			}
			
			var targetClass = QuestReflection.Get(((BardQuestId)check.Quest.intValue).ToString());
			if (targetClass == null || (targetClass.ConditionsNames.Length == 0 && targetClass.StepsNames.Length < 2)) {
				return;
			}

			var step = check.TargetStep;
			var condition = check.TargetCondition;
			if (targetClass.ConditionsNames.Length == 0 || targetClass.StepsNames.Length < 2) {
				if (targetClass.StepsNames.Length > 1 && step.intValue == 0) {
					step.intValue = 1;
					condition.stringValue = "";
				}
				else if (targetClass.ConditionsNames.Length > 0 && step.intValue > 0) {
					step.intValue = 0;
					condition.stringValue = targetClass.GetCondition(1);
				}
				GUI.enabled = false;
			}
			var currentChoice = (step.intValue == -1 || step.intValue > 0) ? 0 : 1;
			var newChoice = EditorGUI.Popup(m_TargetTypeRect, currentChoice, m_RequirementTypes);
			GUI.enabled = true;

			if (currentChoice != newChoice) {
				if (newChoice == 0) {
					step.intValue = 1;
					condition.stringValue = "";
				}
				else {
					step.intValue = 0;
					condition.stringValue = targetClass.GetCondition(1);
				}
			}

			if (newChoice == 0) {
				var stepValue = targetClass.GetStepValue(step.intValue);
				step.intValue = Convert.ToInt32(EditorGUI.EnumFlagsField(m_TargetInputRect, stepValue));
			}
			else {
				var conditionValue = targetClass.GetConditionIndex(condition.stringValue);
				conditionValue = EditorGUI.Popup(m_TargetInputRect, conditionValue, targetClass.ConditionsNamesDisplay);
				condition.stringValue = targetClass.ConditionsNames[conditionValue];
			}*/
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 5 + 8f;
		}
	}

	public class SerializedMessageSkillCheck {
		private static readonly Dictionary<string, SerializedMessageSkillCheck> m_Cache = new();
		public float CachedHeight = 0f;
		public readonly DialogueMessageSkillCheck Target = null;
		public readonly SerializedProperty Complexity;
		public readonly SerializedProperty Modifiers;
		public readonly ReorderableList ModifiersList;

		public SerializedMessageSkillCheck(SerializedProperty property, string id) {
			// Find target skill check
			// There must a better way to do it... right?
			var messageNode = property.serializedObject.targetObject as DialogueMessageBlockNode;
			foreach (var message in messageNode.Messages) {
				foreach (var action in message.Actions.CustomA) {
					if (action.SkillCheck.Id == id) {
						Target = action.SkillCheck;
						break;
					}
				}
				if (Target != null) break;
			}

			Complexity = property.FindPropertyRelative("Complexity");
			Modifiers = property.FindPropertyRelative("Modifiers");
			ModifiersList = new(property.serializedObject, Modifiers, false, true, true, true) {
				drawHeaderCallback = rect => {
					EditorGUI.LabelField(rect, $"{Modifiers.arraySize} Modifiers");
				},
				drawElementCallback = (rect, index, active, focused) => {
					EditorGUI.PropertyField(rect, Modifiers.GetArrayElementAtIndex(index));
				},
				elementHeightCallback = index => {
					return EditorGUI.GetPropertyHeight(Modifiers.GetArrayElementAtIndex(index));
				},
				onAddCallback = list => {
					Target.Modifiers.Add(new());
					property.serializedObject.Update();
				},
				onRemoveCallback = list => {
					Target.Modifiers.ForEach(m => SerializedMessageSkillCheckModifier.Remove(m.Id));
					Target.Modifiers.RemoveAt(list.selectedIndices.Count > 0 ? list.selectedIndices[0] : 0);
					property.serializedObject.Update();
				}
			};
		}

		public static SerializedMessageSkillCheck GetValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var value)) {
				value = new(property, id);
				m_Cache[id] = value;
			}
			return value;
		}

		public static void RemoveCache(string id) {
			if (m_Cache.TryGetValue(id, out var value)) {
				value.Target.Modifiers.ForEach(m => SerializedMessageSkillCheckModifier.Remove(m.Id));
				m_Cache.Remove(id);
			}
		}
	}

	[CustomPropertyDrawer(typeof(DialogueMessageSkillCheck))]
	public class DialogueMessageSkillCheckDrawer : PropertyDrawer {
		private static Rect m_ComplexityLabelRect;
		private static Rect m_ComplexityRect;
		private static Rect m_ModifiersRect;

		private static readonly GUIContent m_ComplexityLabel = new("Complexity");
		static DialogueMessageSkillCheckDrawer() {
			m_ComplexityLabelRect = new(0, 0, 80f, EditorGUIUtility.singleLineHeight);
			m_ComplexityRect = new(80f, 0, 0, EditorGUIUtility.singleLineHeight);
			m_ModifiersRect = new(0, EditorGUIUtility.singleLineHeight + 2f, 0, 0);
		}

		private void SetRects(Rect position, SerializedProperty modifiers) {
			m_ComplexityLabelRect.x = m_ModifiersRect.x = position.x;
			m_ComplexityLabelRect.y = m_ComplexityRect.y = position.y;
			m_ComplexityLabelRect.height = m_ComplexityRect.height = EditorGUIUtility.singleLineHeight;

			m_ComplexityLabelRect.width = 80f;

			m_ComplexityRect.x = m_ComplexityLabelRect.x + m_ComplexityLabelRect.width;
			m_ComplexityRect.width = position.width - m_ComplexityLabelRect.width;

			m_ModifiersRect.width = position.width;
			m_ModifiersRect.y = m_ComplexityRect.y + m_ComplexityRect.height + 2f;
			m_ModifiersRect.height = EditorGUI.GetPropertyHeight(modifiers);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var check = SerializedMessageSkillCheck.GetValue(property);
			if (check == null) return;
			SetRects(position, check.Modifiers);

			EditorGUI.LabelField(m_ComplexityLabelRect, m_ComplexityLabel);
			check.Complexity.intValue = EditorGUI.IntSlider(m_ComplexityRect, check.Complexity.intValue, 1, 20);
			check.ModifiersList.DoList(m_ModifiersRect);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var check = SerializedMessageSkillCheck.GetValue(property);
			if (check == null) return EditorGUIUtility.singleLineHeight;

			if (Event.current.type == EventType.Layout) {
				var height = EditorGUIUtility.singleLineHeight + (check.Modifiers.arraySize > 0 ? 50f : 70f);
				for (int i = 0; i < check.Modifiers.arraySize; i++) {
					height += EditorGUI.GetPropertyHeight(check.Modifiers.GetArrayElementAtIndex(i));
				}
				check.CachedHeight = height;
			}
			return check.CachedHeight;
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
					messages[i].MessageType = 1000;
				}
			}
			return messages;
		}
	}

	[Serializable]
	public class DialogueMessageNode {
		public bool IsOneOff;
		public DialogueNodeRequirements Requirements;
		public DialogueMessageNodeActions Actions;
		[TextArea] public string Message;
		public DialogueMessageType Type;
		public static DialogueMessageNode Dummy => new() { Message = null, Type = default, Actions = new(), Requirements = new() };
		public string Id = Guid.NewGuid().ToString("N");

		public static DialogueMessageNode FromMessage(DialogueMessage msg) {
			var prefs = DialogueSystemPreferences.GetOrCreateSettings();
			var node = new DialogueMessageNode() {
				IsOneOff = msg.IsOneOff,
				Message = msg.PlayerMessage,
				Type = prefs.Messages.Types.Find(t => t.Id == msg.MessageType),
				Actions = new DialogueMessageNodeActions() {
					AttitudeChange = msg.AttitudeChange,
					CustomA = new()
				},
				Requirements = new DialogueNodeRequirements() {
					Quests = msg.Requirements,
					Attitude = new Vector2Int(msg.MinAttitude, msg.MaxAttitude),
					Custom = msg.CustomRequirements?.Split(';').ToList()
				}
			};
			if (msg.MessageAction != null) {
				foreach (var action in msg.MessageAction.Split(';')) {
					node.Actions.CustomA.Add(new(action));
				}
			}
			return node;
		}

		public DialogueMessage Export() {
			Requirements.Custom.RemoveAll(c => string.IsNullOrEmpty(c));
			Actions.CustomA.RemoveAll(c => !c.IsValid);

			if (!string.IsNullOrEmpty(Message)) {
				DialogueGraphUtils.LocalizationCache.Add(Id, Message);
			}
			DialogueMessageSkillCheck skillCheck = null;
			List<DialogueMessageNodeActionCustom> customActions = new();
			Actions.CustomA.ForEach(c => {
				if (c.Type == 5) {
					skillCheck = c.SkillCheck;
					// Add all check modifiers to localization cache
					foreach (var modifier in c.SkillCheck.Modifiers) {
						DialogueGraphUtils.LocalizationCache_SkillChecks.Add(modifier.Id, modifier.Message);
					}
				}
				else {
					customActions.Add(c);
				}
			});

			return new() {
				Id = Id,
				IsOneOff = IsOneOff,
				PlayerMessage = Message,
				MessageType = Type.Id,
				Requirements = Requirements.Quests,
				MinAttitude = Requirements.Attitude.x,
				MaxAttitude = Requirements.Attitude.y,
				CustomRequirements = string.Join(';', Requirements.Custom),
				AttitudeChange = Actions.AttitudeChange,
				MessageAction = string.Join(';', customActions),
				MessageSkillCheck = skillCheck?.ToString()
			};
		}
	}
}
