using UnityEngine;

public abstract class ScriptableConfig : ScriptableObject {
	public virtual void Initialize(string path) { }
	public virtual void RebuildCaches() { }
	private void OnValidate() => RebuildCaches();
}
