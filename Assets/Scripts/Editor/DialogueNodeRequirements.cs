
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Bard.XNodeEditor {
	[CustomPropertyDrawer(typeof(DialogueNodeRequirements))]
	public class DialogueNodeRequirementsDrawer : PropertyDrawer {
		private static readonly Dictionary<string, SerializedDialogueRequirement> m_Cache = new();

		private static readonly Texture2D m_BackgroundTexture;
		private static readonly GUIStyle m_HasChangesStyle;
		private static readonly GUIStyle m_DefaultStyle;
		private static readonly GUIStyle m_FoldoutOverrideStyle;
		private static readonly GUIStyle m_FoldoutStyle;
		private static Rect m_BackgroundRect;
		private static Rect m_FoldoutRect;
		private static Rect m_QuestsRect;
		private static Rect m_AttitudeToggleRect;
		private static Rect m_AttitudeRect;
		private static Rect m_CustomRect;

		public static Color BackgroundColor = Color.white;

		static DialogueNodeRequirementsDrawer() {
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
			m_QuestsRect = new(24f, EditorGUIUtility.singleLineHeight, 0, 18f);
			m_AttitudeToggleRect = new(24f, 0, 24f, EditorGUIUtility.singleLineHeight);
			m_AttitudeRect = new(48f, 0, 0, EditorGUIUtility.singleLineHeight);
			m_CustomRect = new(24f, 0, 0, 18f);
		}

		private SerializedDialogueRequirement GetCachedValue(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (!m_Cache.TryGetValue(id, out var target)) {
				target = new(property);
				m_Cache.Add(id, target);
			}
			return target;
		}

		public static void RemoveCache(SerializedProperty property) {
			var id = property.FindPropertyRelative("Id").stringValue;
			if (m_Cache.ContainsKey(id)) {
				m_Cache.Remove(id);
			}
		}

		private void SetRects(Rect position) {
			m_BackgroundRect.x = m_FoldoutRect.x = position.x;
			m_BackgroundRect.y = m_FoldoutRect.y = position.y;
			m_BackgroundRect.width = m_FoldoutRect.width = position.width;

			position.width -= 2;
			m_AttitudeRect.x = position.x + 48f;
			m_AttitudeRect.width = position.width - 48f;
			m_AttitudeRect.y = m_AttitudeToggleRect.y = position.y + EditorGUIUtility.singleLineHeight + 2f;

			m_QuestsRect.y = m_AttitudeRect.y + m_AttitudeRect.height + 6f;
			m_QuestsRect.x = m_AttitudeToggleRect.x = m_CustomRect.x = position.x + 24f;
			m_QuestsRect.width = m_CustomRect.width = position.width - 24f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SetRects(position);

			var target = GetCachedValue(property);
			var valuesDefault = target.IsDefault;
			m_BackgroundRect.height = GetPropertyHeight(property, label);
			GUI.Box(m_BackgroundRect, GUIContent.none, valuesDefault ? m_DefaultStyle : m_HasChangesStyle);

			property.isExpanded = EditorGUI.Foldout(m_FoldoutRect, property.isExpanded, label, true, valuesDefault ? m_FoldoutStyle : m_FoldoutOverrideStyle);

			if (!property.isExpanded) {
				return;
			}

			m_QuestsRect.height = EditorGUI.GetPropertyHeight(target.Quests);
			m_CustomRect.y = m_QuestsRect.y + m_QuestsRect.height + 2f;
			m_CustomRect.height = EditorGUI.GetPropertyHeight(target.Custom);

			var color = GUI.contentColor;
			if (!target.IsAttitudeDefault) {
				GUI.contentColor = Color.yellow;
			}
			var attitudeActive = GUI.Toggle(m_AttitudeToggleRect, target.AttitudeActive, GUIContent.none);
			if (target.AttitudeActive != attitudeActive) {
				target.AttitudeActive = attitudeActive;
				if (!target.AttitudeActive) {
					target.LastAttitude = target.Attitude.vector2IntValue;
					target.Attitude.vector2IntValue = new Vector2Int(int.MinValue, int.MaxValue);
				}
				else {
					target.Attitude.vector2IntValue = new Vector2Int(Mathf.Max(-100, target.LastAttitude.x), Mathf.Min(100, target.LastAttitude.y));
				}
			}
			GUI.enabled = target.AttitudeActive;;
			EditorGUI.PropertyField(m_AttitudeRect, target.Attitude);
			GUI.enabled = true;
			GUI.contentColor = color;

			if (!target.IsQuestsDefault) {
				GUI.contentColor = Color.yellow;
			}
			var bgColor = GUI.backgroundColor;
			GUI.backgroundColor = BackgroundColor;
			EditorGUI.PropertyField(m_QuestsRect, target.Quests);
			GUI.backgroundColor = bgColor;
			GUI.contentColor = color;

			if (!target.IsCustomDefault) {
				GUI.contentColor = Color.yellow;
			}
			GUI.backgroundColor = BackgroundColor;
			EditorGUI.PropertyField(m_CustomRect, target.Custom);
			GUI.backgroundColor = bgColor;
			GUI.contentColor = color;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var height = EditorGUIUtility.singleLineHeight + 2f;
			if (property == null) return height;

			var target = GetCachedValue(property);
			if (property.isExpanded) {
				height += EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(target.Quests) + EditorGUI.GetPropertyHeight(target.Custom) + 10f;
			}
			return height;
		}
	}

	[Serializable]
	public class DialogueNodeRequirements {
		public string Id = Guid.NewGuid().ToString("N");
		public List<QuestRequirementTyped> Quests;
		[MinMaxSlider(-100, 100)] public Vector2Int Attitude = new(int.MinValue, int.MaxValue);
		public List<string> Custom;
	}
}
