using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using Bard.DialogueSystem;
using Bard.DialogueSystem.Actions;

namespace Bard.XNodeEditor {
	public class SerializedMessageSkillCheck {
		private static readonly Dictionary<string, SerializedMessageSkillCheck> m_Cache = new();
		private readonly bool m_PreventCache = false;
		public float CachedHeight = 0f;
		public readonly DialogueMessageSkillCheck Target = null;
		public readonly SerializedProperty Complexity;
		public readonly SerializedProperty Modifiers;
		public readonly ReorderableList ModifiersList;

		public SerializedMessageSkillCheck(SerializedProperty property, string id) {
			if (property.serializedObject.targetObject is SkillCheckAction) {
				var skillAction = property.serializedObject.targetObject as SkillCheckAction;
				Target = skillAction.SkillCheck;
				m_PreventCache = true;
			}
			else {
				var messageNode = property.serializedObject.targetObject as DialogueMessageBlockNode;
				foreach (var message in messageNode.Messages) {
					foreach (var action in message.Actions.Custom) {
						if (action.SkillCheck.Id == id) {
							Target = action.SkillCheck;
							break;
						}
					}
					if (Target != null) break;
				}
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
				if (!value.m_PreventCache) m_Cache[id] = value;
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
}
