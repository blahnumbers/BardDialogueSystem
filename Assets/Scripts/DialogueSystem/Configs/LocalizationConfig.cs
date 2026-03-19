using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bard {
[Serializable]
	public class LocalizationLanguage {
		public string Language;
		public string ShortPath;
		public LocalizationLanguage(string language, string shortPath) {
			Language = language;
			ShortPath = shortPath;
		}
	}

	[CreateAssetMenu(menuName="Bard/Configuration/New Localization Config", order=13)]
	public class LocalizationConfig : ScriptableConfig {
		public List<LocalizationLanguage> Languages = new() { new("English", "en") };
		public LocalizationLanguage DefaultLanguage => Languages[0];
		public List<string> AdditionalFiles = new() { "SkillChecks" };
	}
}
