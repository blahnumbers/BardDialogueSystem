using UnityEditor;
using Bard.Configuration.Editor;
using Bard.DialogueSystem.Editor;
using Bard.QuestSystem.Editor;

namespace Bard.DialogueSystem.Actions.Editor {
	[DialogueActionDrawer(typeof(QuestConditionAction))]
	public class QuestConditionActionDrawer : DialogueActionDrawer {
		public override void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs) {
			EditorGUI.LabelField(rects.Label1, "Id");
			data.CValue.intValue = EditorGUI.Popup(rects.Input1, data.CValue.intValue, prefs.Quests.QuestNames);
			
			rects.Label2.y = rects.Input2.y = rects.Input1.y + rects.Input1.height + 2f;

			var targetClass = QuestReflection.Get(prefs.Quests.GetById(data.CValue.intValue));
			if (targetClass == null || targetClass.ConditionsNames.Length == 0) return;

			EditorGUI.LabelField(rects.Label2, "Condition");
			var conditionValue = targetClass.GetConditionIndex(data.SValue.stringValue);
			conditionValue = EditorGUI.Popup(rects.Input2, conditionValue, targetClass.ConditionsNamesDisplay);
			data.SValue.stringValue = targetClass.ConditionsNames[conditionValue];
		}
		public override float GetPropertyHeight(SerializedProperty property) {
			return base.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight * 2 + 2f;
		}
	}
}
