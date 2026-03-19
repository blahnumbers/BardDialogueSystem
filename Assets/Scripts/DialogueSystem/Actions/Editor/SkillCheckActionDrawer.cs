using System;
using UnityEditor;

namespace Bard.Editor {
	[DialogueActionDrawer(typeof(SkillCheckAction))]
	public class SkillCheckActionDrawer : DialogueActionDrawer {
		public override void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs) {
			EditorGUI.PropertyField(rects.Initial, data.SkillCheck);
		}
		public override float GetPropertyHeight(SerializedProperty property) {
			return base.GetPropertyHeight(property) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("SkillCheck")) + 2f;
		}
	}
}
