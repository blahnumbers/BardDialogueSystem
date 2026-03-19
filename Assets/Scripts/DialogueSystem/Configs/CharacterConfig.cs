using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bard {
	[CreateAssetMenu(menuName="Bard/Configuration/New Characters Config", order=13)]
	public class CharacterConfig : ScriptableConfig {
		[SerializeField] private List<NPCDefinition> m_Definitions = new() { new(0, "Undefined") };
		public IReadOnlyList<NPCDefinition> Definitions => m_Definitions;
		private string[] m_CachedNames;
		public string[] CharacterNames {
			get {
				if (m_CachedNames == null) RebuildCaches();
				return m_CachedNames;
			}
		}
		private int m_MaxId = 1;

		public override void RebuildCaches() {
			m_CachedNames = m_Definitions.Select(d => d.EditorName).ToArray();
		}

		public void AddDefinition(string name = "") {
			m_Definitions.Add(new(m_MaxId, name));
			m_MaxId++;
		}

		public void ResetDefinitionCounters() {
			m_MaxId = 1;
			m_Definitions.ForEach(t => m_MaxId = Mathf.Max(m_MaxId, t.Id + 1));
			Debug.Log("Character definition counters have been reset. Next definition Id: " + m_MaxId);
		}

		public NPCDefinition GetById(int id) {
			return m_Definitions.Find(d => d.Id == id);
		}
	}
}
