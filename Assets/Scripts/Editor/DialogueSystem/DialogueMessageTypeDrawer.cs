
using UnityEditor;
using UnityEngine;

namespace Bard.DialogueSystem.Editor {
	[CustomPropertyDrawer(typeof(DialogueMessageType))]
	public class DialogueMessageTypeEditor : PropertyDrawer {
		private static readonly GUIContent m_Label = new("");

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.y += 2f;
			position.height -= 4f;
			var id = property.FindPropertyRelative("Id");
			m_Label.text = id.intValue.ToString();
			if (id.intValue == 0) EditorGUI.BeginDisabledGroup(true);
			var labelWidth = Mathf.Min(20f, position.width * 0.05f);
			EditorGUI.LabelField(new Rect(position.x, position.y, labelWidth, position.height), m_Label);

			var nameField = property.FindPropertyRelative("Name");
			position.x += labelWidth;
			position.width -= labelWidth;
			nameField.stringValue = EditorGUI.TextField(position, nameField.stringValue);
			if (id.intValue == 0) EditorGUI.EndDisabledGroup();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
	}
}
