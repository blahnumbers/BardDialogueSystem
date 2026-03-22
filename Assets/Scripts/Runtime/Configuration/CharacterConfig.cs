using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bard.DialogueSystem;
using UnityEditor;

namespace Bard.Configuration {
	[CreateAssetMenu(menuName="Bard/Configuration/New Characters Config", order=13)]
	public class CharacterConfig : ScriptableConfig {
		[SerializeField] private List<NPCDefinition> m_Definitions = new();
		public IReadOnlyList<NPCDefinition> Definitions => m_Definitions;
		private string[] m_CachedNames;
		public string[] CharacterNames {
			get {
				if (m_CachedNames == null) RebuildCaches();
				return m_CachedNames;
			}
		}
		private int m_MaxId = 0;

		public override void RebuildCaches() {
			m_CachedNames = m_Definitions.Select(d => d.EditorName).ToArray();
		}

#if UNITY_EDITOR
		public override bool Initialize() {
			if (m_Initialized) return false;

			EditorApplication.delayCall += () => {
				if (m_Initialized) return;
				AddDefinition("Undefined");
				base.Initialize();
			};
			return false;
		}
#endif

		public void AddDefinition(string name) {
			var npcAsset = CreateInstance<NPCDefinition>();
			npcAsset.name = name;
			npcAsset.Initialize(m_MaxId, name);

			AssetDatabase.AddObjectToAsset(npcAsset, AssetDatabase.GetAssetPath(this));

			m_Definitions.Add(npcAsset);

			EditorUtility.SetDirty(npcAsset);
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

			m_MaxId++;
		}

		public void DeleteDefinitionAtIndex(int id) {
			var definition = m_Definitions[id];
			m_Definitions.RemoveAt(id);

			AssetDatabase.RemoveObjectFromAsset(definition);
			Undo.DestroyObjectImmediate(definition);

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		public void ResetDefinitionCounters() {
			m_MaxId = 1;
			m_Definitions.ForEach(t => m_MaxId = Mathf.Max(m_MaxId, t.Id + 1));
			Debug.Log("Character definition counters have been reset. Next definition Id: " + m_MaxId);
		}

		public NPCDefinition GetById(int id) {
			return m_Definitions.Find(d => d.Id == id);
		}
	}
}
