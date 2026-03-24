using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bard.DialogueSystem;
using UnityEditor;
using System;

namespace Bard.Configuration {
	[CreateAssetMenu(menuName="Bard/Configuration/New Characters Config", order=13)]
	public class CharacterConfig : ScriptableConfig {
		[SerializeField] private List<NPCDefinitionGroup> m_DefinitionGroups = new();
		public IReadOnlyList<NPCDefinitionGroup> DefinitionGroups => m_DefinitionGroups;
		public IReadOnlyList<NPCDefinition> Definitions => m_DefinitionGroups.SelectMany(dg => dg.Definitions).ToList();
		private string[] m_CachedNames;
		private int[] m_CachedIds;
		private Dictionary<int, int> m_IdToIndex;
		public string[] CharacterNames {
			get {
				if (m_CachedNames == null) RebuildCaches();
				return m_CachedNames;
			}
		}

		public override void RebuildCaches() {
			var defs = Definitions;
			m_CachedNames = defs.Select(d => d.EditorName).ToArray();
			m_CachedIds = defs.Select(d => d.Id).ToArray();
			m_IdToIndex = defs.Select((d, i) => (d.Id, i)).ToDictionary(x => x.Id, x => x.i);
		}

		public int GetIndexById(int id) {
			if (m_IdToIndex == null) RebuildCaches();
			return m_IdToIndex.TryGetValue(id, out var idx) ? idx : -1;
		}

		public int GetIdByIndex(int index) {
			if (m_IdToIndex == null) RebuildCaches();
			return index >= 0 && index < m_CachedIds.Length ? m_CachedIds[index] : -1;
		}

		public void VerifyIdIntegrity() {
			var isDirty = false;
			foreach (var group in m_DefinitionGroups) {
				isDirty |= group.UpdateDefinitionIds();
			}

			// Throw warnings for any NPCDefinitions that now have duplicate IDs
			var duplicates = Definitions.GroupBy(d => d.Id).Where(g => g.Count() > 1).ToList();
			foreach (var group in duplicates) {
				Debug.LogWarning($"Duplicate NPC Id {group.Key}: {string.Join(", ", group.Select(d => d.InternalName))}");
			}

			if (isDirty && duplicates.Count == 0) {
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			}
		}

		public void AddGroup(string name, int startIdx) {
			m_DefinitionGroups.Add(new(this, name, startIdx));
		}

		public void SwapGroups(int first, int second) {
			Debug.Assert(Mathf.Min(first, second) >= 0 && m_DefinitionGroups.Count > Mathf.Max(first, second));
			(m_DefinitionGroups[second], m_DefinitionGroups[first]) = (m_DefinitionGroups[first], m_DefinitionGroups[second]);
		}

#if UNITY_EDITOR
		public override bool Initialize() {
			if (m_Initialized) return false;

			EditorApplication.delayCall += () => {
				if (m_Initialized) return;

				AddGroup("Default", 0);
				m_DefinitionGroups[0].AddDefinition("Undefined");
				base.Initialize();

				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			};
			return false;
		}
#endif

		public NPCDefinition GetById(int id) {
			return m_DefinitionGroups
				.Find(dg => dg.StartIndex <= id && dg.StartIndex + dg.MaxId >= id)?
				.GetById(id);
		}
	}

	[Serializable]
	public class NPCDefinitionGroup {
		[SerializeField] private CharacterConfig TargetConfig;
		public string GroupName;
		public int StartIndex;
		[SerializeField] private List<NPCDefinition> m_Definitions = new();
		public IReadOnlyList<NPCDefinition> Definitions => m_Definitions;
		[SerializeField] private int m_MaxId = 0;
		public int MaxId => m_MaxId;

		public NPCDefinitionGroup(CharacterConfig config, string name, int idx) {
			TargetConfig = config;
			GroupName = name;
			StartIndex = idx;
		}

		public NPCDefinition GetById(int id) {
			return m_Definitions.Find(d => d.Id == id);
		}

		public bool UpdateDefinitionIds() {
			var isDirty = false;
			foreach (var d in m_Definitions) {
				isDirty |= d.UpdateIdOffset(StartIndex);
			}
			return isDirty;
		}

#if UNITY_EDITOR
		public void AddDefinition(string name) {
			var npcAsset = ScriptableObject.CreateInstance<NPCDefinition>();
			npcAsset.name = name;
			npcAsset.Initialize(m_MaxId, name, null, StartIndex);

			AssetDatabase.AddObjectToAsset(npcAsset, AssetDatabase.GetAssetPath(TargetConfig));

			m_Definitions.Add(npcAsset);

			EditorUtility.SetDirty(npcAsset);
			EditorUtility.SetDirty(TargetConfig);
			AssetDatabase.SaveAssets();

			m_MaxId++;
		}

		public void DeleteDefinitionAtIndex(int id) {
			var definition = m_Definitions[id];
			m_Definitions.RemoveAt(id);

			AssetDatabase.RemoveObjectFromAsset(definition);
			Undo.DestroyObjectImmediate(definition);

			EditorUtility.SetDirty(TargetConfig);
			AssetDatabase.SaveAssets();
		}

		public void ResetDefinitionCounters() {
			m_MaxId = 1;
			m_Definitions.ForEach(t => m_MaxId = Mathf.Max(m_MaxId, t.Id + 1));
			Debug.Log($"{GroupName} NPC group Id counters have been reset. Next definition Id: {m_MaxId}");
		}
#endif
	}
}
