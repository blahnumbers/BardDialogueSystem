using System;
using UnityEditor;

namespace Bard.Editor {
	public partial class QuestProgressActionDrawer : DialogueActionDrawer {
		public override void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs) {
			EditorGUI.LabelField(rects.Label1, "Id");
			data.CValue.intValue = EditorGUI.Popup(rects.Input1, data.CValue.intValue, prefs.Quests.QuestNames);
			
			rects.Label2.y = rects.Input2.y = rects.Input1.y + rects.Input1.height + 2f;

			var targetClass = QuestReflection.Get(prefs.Quests.GetById(data.CValue.intValue));
			if (targetClass == null) return;

			EditorGUI.LabelField(rects.Label2, "Step");
			var stepValue = targetClass.GetStepValue(data.IValue.intValue);
			data.IValue.intValue = Convert.ToInt32(EditorGUI.EnumPopup(rects.Input2, stepValue));
		}
		public override float GetPropertyHeight(SerializedProperty property) {
			return base.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight * 2 + 2f;
		}
	}
}
