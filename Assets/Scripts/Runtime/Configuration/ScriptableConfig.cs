using UnityEngine;

namespace Bard.Configuration {
	public abstract class ScriptableConfig : ScriptableObject {
		protected bool m_Initialized = false;
#if UNITY_EDITOR
		public virtual bool Initialize() {
			if (m_Initialized) return false;

			m_Initialized = true;
			return true;
		}
		private void OnEnable() => Initialize();
#endif

		public virtual void RebuildCaches() { }
		private void OnValidate() => RebuildCaches();
	}
}
