using UnityEngine;
using XNode.NodeGroups;
using XNodeEditor;
using XNodeEditor.NodeGroups;

namespace Bard.XNodeEditor {
	[CustomNodeEditor(typeof(DialogueNodeGroup))]
	public class DialogueNodeGroupEditor : NodeGroupEditor {
		private static readonly GUIStyle m_HeaderStyle = new() {
			fontSize = 20,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter,
			normal = new() {
				textColor = Color.white
			}
		};

		public override void OnHeaderGUI() {
			m_HeaderStyle.fontSize = Mathf.Min(200, Mathf.RoundToInt(20 * NodeEditorWindow.current.zoom));
			group.headerHeight = m_HeaderStyle.fontSize + 20;
			GUILayout.Label(target.name, m_HeaderStyle, GUILayout.Height(group.headerHeight));
		}
	}

	public class DialogueNodeGroup : NodeGroup {
		new private void OnEnable() {
			base.OnEnable();
			headerHeight = 50;
		}
	}
}
