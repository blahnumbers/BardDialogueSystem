using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bard {
	public class QuestConfig : ScriptableConfig {
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

		public override void RebuildCaches() {
			m_CachedQuestNames = m_Definitions.Select(d => d.Name).ToArray();
		}

		public void AddDefinition() {
			m_Definitions.Add(new(m_MaxId, ""));
			m_MaxId++;
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
