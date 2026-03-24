using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bard.QuestSystem;

namespace Bard.Configuration {
	[CreateAssetMenu(menuName="Bard/Configuration/New Quests Config", order=12)]
	public class QuestConfig : ScriptableConfig {
		[SerializeField] private List<QuestTypeDefinition> m_Types = new() { new(0, "Default") };
		public IReadOnlyList<QuestTypeDefinition> Types => m_Types;
		private string[] m_CachedQuestTypes;
		private int[] m_CachedQuestTypeIds;
		private Dictionary<int, int> m_TypeIdToIndex;
		public string[] QuestTypes {
			get {
				if (m_CachedQuestTypes == null) RebuildCaches();
				return m_CachedQuestTypes;
			}
		}
		[SerializeField] private List<QuestDefinition> m_Definitions = new() { new(0, "Undefined") };
		public IReadOnlyList<QuestDefinition> Definitions => m_Definitions;
		private string[] m_CachedQuestNames;
		private int[] m_CachedQuestIds;
		private Dictionary<int, int> m_QuestIdToIndex;
		public string[] QuestNames {
			get {
				if (m_CachedQuestNames == null) RebuildCaches();
				return m_CachedQuestNames;
			}
		}
		[SerializeField] private int m_MaxId = 1;
		[SerializeField] private int m_MaxTypeId = 1;

		public override void RebuildCaches() {
			var enabledQuests = m_Definitions.Where(d => d.Enabled);
			m_CachedQuestNames = enabledQuests.Select(d => d.EditorName).ToArray();
			m_CachedQuestIds = enabledQuests.Select(d => d.Id).ToArray();
			m_QuestIdToIndex = enabledQuests.Select((d, i) => (d.Id, i)).ToDictionary(x => x.Id, x => x.i);

			m_CachedQuestTypes = m_Types.Select(t => t.EditorName).ToArray();
			m_CachedQuestTypeIds = m_Types.Select(t => t.Id).ToArray();
			m_TypeIdToIndex = m_Types.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
		}

		public int GetIndexById(int id) {
			if (m_QuestIdToIndex == null) RebuildCaches();
			return m_QuestIdToIndex.TryGetValue(id, out var idx) ? idx : -1;
		}

		public int GetIdByIndex(int index) {
			if (m_QuestIdToIndex == null) RebuildCaches();
			return index >= 0 && index < m_CachedQuestIds.Length ? m_CachedQuestIds[index] : -1;
		}

		public int GetTypeIndexById(int id) {
			if (m_TypeIdToIndex == null) RebuildCaches();
			return m_TypeIdToIndex.TryGetValue(id, out var idx) ? idx : -1;
		}

		public int GetTypeIdByIndex(int index) {
			if (m_TypeIdToIndex == null) RebuildCaches();
			return index >= 0 && index < m_CachedQuestTypeIds.Length ? m_CachedQuestTypeIds[index] : -1;
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
