using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;
using Bard.QuestSystem;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(QuestStepNode))]
	public class QuestStepNodeEditor : NodeEditor {
		private QuestStepNode m_Target;
		private SerializedProperty m_Hint;
		private SerializedProperty m_Description;
		private SerializedProperty m_Notification;
		private SerializedProperty m_IsFinal;
		private GUIContent m_DescriptionLabel;
		private GUIContent m_DescriptionContent;

		public override void OnHeaderGUI() {
			if (m_Target != null) {
				GUILayout.Label($"{target.name} [{m_Target.Id}]", NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
				return;
			}
			base.OnHeaderGUI();
		}

		public override void OnCreate() {
			m_Target = target as QuestStepNode;
			m_Hint = serializedObject.FindProperty("InternalHint");
			m_Description = serializedObject.FindProperty("Description");
			m_Notification = serializedObject.FindProperty("Notification");
			m_IsFinal = serializedObject.FindProperty("IsFinal");
			m_DescriptionLabel = new GUIContent("Description (shown in Quest log)");
			m_DescriptionContent = new GUIContent();
		}

		public override void OnBodyGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_IsFinal);
			EditorGUILayout.PropertyField(m_Hint);
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField(m_DescriptionLabel);

			var rect = GUILayoutUtility.GetLastRect();
			rect.y += rect.height;
			m_DescriptionContent.text = m_Description.stringValue;
			rect.height = Mathf.Max(EditorGUIUtility.singleLineHeight * 2f, GUI.skin.textArea.CalcHeight(m_DescriptionContent, 348) + 8f);
			m_Description.stringValue = EditorGUI.TextArea(rect, m_Description.stringValue, GUI.skin.textArea);
			EditorGUILayout.Space(rect.height);

			EditorGUILayout.PropertyField(m_Notification);

			serializedObject.ApplyModifiedProperties();
		}

		public override void OnRename() {
			m_Target.Name = target.name;
		}
	}

	[NodeWidth(380)]
	public class QuestStepNode : Node {
		[Input] public Node Input;
		[Output] public QuestStepNode Output;

		public long Id;
		public string Name;
		public string InternalHint;
		public string Description;
		public string Notification;
		public bool IsFinal;

		public override object GetValue(NodePort port) => null;

		protected override void Init() {
			if (Id == 0) {
				var steps = graph.nodes.OfType<QuestStepNode>().OrderBy(c => -c.Id).ToList();
				if (steps.Count > 0) {
					Id = steps[0].Id << 1;
				}
				else {
					Id = 1;
				}
			}	
		}

		public void Setup(QuestStep step) {
			Id = step.Id;
			Description = step.Description;
			Notification = step.Notification;
			IsFinal = step.IsFinal;
		}
	}
}
