using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bard {
[Serializable]
	public class LocalizationLanguage {
		public string Language;
		public string ShortPath;
	}

	[CreateAssetMenu]
	public class LocalizationConfig : ScriptableObject {
		public string BasePath;
		public List<LocalizationLanguage> Languages;
		public List<string> DefaultFiles;
	}
}
