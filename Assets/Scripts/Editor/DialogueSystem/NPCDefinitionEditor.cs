
using UnityEditor;
using UnityEngine;

namespace Bard.DialogueSystem.Editor {
	[CustomEditor(typeof(NPCDefinition))]
	public class NPCDefinitionEditor : UnityEditor.Editor {
		private SerializedProperty m_Id;
		private SerializedProperty m_IdOffset;
		private int Id => m_Id.intValue + m_IdOffset.intValue;
		private SerializedProperty m_InternalName;
		private SerializedProperty m_EditorName;
		private SerializedProperty m_DisplayNames;

		public void OnEnable() {
			m_Id = serializedObject.FindProperty("m_Id");
			m_IdOffset = serializedObject.FindProperty("m_IdOffset");
			m_InternalName = serializedObject.FindProperty("InternalName");
			m_EditorName = serializedObject.FindProperty("EditorName");
			m_DisplayNames = serializedObject.FindProperty("DisplayNames");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			var shouldDisable = Id == 0 && m_InternalName.stringValue == "Undefined";
			if (shouldDisable) EditorGUI.BeginDisabledGroup(true);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(Id.ToString(), GUI.skin.label);
			m_InternalName.stringValue = EditorGUILayout.TextField(m_InternalName.stringValue);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.PropertyField(m_EditorName);
			EditorGUILayout.PropertyField(m_DisplayNames);

			if (shouldDisable) EditorGUI.EndDisabledGroup();
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}
