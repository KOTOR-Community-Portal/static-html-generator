using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StaticHtmlGenerator;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Microsoft.Design",
	"IDE1006",
	Justification = "JavaScript naming convention.")
]
internal partial class JsHtmlHelper {
	public static string fromMarkdown(string contents) {
		return Markdown.ToHtml(contents);
	}

	public static void minHeadings(IElement parentElement, int value) {
		if (value <= 1 || value > 6)
			return;
		var doc = parentElement.Owner;
		var srcs = parentElement.QuerySelectorAll("h1, h2, h3, h4, h5, h6");
		foreach( var src in srcs) {
			int level = int.Parse(src.TagName[1..]) - 1 + value;
			var dst = doc.CreateElement("h" + level);
			dst.TextContent = src.TextContent;
			foreach (var a in src.Attributes)
				dst.SetAttribute(a.Name, a.Value);
			foreach (var child in src.Children)
				dst.AppendChild(child);
			src.Parent.ReplaceChild(dst, src);
			src.Remove();
		}
	}

	[GeneratedRegex("""^([^A-Za-z]+.*)""")]
	private static partial Regex BeginningRegex();

	[GeneratedRegex("""-+""")]
	private static partial Regex MultipleDashRegex();

	[GeneratedRegex("""[\s\/\\]""")]
	private static partial Regex SeparatorRegex();

	[GeneratedRegex("""[^A-Za-z0-9-_]""")]
	private static partial Regex SpecialCharacterRegex();
}