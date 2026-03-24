using UnityEngine;
using UnityEditor;
using Bard.Configuration;
using Bard.Configuration.Editor;

namespace Bard.Editor {
	public static class BardEditorGUI {
		public static int QuestPopup(Rect rect, int currentId, DialogueProjectSettings prefs) {
			return QuestPopup(rect, currentId, prefs.Quests);
		}
		
		public static int QuestPopup(Rect rect, int currentId, QuestConfig questPrefs) {
			var curIndex = questPrefs.GetIndexById(currentId);
			var newIndex = EditorGUI.Popup(rect, curIndex, questPrefs.QuestNames);
			return newIndex != curIndex ? questPrefs.GetIdByIndex(newIndex) : currentId;
		}

		public static int QuestPopup(int currentId, DialogueProjectSettings prefs) {
			return QuestPopup(currentId, prefs.Quests);
		}
		
		public static int QuestPopup(int currentId, QuestConfig questPrefs) {
			var curIndex = questPrefs.GetIndexById(currentId);
			var newIndex = EditorGUILayout.Popup(curIndex, questPrefs.QuestNames);
			return newIndex != curIndex ? questPrefs.GetIdByIndex(newIndex) : currentId;
		}

		public static int QuestTypePopup(Rect rect, int currentId, DialogueProjectSettings prefs) {
			return QuestTypePopup(rect, currentId, prefs.Quests);
		}
		
		public static int QuestTypePopup(Rect rect, int currentId, QuestConfig questPrefs) {
			var curIndex = questPrefs.GetTypeIndexById(currentId);
			var newIndex = EditorGUI.Popup(rect, curIndex, questPrefs.QuestTypes);
			return newIndex != curIndex ? questPrefs.GetTypeIdByIndex(newIndex) : currentId;
		}

		public static int QuestTypePopup(int currentId, DialogueProjectSettings prefs) {
			return QuestTypePopup(currentId, prefs.Quests);
		}
		
		public static int QuestTypePopup(int currentId, QuestConfig questPrefs) {
			var curIndex = questPrefs.GetTypeIndexById(currentId);
			var newIndex = EditorGUILayout.Popup(curIndex, questPrefs.QuestTypes);
			return newIndex != curIndex ? questPrefs.GetTypeIdByIndex(newIndex) : currentId;
		}

		public static int NpcPopup(Rect rect, int currentId, DialogueProjectSettings prefs) {
			return NpcPopup(rect, currentId, prefs.Characters);
		}
		
		public static int NpcPopup(Rect rect, int currentId, CharacterConfig charPrefs) {
			var curIndex = charPrefs.GetIndexById(currentId);
			var newIndex = EditorGUI.Popup(rect, curIndex, charPrefs.CharacterNames);
			return newIndex != curIndex ? charPrefs.GetIdByIndex(newIndex) : currentId;
		}

		public static int NpcPopup(int currentId, DialogueProjectSettings prefs) {
			return NpcPopup(currentId, prefs.Characters);
		}
		
		public static int NpcPopup(int currentId, CharacterConfig charPrefs) {
			var curIndex = charPrefs.GetIndexById(currentId);
			var newIndex = EditorGUILayout.Popup(curIndex, charPrefs.CharacterNames);
			return newIndex != curIndex ? charPrefs.GetIdByIndex(newIndex) : currentId;
		}

		public static int MessageTypePopup(Rect rect, int currentId, DialogueProjectSettings prefs) {
			return MessageTypePopup(rect, currentId, prefs.Messages);
		}

		public static int MessageTypePopup(Rect rect, int currentId, DialogueMessageConfig msgConfig) {
			var curIndex = msgConfig.GetTypeIndexById(currentId);
			var newIndex = EditorGUI.Popup(rect, curIndex, msgConfig.TypeNames);
			return newIndex != curIndex ? msgConfig.GetTypeIdByIndex(newIndex) : currentId;
		}

		public static int MessageTypePopup(int currentId, DialogueProjectSettings prefs) {
			return MessageTypePopup(currentId, prefs.Messages);
		}

		public static int MessageTypePopup(int currentId, DialogueMessageConfig msgConfig) {
			var curIndex = msgConfig.GetTypeIndexById(currentId);
			var newIndex = EditorGUILayout.Popup(curIndex, msgConfig.TypeNames);
			return newIndex != curIndex ? msgConfig.GetTypeIdByIndex(newIndex) : currentId;
		}
	}
}
