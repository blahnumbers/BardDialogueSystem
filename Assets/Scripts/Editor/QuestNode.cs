using UnityEditor;
using XNode;
using XNodeEditor;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(QuestNode))]
	public class QuestNodeEditor : NodeEditor {
		private QuestNode m_Target;
		private SerializedProperty m_Hint;
		private SerializedProperty m_Type;

		public override void OnCreate() {
			m_Target = target as QuestNode;
			m_Hint = serializedObject.FindProperty("InternalHint");
			m_Type = serializedObject.FindProperty("Type");
		}

		public override void OnBodyGUI() {
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(m_Type);
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
