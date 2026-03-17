using System;
using UnityEditor;
using UnityEngine;

namespace Bard.Editor {
	public struct MessageActionRects {
		public Rect Label1;
		public Rect Input1;
		public Rect Label2;
		public Rect Input2;

		public MessageActionRects(float height) {
			Label1 = new(0, height, 0, height);
			Input1 = new(0, height, 0, height);
			Label2 = new(0, height, 0, height);
			Input2 = new(0, height, 0, height);
		}
	}

	public abstract class DialogueActionDrawer {
		public abstract void DrawInspector(SerializedMessageAction data, MessageActionRects rects, DialogueProjectSettings prefs);
		public virtual float GetPropertyHeight(SerializedProperty property) => EditorGUIUtility.singleLineHeight;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DialogueActionDrawerAttribute : Attribute {
		public Type ActionType { get; }
		public DialogueActionDrawerAttribute(Type actionType) => ActionType = actionType;
	}
}
