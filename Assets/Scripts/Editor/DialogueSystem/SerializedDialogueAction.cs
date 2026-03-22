using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using Bard.DialogueSystem.Editor;

namespace Bard.XNodeEditor {
	public class SerializedDialogueAction {
		private static readonly Dictionary<string, SerializedDialogueAction> m_Cache = new();
		public readonly DialogueMessageNodeActions Target = null;
		public SerializedProperty Property;
		public SerializedProperty Attitude;
		public SerializedProperty Custom;
		public ReorderableList CustomList;
		private readonly List<SerializedProperty> m_CustomListElements = new();
		private bool m_CustomListDirty = true;
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
			Custom = prop.FindPropertyRelative("Custom");
			
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
					if (!DialogueGraphUtils.Overlaps(rect)) return;

					ReloadCachedListIfNeeded();
					EditorGUI.PropertyField(rect, m_CustomListElements[index]);
				},
				elementHeightCallback = index => {
					ReloadCachedListIfNeeded();
					return EditorGUI.GetPropertyHeight(m_CustomListElements[index]);
				},
				onAddCallback = list => {
					Target.Custom.Add(new());
					prop.serializedObject.Update();
					m_CustomListDirty = true;
				},
				onRemoveCallback = list => {
					Target.Custom.RemoveAt(list.selectedIndices.Count > 0 ? list.selectedIndices[0] : 0);
					prop.serializedObject.Update();
					m_CustomListDirty = true;
				}
			};
		}

		private void ReloadCachedListIfNeeded() {
			if (!m_CustomListDirty) return;

			m_CustomListElements.Clear();
			for (int i = 0; i < Custom.arraySize; i++) {
				m_CustomListElements.Add(Custom.GetArrayElementAtIndex(i));
			}
			m_CustomListDirty = false;
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
				if (value.Target != null && value.Target.Custom != null) {
					foreach (var action in value.Target.Custom) {
						if (action.SkillCheck != null) {
							SerializedMessageSkillCheck.RemoveCache(action.SkillCheck.Id);
						}
						SerializedMessageAction.Remove(action.Id);
					}
				}
				m_Cache.Remove(id);
			}
		}
	}
}
