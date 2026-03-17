
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bard {
	[Serializable]
	public class Quest {
		public string Id;
		public string Name;
		public List<QuestStep> Steps = new();
		public List<QuestCondition> Conditions = new();
		private readonly HashSet<string> m_ValidConditions = new();
		[JsonIgnore] public IReadOnlyCollection<string> ValidConditions => m_ValidConditions;
		[JsonIgnore] public long Progress = QuestStep.PROGRESS_UNINITIALIZED;
		public int Type = 0;
		[JsonIgnore] public bool IsStarted => Progress >= 0;
		[JsonIgnore] public bool IsCompleted {
			get {
				return Progress >= QuestStep.PROGRESS_COMPLETED;
			}
			set {
				Progress |= QuestStep.PROGRESS_COMPLETED;
			}
		}
		[JsonIgnore] public bool IsActive => IsStarted && !IsCompleted;

		public bool HasProgress(long step) {
			if (!IsActive && !IsCompleted) return false;
			if (step == 0) return true;
			return (Progress & step) > 0;
		}

		public bool HasCondition(string condition) {
			return m_ValidConditions.Contains(condition);
		}

		public bool AddCondition(string condition) {
			var target = Conditions.Find(c => string.Compare(c.Id, condition, true) == 0);
			if (target == null) return false;

			if (!IsActive) Progress = 0;
			m_ValidConditions.Add(target.Id);

			return true;
		}

		public bool AddConditionIfMissing(string condition) {
			if (HasCondition(condition)) return true;
			return AddCondition(condition);
		}

		public void Reset() {
			Progress = QuestStep.PROGRESS_UNINITIALIZED;
			m_ValidConditions.Clear();
		}
	}
}
