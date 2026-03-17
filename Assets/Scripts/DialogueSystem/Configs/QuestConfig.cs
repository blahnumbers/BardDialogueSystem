using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Bard {
	public class QuestDefinition {
		public int Id;
		public string Name;
		public string SanitizedName => GetSanitizedName();

		private string GetSanitizedName() {
			var s = Regex.Replace(Name, @"[^A-Za-z0-9_]", "");
			return char.IsDigit(s[0]) ? "_" + s : s;
		}
	}

	public class QuestConfig : ScriptableConfig {
		public List<QuestDefinition> Definitions;
		public string[] QuestNames => Definitions.Select(d => d.Name).ToArray();

		public QuestDefinition GetById(int id) {
			return Definitions.Find(d => d.Id == id);
		}
	}
}
