using System;

namespace Bard.QuestSystem {
	[Serializable]
	public class QuestRequirementTyped : QuestRequirement {
		public bool IsFailure = false;
		public bool ShouldSerializeIsFailure() => IsFailure;
	}

	[Serializable]
	public class QuestRequirement {
		public int Id = 0;
		public long Step = QuestStep.PROGRESS_UNINITIALIZED;
		public bool ShouldSerializeStep() => Step > QuestStep.PROGRESS_UNINITIALIZED;
		public string Condition = null;
		public bool ShouldSerializeCondition() => !string.IsNullOrEmpty(Condition);

		public bool IsValid() {
			return Id != 0 && ((Step > QuestStep.PROGRESS_UNINITIALIZED) ^ (!string.IsNullOrEmpty(Condition)));
		}
	}
}
