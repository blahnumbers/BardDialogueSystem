

using System;
using System.Collections.Generic;
using Bard.Editor;
using UnityEditor;
using UnityEngine;

namespace Bard.XNodeEditor {
	[CustomPropertyDrawer(typeof(DialogueMessageNodeActions))]
	public class DialogueMessageNodeActionsDrawer : PropertyDrawer {
		private static readonly Dictionary<string, SerializedDialogueAction> m_Cache = new();

		private static readonly Texture2D m_BackgroundTexture;
		private static readonly GUIStyle m_HasChangesStyle;
		private static readonly GUIStyle m_DefaultStyle;
		private static readonly GUIStyle m_FoldoutOverrideStyle;
		private static readonly GUIStyle m_FoldoutStyle;

		private static Rect m_BackgroundRect;
		private static Rect m_FoldoutRect;
		private static Rect m_AttitudeRect;
		private static Rect m_AttitudeFieldRect;
		private static Rect m_CustomRect;

		public static Color BackgroundColor = Color.white;
		static DialogueMessageNodeActionsDrawer() {
			m_BackgroundTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
			m_BackgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.25f));
			m_BackgroundTexture.Apply();
			m_HasChangesStyle = new() {
				fontStyle = FontStyle.Bold,
				normal = { background = m_BackgroundTexture }
			};
			m_DefaultStyle = new();
			m_FoldoutOverrideStyle = new GUIStyle(EditorStyles.foldout) {
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.yellow }
			};
			m_FoldoutStyle = new GUIStyle(EditorStyles.foldout);

			m_BackgroundRect = new(0, 0, 0, 18f);
			m_FoldoutRect = new(0, 0, 0, EditorGUIUtility.singleLineHeight);
			m_AttitudeRect = new(24f, EditorGUIUtility.singleLineHeight, 0, EditorGUIUtility.singleLineHeight);
			m_AttitudeFieldRect = new(0, EditorGUIUtility.singleLineHeight, 80f, EditorGUIUtility.singleLineHeight);
			m_CustomRect = new(24f, EditorGUIUtility.singleLineHeight * 2 + 2f, 0, 0);
		}

		private SerializedDialogueAction GetCachedValue(SerializedProperty property) {
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

		private void SetRects(Rect position) {
			m_BackgroundRect.x = m_FoldoutRect.x = position.x;
			m_BackgroundRect.y = m_FoldoutRect.y = position.y;
			m_BackgroundRect.width = position.width;

			position.width -= 2;
			m_FoldoutRect.width = position.width;

			m_AttitudeRect.x = position.x + 24f;
			m_AttitudeFieldRect.x = position.x + position.width - 80f;
			m_AttitudeRect.y = m_AttitudeFieldRect.y = position.y + EditorGUIUtility.singleLineHeight;
			m_AttitudeRect.width = position.width - 104f;

			m_CustomRect.x = position.x + 24f;
			m_CustomRect.y = position.y + EditorGUIUtility.singleLineHeight * 2f + 2f;
			m_CustomRect.width = position.width - 24f;
			m_CustomRect.height = position.height - EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SetRects(position);

			var m_Target = GetCachedValue(property);
			var valuesDefault = m_Target.IsDefault;
			m_BackgroundRect.height = GetPropertyHeight(property, label);
			GUI.Box(m_BackgroundRect, GUIContent.none, valuesDefault ? m_DefaultStyle : m_HasChangesStyle);

			property.isExpanded = EditorGUI.Foldout(m_FoldoutRect, property.isExpanded, label, true, valuesDefault ? m_FoldoutStyle : m_FoldoutOverrideStyle);

			if (!property.isExpanded) {
				return;
			}

			var color = GUI.contentColor;
			if (!m_Target.IsAttitudeDefault) {
				GUI.contentColor = Color.yellow;
			}
			GUI.Label(m_AttitudeRect, "Attitude Change");
			m_Target.Attitude.intValue = EditorGUI.IntField(m_AttitudeFieldRect, m_Target.Attitude.intValue);
			GUI.contentColor = color;

			if (!m_Target.IsCustomDefault) {
				GUI.contentColor = Color.yellow;
			}
			var bgColor = GUI.backgroundColor;
			GUI.backgroundColor = BackgroundColor;
			m_Target.CustomList.DoList(m_CustomRect);
			GUI.backgroundColor = bgColor;
			GUI.contentColor = color;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if (property == null) {
				return EditorGUIUtility.singleLineHeight;
			}
			var m_Target = GetCachedValue(property);
			var height = EditorGUIUtility.singleLineHeight + 2;
			if (property.isExpanded) {
				height += EditorGUIUtility.singleLineHeight + 2 + (m_Target.Custom.arraySize > 0 ? m_Target.CustomList.GetHeight() : 70f);
			}
			return height;
		}
	}

	[CustomPropertyDrawer(typeof(DialogueMessageNodeActionCustom))]
	public class DialogueMessageNodeActionCustomDrawer : PropertyDrawer {
		private static readonly DialogueProjectSettings m_Prefs;
		private static DialogueActionConfig ActionsConfig => m_Prefs.MessageActions;
		private static Rect m_BaseRect;
		private static MessageActionRects m_Rects;

		static DialogueMessageNodeActionCustomDrawer() {
			m_Prefs = DialogueSystemPreferences.GetOrCreateSettings();
			m_BaseRect = new(0, 0, 0, EditorGUIUtility.singleLineHeight);
			m_Rects = new(EditorGUIUtility.singleLineHeight);
		}

		private void SetRects(Rect position) {
			m_BaseRect.x = m_Rects.Label1.x = m_Rects.Label2.x = position.x;
			m_BaseRect.y = position.y;
			m_BaseRect.width = position.width;

			m_Rects.Label1.y = m_Rects.Input1.y = m_Rects.Label2.y = m_Rects.Input2.y = m_BaseRect.y + m_BaseRect.height + 2;
			m_Rects.Label1.width = m_Rects.Label2.width = position.width / 3 - 4;
			m_Rects.Input1.width = m_Rects.Input2.width = position.width - m_Rects.Label1.width - 4;

			m_Rects.Input1.x = m_Rects.Input2.x = m_Rects.Label1.x + m_Rects.Label1.width + 4;

			m_BaseRect.height = m_Rects.Label1.height = m_Rects.Input1.height = m_Rects.Label2.height = m_Rects.Input2.height = EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SetRects(position);

			var data = SerializedMessageAction.GetValue(property);
			data.Type.intValue = EditorGUI.Popup(m_BaseRect, data.Type.intValue, ActionsConfig.ActionNames);
			if (ActionsConfig.TryGetById(data.Type.intValue, out var action)) {
				var drawer = DialogueActionDrawerRegistry.GetDrawer(action.GetType());
				drawer?.DrawInspector(data, m_Rects, m_Prefs);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var data = SerializedMessageAction.GetValue(property);
			if (data == null) return EditorGUIUtility.singleLineHeight;
			
			if (Event.current.type == EventType.Layout) {
				DialogueActionDrawer drawer = null;
				if (ActionsConfig.TryGetById(data.Type.intValue, out var action)) {
					drawer = DialogueActionDrawerRegistry.GetDrawer(action.GetType());
				}
				data.CachedHeight = drawer != null ? drawer.GetPropertyHeight(property) : EditorGUIUtility.singleLineHeight;
			}
			return data.CachedHeight;
		}
	}

	[Serializable]
	public class DialogueMessageNodeActionCustom {
		public string Id = Guid.NewGuid().ToString("N");
		public int Type = 0;
		public int CValue = 0;
		public int IValue = 0;
		public string SValue = string.Empty;
		public DialogueMessageSkillCheck SkillCheck = new() { Complexity = 10 };
		public bool IsValid => Type != 0;

		public DialogueMessageNodeActionCustom(string input) =>
			throw new NotImplementedException($"DialogueMessageNodeActionCustom construction from string is not yet implemented");

		/*public override string ToString() {
			return Type switch {
				DialogueMessageNodeActionType.QuestProgress => "ProgressQuest:" + QuestId.ToString() + ":" + IValue,
				DialogueMessageNodeActionType.QuestCondition => "AddCondition:" + QuestId.ToString() + ":" + SValue,
				DialogueMessageNodeActionType.QuestAdd => "AddQuest:" + QuestId.ToString(),
				DialogueMessageNodeActionType.PlayCinematic => "PlayCinematic:" + CinematicId.ToString(),
				DialogueMessageNodeActionType.PlayLute => "PlayLute:" + SValue,
				DialogueMessageNodeActionType.AttitudeChange => "AttitudeChange:" + SValue + ":" + IValue,
				DialogueMessageNodeActionType.BalanceChange => "UpdateBalance:" + IValue,
				DialogueMessageNodeActionType.HungerChange => "UpdateHunger:" + IValue,
				DialogueMessageNodeActionType.NameChange => "NameChange:" + SValue + ":" + IValue,
				DialogueMessageNodeActionType.HideInteractable => "Hide:" + SValue,
				DialogueMessageNodeActionType.ShowInteractable => "Show:" + SValue,
				_ => "",
			};
		}*/
	}

	[Serializable]
	public class DialogueMessageNodeActions {
		public string Id = Guid.NewGuid().ToString("N");
		public int AttitudeChange = 0;
		public List<DialogueMessageNodeActionCustom> CustomA = new();
	}
}
