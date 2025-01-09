using System.Collections.Generic;

namespace StaticHtmlGenerator {
	public interface IManifest {
		IReadOnlyDictionary<string, IPage> Pages { get; }
	}
}