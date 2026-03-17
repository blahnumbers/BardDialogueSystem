using UnityEditor;

namespace Bard.Editor {
	[DialogueActionDrawer(typeof(QuestAddAction))]
	public partial class QuestAddActionDrawer : DialogueActionDrawer {
		public override void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs) {
			EditorGUI.LabelField(rects.Label1, "Id");
			data.CValue.intValue = EditorGUI.Popup(rects.Input1, data.CValue.intValue, prefs.Quests.QuestNames);
		}
		public override float GetPropertyHeight(SerializedProperty property) {
			return base.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight + 2f;
		}
	}
}
