using UnityEditor;
using XNode;
using XNodeEditor;
using Bard.Configuration.Editor;
using Bard.QuestSystem;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(QuestNode))]
	public class QuestNodeEditor : NodeEditor {
		private SerializedProperty m_Hint;
		private SerializedProperty m_Type;

		public override void OnCreate() {
			m_Hint = serializedObject.FindProperty("InternalHint");
			m_Type = serializedObject.FindProperty("Type");
		}

		public override void OnBodyGUI() {
			serializedObject.Update();
			
			var qPrefs = DialogueSystemPreferences.GetOrCreateSettings().Quests;
			m_Type.intValue = EditorGUILayout.Popup("Quest Type", m_Type.intValue, qPrefs.QuestTypes);
			EditorGUILayout.PropertyField(m_Hint);

			serializedObject.ApplyModifiedProperties();
		}
	}

	[NodeWidth(380)]
	[DisallowMultipleNodes]
	public class QuestNode : Node {
		[Output] public QuestConditionsNode Conditions;
		[Output] public QuestStepNode Steps;

		public string InternalHint;
		public int Type;

		public override object GetValue(NodePort port) => null;

		public void Setup(Quest quest) {
			name = quest.Name;
		}
	}
}
