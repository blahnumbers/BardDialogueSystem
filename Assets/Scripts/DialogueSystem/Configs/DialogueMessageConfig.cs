using System.Collections.Generic;
using UnityEngine;

namespace Bard {
	[CreateAssetMenu(menuName="Bard/Configuration/New Message Types Config", order=10)]
	public class DialogueMessageConfig : ScriptableConfig {
		public List<DialogueMessageType> Types = new() { default };
		private int m_MaxTypeId = 1;
		public int NextTypeId => m_MaxTypeId;

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
