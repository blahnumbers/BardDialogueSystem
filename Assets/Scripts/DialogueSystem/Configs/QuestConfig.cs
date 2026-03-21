using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bard {
	[CreateAssetMenu(menuName="Bard/Configuration/New Quests Config", order=12)]
	public class QuestConfig : ScriptableConfig {
		[SerializeField] private List<QuestTypeDefinition> m_Types = new() { new(0, "Default") };
		public IReadOnlyList<QuestTypeDefinition> Types => m_Types;
		private string[] m_CachedQuestTypes;
		public string[] QuestTypes {
			get {
				if (m_CachedQuestTypes == null) RebuildCaches();
				return m_CachedQuestTypes;
			}
		}
		[SerializeField] private List<QuestDefinition> m_Definitions = new() { new(0, "Undefined") };
		public IReadOnlyList<QuestDefinition> Definitions => m_Definitions;
		private string[] m_CachedQuestNames;
		public string[] QuestNames {
			get {
				if (m_CachedQuestNames == null) RebuildCaches();
				return m_CachedQuestNames;
			}
		}
		private int m_MaxId = 1;
		private int m_MaxTypeId = 1;

		public override void RebuildCaches() {
			m_CachedQuestNames = m_Definitions.Where(d => d.Enabled).Select(d => d.EditorName).ToArray();
			m_CachedQuestTypes = m_Types.Select(t => t.EditorName).ToArray();
		}

		public void AddType(string name = "") {
			m_Types.Add(new(m_MaxTypeId, name));
			m_MaxTypeId++;
		}

		public void AddDefinition(string name = "") {
			m_Definitions.Add(new(m_MaxId, name));
			m_MaxId++;
		}

		public void AddDefinitionIfMissing(string name) {
			if (m_Definitions.Find(d => d.InternalName == name) != null) return;
			AddDefinition(name);
		}

		public void ResetDefinitionCounters() {
			m_MaxId = 1;
			m_Definitions.ForEach(t => m_MaxId = Mathf.Max(m_MaxId, t.Id + 1));
			Debug.Log("Quest definition counters have been reset. Next definition Id: " + m_MaxId);
		}

		public QuestDefinition GetById(int id) {
			return m_Definitions.Find(d => d.Id == id);
		}
	}
}
