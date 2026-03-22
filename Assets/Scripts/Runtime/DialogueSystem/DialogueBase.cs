using System;
using System.Collections.Generic;
using Bard.QuestSystem;

namespace Bard.DialogueSystem {
	[Serializable]
	public class DialogueBase {
		public List<QuestRequirementTyped> Requirements = null;
		public bool ShouldSerializeRequirements() => Requirements != null && Requirements.Count > 0;
		public string CustomRequirements;
		public bool ShouldSerializeCustomRequirements() => !string.IsNullOrEmpty(CustomRequirements);
		public int MinAttitude = int.MinValue;
		public bool ShouldSerializeMinAttitude() => MinAttitude != int.MinValue;
		public int MaxAttitude = int.MaxValue;
		public bool ShouldSerializeMaxAttitude() => MaxAttitude != int.MaxValue;
	}
}
