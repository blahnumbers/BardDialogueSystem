using System;
using Newtonsoft.Json;

namespace Bard.QuestSystem {
	[Serializable]
	public class QuestCondition : QuestStepBase {
		[JsonIgnore] public string GUID = Guid.NewGuid().ToString("N");
		public string Id;
		public bool ShowsNotification;
		public bool ShouldSerializeNotification() => ShowsNotification;
	}
}
