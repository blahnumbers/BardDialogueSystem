using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Bard.DialogueSystem;
using Bard.DialogueSystem.Actions;

namespace Bard.Configuration {
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

#if UNITY_EDITOR
		public override bool Initialize() {
			if (m_Initialized) return false;

			EditorApplication.delayCall += () => {
				if (m_Initialized) return;

				var path = AssetDatabase.GetAssetPath(this);
				AttachDefaultAction<QuestProgressAction>("Set Quest Progress", path);
				AttachDefaultAction<QuestConditionAction>("Set Quest Condition", path);
				AttachDefaultAction<QuestAddAction>("Add Quest", path);
				AttachDefaultAction<SkillCheckAction>("Skill Check", path);
				AttachDefaultAction<AttitudeChangeAction>("Attitude Change", path);

				base.Initialize();

				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			};
			return false;
		}
#endif

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
