using System.Collections;
using System.Collections.Generic;

namespace StaticHtmlGenerator {
	public interface IPage {
		string Path { get; }
		string Title { get; }
		string Source { get; }
		string? Parent { get; }
		bool HasParent => Parent != null;
		IList<string> Children { get; }
		bool HasChildren => Children.Count > 0;
		IDictionary<string, string> Tokens { get; }
	}
}