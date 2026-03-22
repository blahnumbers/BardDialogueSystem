using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Bard.DialogueSystem {
	public interface ILocalizationProvider {
		public bool IsReady { get; }
		public bool TryGetString(string id, out string message);
		public UniTask<IReadOnlyCollection<LocalizationString>> GetLocalizationAsync(string path, CancellationToken token = default);
	}

	public class LocalizationString {
		public string Id;
		public string String;
	}
}
