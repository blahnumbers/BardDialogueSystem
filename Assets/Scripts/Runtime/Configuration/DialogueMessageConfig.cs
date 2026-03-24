using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bard.DialogueSystem;

namespace Bard.Configuration {
	[CreateAssetMenu(menuName="Bard/Configuration/New Message Types Config", order=10)]
	public class DialogueMessageConfig : ScriptableConfig {
		public List<DialogueMessageType> Types = new() { new(0, "Default") };
		[SerializeField] private int m_MaxTypeId = 1;
		public int NextTypeId => m_MaxTypeId;
		public string[] m_CachedTypeNames;
		private int[] m_CachedTypeIds;
		private Dictionary<int, int> m_TypeIdToIndex;

		public string[] TypeNames {
			get {
				if (m_CachedTypeNames == null) RebuildCaches();
				return m_CachedTypeNames;
			}
		}

		public override void RebuildCaches() {
			m_CachedTypeNames = Types.Select(t => t.Name).ToArray();
			m_CachedTypeIds = Types.Select(t => t.Id).ToArray();
			m_TypeIdToIndex = Types.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
		}

		public int GetTypeIndexById(int id) {
			if (m_TypeIdToIndex == null) RebuildCaches();
			return m_TypeIdToIndex.TryGetValue(id, out var idx) ? idx : -1;
		}

		public int GetTypeIdByIndex(int index) {
			if (m_TypeIdToIndex == null) RebuildCaches();
			return index >= 0 && index < m_CachedTypeIds.Length ? m_CachedTypeIds[index] : -1;
		}

		public void AddType() {
			Types.Add(new(m_MaxTypeId));
			m_MaxTypeId++;
		}

		public void ResetTypeCounters() {
			m_MaxTypeId = 0;
			Types.ForEach(t => m_MaxTypeId = Mathf.Max(m_MaxTypeId, t.Id + 1));
			Debug.Log("Message type counters have been reset. Next type Id: " + m_MaxTypeId);
		}
	}
}
