using System;

namespace Bard.QuestSystem {
	[Serializable]
	public class QuestStepBase {
		public string Description;
	}

	[Serializable]
	public class QuestStep : QuestStepBase {
		public const long PROGRESS_INVALID = -1000;
		public const long PROGRESS_UNINITIALIZED = -1;
		public const long PROGRESS_COMPLETED = (long)1 << 61;

		public long Id = 0;
		public bool IsFinal = false;
		public bool ShouldSerializeIsFinal() => IsFinal;
		public string Notification;
		public bool ShouldSerializeNotification() => !string.IsNullOrEmpty(Notification);
	}
}
