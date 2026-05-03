using Markdig;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace StaticHtmlGenerator;

internal static partial class Markdown {
	public static string ToHtml(string md) {
		md = new StringBuilder(md)
			.ReplaceLineEndings()
			.ReplaceSpoilers()
			.ToString()
			.ReplaceLineBreaks()
			.ReplaceEmptyLines();
		var pipeline = new MarkdownPipelineBuilder()
			.UseCustomContainers()
			.UseDefinitionLists()
			.UseEmphasisExtras()
			.UseGenericAttributes()
			.UsePipeTables()
			.Build();
		return Markdig.Markdown.ToHtml(md, pipeline);
	}

	[GeneratedRegex("""^[ ]{0,3}([-_*])[ \t]*\1[ \t]*\\[ \t]*$""", RegexOptions.Multiline)]
	private static partial Regex EmptyLineRegex();

	[GeneratedRegex("""^[ ]{0,3}([-_*])[ \t]*\1[ \t]*$""", RegexOptions.Multiline)]
	private static partial Regex LineBreakRegex();

	private static string ReplaceEmptyLines(this string text) {
		// It seems that an extra \n needs to be inserted to prevent Markdig from eating the next line.
		return EmptyLineRegex().Replace(text, "<p></p>\n");
	}

	private static string ReplaceLineBreaks(this string text) {
		return LineBreakRegex().Replace(text, "<br />");
	}

	private static StringBuilder ReplaceLineEndings(this StringBuilder sb) {
		return sb
			.Replace("\r\n", "\n")
			.Replace("\r", "\n");
	}

	private static StringBuilder ReplaceSpoilers(this StringBuilder sb) {
		return sb
			.Replace(">!", "::")
			.Replace("!<", "::{.spoiler}");
	}
}