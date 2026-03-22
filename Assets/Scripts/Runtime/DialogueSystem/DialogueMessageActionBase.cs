namespace Bard.DialogueSystem {
	public class DialogueMessageActionBase {
		public int Type = 0;
		public int CValue = 0;
		public int IValue = 0;
		public string SValue = string.Empty;
		public DialogueMessageSkillCheck SkillCheck = new() { Complexity = 10 };
	}
}
