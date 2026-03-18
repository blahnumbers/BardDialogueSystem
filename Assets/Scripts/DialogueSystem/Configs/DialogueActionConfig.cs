

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bard {
	[Serializable]
	public class DialogueActionConfigEntry {
		public int Id;
		public DialogueAction Action;
		public DialogueActionConfigEntry(int id, DialogueAction action = null) {
			Id = id;
			Action = action;
		}
	}

	[CreateAssetMenu(menuName="Bard/Configuration/New Message Actions Config", order=11)]
	public class DialogueActionConfig : ScriptableConfig {
		[SerializeField] private List<DialogueActionConfigEntry> m_Types = new() { null };
		public IReadOnlyList<DialogueActionConfigEntry> Types => m_Types;
		public string[] m_CachedActionNames;
		public string[] ActionNames {
			get {
				if (m_CachedActionNames == null) RebuildCaches();
				return m_CachedActionNames;
			}
		}

		public override void RebuildCaches() {
			m_CachedActionNames = Types.Select(t => t.Action != null ? t.Action.Name : "Undefined").ToArray();
		}

		public override void Initialize(string path) {
			AttachDefaultAction<QuestProgressAction>("Set Quest Progress", path);
			AttachDefaultAction<QuestConditionAction>("Set Quest Condition", path);
			AttachDefaultAction<QuestAddAction>("Add Quest", path);
		}

		private void AttachDefaultAction<T>(string name, string path) where T : DialogueAction {
			var action = CreateInstance<T>();
			action.name = name;
			AssetDatabase.AddObjectToAsset(action, path);
			m_Types.Add(new(m_Types.Count, action));
		}

		public bool TryGetById(int id, out DialogueAction action) {
			action = m_Types.Find(t => t.Id == id)?.Action;
			return action != null;
		}

		public void AddType() {
			m_Types.Add(new(m_Types.Count));
		}
	}
}
