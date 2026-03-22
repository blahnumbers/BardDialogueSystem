using System;

namespace Bard.DialogueSystem {
	[Serializable]
	public class DialogueMessageType {
		public static int EXIT => 1000;
		public int Id;
		public string Name;

		public bool IsDefault() {
			return Id == 0 && Name == "Default";
		}

		public DialogueMessageType(int id, string name = "") {
			Id = id;
			Name = name;
		}
	}
}
