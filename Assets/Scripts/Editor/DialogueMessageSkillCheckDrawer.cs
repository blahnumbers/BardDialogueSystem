using UnityEngine;
using UnityEditor;

namespace Bard.XNodeEditor {
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
}
