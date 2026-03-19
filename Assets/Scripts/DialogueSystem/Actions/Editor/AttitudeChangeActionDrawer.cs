using System;
using UnityEditor;

namespace Bard.Editor {
	[DialogueActionDrawer(typeof(AttitudeChangeAction))]
	public class AttitudeChangeActionDrawer : DialogueActionDrawer {
		public override void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs) {
			EditorGUI.LabelField(rects.Label1, "Target NPC");
			data.CValue.intValue = EditorGUI.Popup(rects.Input1, data.CValue.intValue, prefs.Characters.CharacterNames);
			
			rects.Label2.y = rects.Input2.y = rects.Input1.y + rects.Input1.height + 2f;

			EditorGUI.LabelField(rects.Label2, "Change");
			data.IValue.intValue = EditorGUI.IntField(rects.Input2, data.IValue.intValue);
		}
		public override float GetPropertyHeight(SerializedProperty property) {
			return base.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight * 2 + 2f;
		}
	}
}
