

using System;
using System.Text.RegularExpressions;

namespace Bard {
	[Serializable]
	public class QuestDefinition {
		public int Id;
		public string Name;
		public string SanitizedName => GetSanitizedName();
		private string GetSanitizedName() {
			var s = Regex.Replace(Name, @"[^A-Za-z0-9_]", "");
			return char.IsDigit(s[0]) ? "_" + s : s;
		}
		public QuestDefinition(int id, string name) {
			Id = id;
			Name = name;
		}
	}
}
