using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Bard.Editor {
	public static class DialogueActionDrawerRegistry {
		private static readonly Dictionary<Type, DialogueActionDrawer> s_Drawers;

		static DialogueActionDrawerRegistry() {
			s_Drawers = new Dictionary<Type, DialogueActionDrawer>();
			foreach (var type in TypeCache.GetTypesWithAttribute<DialogueActionDrawerAttribute>()) {
				var attr = type.GetCustomAttribute<DialogueActionDrawerAttribute>();
				s_Drawers[attr.ActionType] = (DialogueActionDrawer)Activator.CreateInstance(type);
			}
		}

		public static DialogueActionDrawer GetDrawer(Type actionType) {
			return s_Drawers.TryGetValue(actionType, out var drawer) ? drawer : null;
		}
	}
}
