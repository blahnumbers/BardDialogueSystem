using Bard.Configuration;
using UnityEditor;
using UnityEngine;

namespace Bard.DialogueSystem {
	// No support for create menu yet, it'll need a project-wide id tracking and config reference storage
	//[CreateAssetMenu(menuName="Bard/Definitions/New NPC Definition")]
	public class NPCDefinition : ScriptableObject {
		public int Id;
		[SerializeField] private string m_InternalName;
		public string InternalName;
#if UNITY_EDITOR
		[SerializeField] private string m_EditorName;
		public string EditorName;
#endif
		public string[] DisplayNames;
		public void Initialize(int id, string name, string displayName = null) {
			Id = id;
			m_InternalName = InternalName = name;
			m_EditorName = EditorName = displayName ?? name;
			DisplayNames = new[] { name };
		}

		private void OnValidate() {
			if (!string.IsNullOrEmpty(InternalName) && m_InternalName != InternalName) {
				name = m_InternalName = InternalName;
				EditorUtility.SetDirty(this);
				EditorApplication.delayCall += AssetDatabase.SaveAssets;
			}
			if (m_EditorName != EditorName) {
				EditorApplication.delayCall += () => {
					var assetPath = AssetDatabase.GetAssetPath(this);
					var configAsset = AssetDatabase.LoadAssetAtPath<CharacterConfig>(assetPath);
					if (configAsset != null) configAsset.RebuildCaches();
				};
			}
		}
	}
}
