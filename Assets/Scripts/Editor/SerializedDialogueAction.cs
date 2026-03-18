using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace Bard.XNodeEditor {
	public class SerializedDialogueAction {
		private static readonly Dictionary<string, SerializedDialogueAction> m_Cache = new();
		public readonly DialogueMessageNodeActions Target = null;
		public SerializedProperty Property;
		public SerializedProperty Attitude;
		public SerializedProperty Custom;
		public ReorderableList CustomList;
		public float CachedHeight = 0f;

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


		public static SerializedDialogueAction GetValue(SerializedProperty property) {
			string id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var target)) {
				target = new(property);
				m_Cache.Add(id, target);
			}
			return target;
		}

		public static void RemoveCache(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (m_Cache.TryGetValue(id, out var value)) {
				if (value.Target != null && value.Target.CustomA != null) {
					foreach (var action in value.Target.CustomA) {
						if (action.SkillCheck != null) {
							SerializedMessageSkillCheck.RemoveCache(action.SkillCheck.Id);
						}
					}
				}
				m_Cache.Remove(id);
			}
		}
	}
}
