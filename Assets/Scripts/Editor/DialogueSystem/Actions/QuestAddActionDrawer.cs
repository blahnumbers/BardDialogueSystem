using UnityEditor;
using Bard.Configuration.Editor;
using Bard.DialogueSystem.Editor;
using Bard.Editor;

namespace Bard.DialogueSystem.Actions.Editor {
	[DialogueActionDrawer(typeof(QuestAddAction))]
	public class QuestAddActionDrawer : DialogueActionDrawer {
		public override void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs) {
			EditorGUI.LabelField(rects.Label1, "Id");
			data.CValue.intValue = BardEditorGUI.QuestPopup(rects.Input1, data.CValue.intValue, prefs);
		}
		public override float GetPropertyHeight(SerializedProperty property) {
			return base.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight + 2f;
		}
	}
}
