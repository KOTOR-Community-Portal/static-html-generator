using System.Collections.Generic;

namespace StaticHtmlGenerator;

internal class PageContext {
	private readonly Dictionary<string, string> tokens = new();

	public string Path { get; set; }

	public string Title { get; set; }

	public string Template { get; set; }

	public string Parent { get; set; }

	public Dictionary<string, string> Tokens => tokens;
}