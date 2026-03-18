
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Bard.Quests;

namespace Bard.Editor {
	public class QuestReflection {
		private static Assembly m_TargetAssembly;
		private static readonly Regex m_NameUnderlineRegex = new(@"(\w)_([A-Z])");
		private static readonly Regex m_NameRegex = new("([a-z])([A-Z0-9])");
		private static readonly Dictionary<string, QuestReflection> m_Cache = new();

		public Type BaseType;
		public Type StepsType;
		public string[] StepsNames;
		public Array StepsValues;
		public Type ConditionsType;
		public string[] ConditionsNames;
		private string[] m_ConditionsNamesDisplay;
		public string[] ConditionsNamesDisplay {
			get {
				if (m_ConditionsNamesDisplay == null) {
					m_ConditionsNamesDisplay = new string[ConditionsNames.Length];
					for (int i = 0; i < ConditionsNames.Length; i++) {
						if (m_NameUnderlineRegex.IsMatch(ConditionsNames[i])) {
							m_ConditionsNamesDisplay[i] = m_NameRegex.Replace(m_NameUnderlineRegex.Replace(ConditionsNames[i], "$1/$2"), "$1 $2");
						}
						else {
							m_ConditionsNamesDisplay[i] = m_NameRegex.Replace(m_NameRegex.Replace(ConditionsNames[i], "$1/$2", 1), "$1 $2");
						}
					}
				}
				return m_ConditionsNamesDisplay;
			}
		}

		public static QuestReflection Get(QuestDefinition definition) {
			return Get(definition?.InternalName);
		}

		public static QuestReflection Get(string name) {
			if (string.IsNullOrEmpty(name)) return null;

			if (m_TargetAssembly == null) {
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
					var types = assembly.GetTypes();
					if (types.Contains(typeof(Template))) {
						m_TargetAssembly = assembly;
					}
				}
			}
			if (m_Cache.TryGetValue(name, out var value)) {
				return value;
			}
			try {
				value = new(name, m_TargetAssembly);
				m_Cache.Add(name, value);
			}
			catch {}
			return value;
		}

		public QuestReflection(string name, Assembly assembly) {
			BaseType = assembly.GetType("Bard.Quests." + name);
			if (BaseType == null) throw new Exception("Class not found");
			StepsType = BaseType.GetNestedType("Steps");
			ConditionsType = BaseType.GetNestedType("Conditions");

			StepsNames = StepsType.GetEnumNames();
			StepsValues = StepsType.GetEnumValues();
			ConditionsNames = ConditionsType.GetEnumNames();
		}

		public string GetCondition(int index) {
			return Enum.ToObject(ConditionsType, index).ToString();
		}

		public int GetConditionIndex(string value) {
			for (int i = 0; i < ConditionsNames.Length; i++) {
				if (ConditionsNames[i] == value) {
					return i;
				}
			}
			return 0;
		}

		public int GetStepIndex(int value) {
			for (int i = 0; i < StepsValues.Length; i++) {
				var stepvalue = (int)StepsValues.GetValue(i);
				if (stepvalue == value) {
					return i;
				}
			}
			return 0;
		}

		public Enum GetStepValue(int value) {
			var val = Enum.ToObject(StepsType, value);
			return (Enum)val;
		}
	}
}
