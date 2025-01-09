using HtmlAgilityPack;
using Markdig;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace StaticHtmlGenerator.Html {
	public static class Extensions {
		public static void LoadMarkdown(this HtmlDocument htmlDoc, string md) {
			md = new StringBuilder(md)
				.ReplaceLineEndings()
				.ReplaceSpoilers()
				.ToString()
				.ReplaceEmptyLines()
				.ReplaceLineBreaks();
			var pipeline = new MarkdownPipelineBuilder()
				.UseCustomContainers()
				.UseEmphasisExtras()
				.UseGenericAttributes()
				.UsePipeTables()
				.Build();
			var html = Markdown.ToHtml(md, pipeline);
			htmlDoc.LoadHtml(html);

			var htmlNode = HtmlNode.CreateNode("<html></html>");
			var headNode = HtmlNode.CreateNode("<head></head>");
			var bodyNode = HtmlNode.CreateNode("<body></body>");
			htmlNode.AppendChild(headNode);
			htmlNode.AppendChild(bodyNode);
			foreach( var child in htmlDoc.DocumentNode.SelectNodes("/*") )
				bodyNode.AppendChild(child);
			htmlDoc.DocumentNode.RemoveAllChildren();
			htmlDoc.DocumentNode.AppendChild(htmlNode);
		}

		public static void CloneAttributes(this HtmlNode htmlNode, HtmlNode other) {
			foreach( var attribute in other.Attributes ) {
				if( attribute.Name == "class" ) {
					foreach( var className in attribute.Value.Split(' ') )
						if( !htmlNode.HasClass(className) )
							htmlNode.AddClass(className);
				}
				else {
					if( !htmlNode.Attributes.Contains(attribute.Name) )
						htmlNode.SetAttributeValue(attribute.Name, attribute.Value);
				}
			}
		}

		public static HtmlNode SelectHead(this HtmlDocument htmlDoc) {
			return htmlDoc.DocumentNode.SelectSingleNode(XPaths.Head);
		}

		public static HtmlNode? SelectBody(this HtmlDocument htmlDoc) {
			return htmlDoc.DocumentNode.SelectSingleNode(XPaths.Body);
		}

		public static HtmlNodeCollection SelectAll(this HtmlNode htmlNode, string xPath) {
			return htmlNode.SelectNodes(xPath) ?? new(htmlNode);
		}

		public static bool TrySelect(this HtmlNode htmlNode, string xPath, [MaybeNullWhen(false)] out HtmlNode result) {
			result = htmlNode.SelectSingleNode(xPath);
			return result != null;
		}

		private static string ReplaceEmptyLines(this string text) {
			// It seems that an extra \n needs to be inserted to prevent Markdig from eating the next line.
			return Regex.Replace(text, @"^[ ]{0,3}([-_*])[ \t]*\1[ \t]*$", "<p></p>\n", RegexOptions.Multiline);
		}

		private static string ReplaceLineBreaks(this string text) {
			return Regex.Replace(text, @"^[ ]{0,3}([-_*])[ \t]*\1[ \t]*\\[ \t]*$", "<br />", RegexOptions.Multiline);
		}

		private static StringBuilder ReplaceLineEndings(this StringBuilder sb) {
			return sb.Replace("\r\n", "\n").Replace("\r", "\n");
		}

		private static StringBuilder ReplaceSpoilers(this StringBuilder sb) {
			return sb
				.Replace(">!", "::")
				.Replace("!<", "::{.spoiler}");
		}
	}
}