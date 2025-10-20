using HtmlAgilityPack;
using StaticHtmlGenerator.Collections;
using StaticHtmlGenerator.Html;
using StaticHtmlGenerator.IO;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace StaticHtmlGenerator {
	public class HtmlGenerator {
		private const string INDEX = "index.html";
		private const string INSERT_SRC = "data-insert-src";
		private const string NAVIGATION = "data-navigation";
		private const string NAV_ITEM = "nav-item";
		private const string NAV_LINK = "nav-link";
		private const string NAV_SEP = "nav-sep";
		private const string TEMPLATE = "data-template";
		private const string TOC = "data-toc";

		private readonly HtmlGeneratorSettings _settings;

		public HtmlGenerator(HtmlGeneratorSettings settings) {
			ArgumentNullException.ThrowIfNull(settings, nameof(settings));
			_settings = settings;
		}

		public void Build() {
			Directories.CopyAll(new(_settings.GetPublicPath()), new(_settings.GetBuildPath()));
		}

		public void Clean() {
			var buildPath = _settings.GetBuildPath();
			if( !Directory.Exists(buildPath) )
				return;
			var buildDirectory = new DirectoryInfo(buildPath);
			foreach( var file in buildDirectory.EnumerateFiles() )
				file.Delete();
			foreach( var directory in buildDirectory.EnumerateDirectories() )
				directory.Delete(recursive: true);
		}

		public void Generate(HtmlDocument htmlDoc, IHtmlGeneratorContext context) {
			HandleTokens(htmlDoc, context);
			HandleInsertion(htmlDoc, context);
			HandleTableHeaders(htmlDoc);
			HandleNavigation(htmlDoc, context);
			HandleTableOfContents(htmlDoc);
			HandleSpoilers(htmlDoc);
			HandleBuildPaths(htmlDoc, context);
		}

		public HtmlDocument Open(string path) {
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			if( IO.Paths.IsRelativeToWorkingDirectory(path) )
				path =  Path.Combine(_settings.WorkingDirectory, path);
			return ReadHtmlDocument(path);
		}

		private static HtmlDocument CreateHtmlDocument() {
			return new HtmlDocument() {
				OptionWriteEmptyNodes = true,
			};
		}

		private static HtmlDocument CreateHtmlDocumentFromHtml(string html) {
			var htmlDoc = CreateHtmlDocument();
			htmlDoc.LoadHtml(html);
			return htmlDoc;
		}

		private static HtmlDocument CreateHtmlDocumentFromMarkdown(string md) {
			var htmlDoc = CreateHtmlDocument();
			htmlDoc.LoadMarkdown(md);
			return htmlDoc;
		}

		private static void DoGetTagBranches(HtmlNode currentNode, Branch<int> currentBranch, Dictionary<string, Branch<int>> tagBranches) {
			if( IsTemplateNode(currentNode) ) {
				var tagBranch = new Branch<int>(currentBranch);
				tagBranches[currentNode.Name] = tagBranch;
			}
			for( int i = 0; i < currentNode.ChildNodes.Count; ++i ) {
				var nextNode = currentNode.ChildNodes[i];
				var nextBranch = new Branch<int>(currentBranch.Concat(i));
				DoGetTagBranches(nextNode, nextBranch, tagBranches);
			}
		}

		private static HtmlNode FindChildByNodeType(HtmlNode node, HtmlNodeType nodeType) {
			for( int i = 0; i < node.ChildNodes.Count; ++i ) {
				var child = node.ChildNodes[i];
				if( child.NodeType == nodeType ) {
					return child;
				}
			}
			return null;
		}

		private static int GetHeadingLevel(string htmlName) {
			return htmlName.Length == 2
				&& htmlName[0] == 'h'
				&& int.TryParse(htmlName[1..2], out var level)
				&& level >= 1 && level <= 6
					? level
					: 0;
		}

		private static Dictionary<string, Branch<int>> GetTagBranches(HtmlNode node) {
			var tagBranches = new Dictionary<string, Branch<int>>();
			DoGetTagBranches(node, new(), tagBranches);
			return tagBranches;
		}

		private static void InsertContent(HtmlNode content, HtmlNode destination) {
			foreach( var child in content.ChildNodes ) {
				var clone = child.CloneNode(deep: true);
				destination.AppendChild(clone);
			}
		}

		private static void InsertContentWithTemplate(HtmlNode content, HtmlNode template, HtmlNode destination) {
			var templateBranches = GetTagBranches(template);
			var previousBranch = new Branch<int>();
			var destinationNodes = new Stack<HtmlNode>();
			destinationNodes.Push(destination);
			var previousDestination = destinationNodes.Peek();
			foreach( var currentNode in content.ChildNodes ) {
				Branch<int> templateBranch = default;
				bool hasTemplate = false;
				int anchorIndex = 0;
				var anchorNode = currentNode;
				while( anchorNode != null
					&& !(hasTemplate = templateBranches.TryGetValue(anchorNode.Name, out templateBranch)) ) {
					anchorNode = FindChildByNodeType(anchorNode, HtmlNodeType.Element);
					++anchorIndex;
				}
				if( hasTemplate ) {
					bool isSameLevel = templateBranch == previousBranch;
					bool hasSeparator = template.TrySelect(XPaths.WithClass($"{anchorNode.Name}-sep"), out var separator)
						&& previousBranch.Count > 0;
					var commonAncestors = previousBranch.GetCommonAncestors(templateBranch).ToList();
					int templateCount = commonAncestors.Count;
					var templateNode = template;
					if( isSameLevel ) {
						--templateCount;
						for( int i = 0; i < templateCount; ++i )
							templateNode = templateNode.ChildNodes[commonAncestors[i]];
						destinationNodes.Pop();
						if( anchorNode != currentNode ) {
							var parentNode = anchorNode.ParentNode;
							while( parentNode != currentNode ) {
								destinationNodes.Peek().AppendChild(parentNode.CloneNode(deep: false));
								parentNode = parentNode.ParentNode;
							}
							destinationNodes.Peek().AppendChild(currentNode.CloneNode(deep: false));
						}
						if( hasSeparator ) {
							var destinationAncestors = new Stack<HtmlNode>();
							while( destinationNodes.Count > 1 )
								destinationAncestors.Push(destinationNodes.Pop());
							destinationNodes.Peek().AppendChild(separator!.CloneNode(deep: true));
							while( destinationAncestors.Count > 0 ) {
								var clone = destinationAncestors.Pop().CloneNode(deep: false);
								destinationNodes.Peek().AppendChild(clone);
								destinationNodes.Push(clone);
							}
						}
					}
					else {
						for( int i = 0; i < templateCount; ++i )
							templateNode = templateNode.ChildNodes[commonAncestors[i]];
						while( destinationNodes.Count - 1 > commonAncestors.Count )
							destinationNodes.Pop();
						if( hasSeparator )
							destinationNodes.Peek().AppendChild(separator!.CloneNode(deep: true));
					}
					previousDestination = destinationNodes.Peek();
					for( int i = templateCount; i < templateBranch.Count; ++i ) {
						templateNode = templateNode.ChildNodes[templateBranch[i]];
						var currentDestination = templateNode.CloneNode(deep: false);
						if( i == anchorIndex )
							currentDestination.CloneAttributes(currentNode);
						previousDestination.AppendChild(currentDestination);
						destinationNodes.Push(currentDestination);
						previousDestination = currentDestination;
					}
					var childNodes = new List<HtmlNode>(anchorNode.ChildNodes);
					foreach( var child in childNodes )
						previousDestination.AppendChild(child);
					previousBranch = anchorNode == currentNode
						? templateBranch
						: new(templateBranch.Take(templateBranch.Count - anchorIndex - 1));
				}
				else {
					var clone = currentNode.CloneNode(deep: true);
					previousDestination.AppendChild(clone);
				}
			}
		}

		private static void InsertTableOfContents(HtmlNode content, HtmlNode destination) {
			ArgumentNullException.ThrowIfNull(content, nameof(content));
			ArgumentNullException.ThrowIfNull(destination, nameof(destination));
			var htmlDoc = destination.OwnerDocument;
			var listNodes = new Stack<HtmlNode>();
			listNodes.Push(destination);
			int previousLevel = 0;
			foreach( var node in content.Descendants() ) {
				int currentLevel = GetHeadingLevel(node.Name);
				if( currentLevel > 0 ) {
					var nodeText = HtmlEntity.DeEntitize(node.InnerHtml);
					string id = node.GetAttributeValue("id", "");
					if( id == "" ) {
						id = Text.CleanPath(WebUtility.HtmlDecode(node.InnerText));
						node.Attributes.Add("id", id);
					}
					if( currentLevel >= previousLevel ) {
						for( int i = previousLevel; i < currentLevel; ++i ) {
							var newList = htmlDoc.CreateElement("ul");
							listNodes.Peek().AppendChild(newList);
							listNodes.Push(newList);
						}
					}
					else {
						for( int i = previousLevel; i >= currentLevel; --i )
							listNodes.Pop();
						var newList = htmlDoc.CreateElement("ul");
						listNodes.Peek().AppendChild(newList);
						listNodes.Push(newList);
					}
					var list = listNodes.Peek();
					var item = htmlDoc.CreateElement("li");
					var link = htmlDoc.CreateElement("a");
					link.Attributes.Add("href", "#" + id);
					link.InnerHtml = nodeText;
					item.AppendChild(link);
					list.AppendChild(item);
					previousLevel = currentLevel;
				}
			}
		}

		private static bool IsTemplateNode(HtmlNode node) {
			return node.NodeType == HtmlNodeType.Element
				&& (
					   node.Name == "h1"
					|| node.Name == "h2"
					|| node.Name == "h3"
					|| node.Name == "h4"
					|| node.Name == "h5"
					|| node.Name == "h6"
					|| node.Name == "p"
					|| node.Name == "br"
					|| node.Name == "hr"
					|| node.Name == "ol"
					|| node.Name == "ul"
					|| node.Name == "dl"
					|| node.Name == "a"
					|| node.Name == "img"
					|| node.Name == "table"
				);
		}

		private static HtmlDocument ReadHtmlDocument(string path) {
			HtmlDocument htmlDoc;
			string ext = Path.GetExtension(path);
			ThrowIfFileNotFound(path);
			if( ext == ".html" ) {
				htmlDoc = CreateHtmlDocumentFromHtml(File.ReadAllText(path));
			}
			else if( ext == ".md" ) {
				htmlDoc = CreateHtmlDocumentFromMarkdown(File.ReadAllText(path));
			}
			else {
				ThrowFileFormatNotSupported(path);
				return null;
			}
			ThrowIfHasNoHead(htmlDoc, path);
			ThrowIfHasNoBody(htmlDoc, path);
			ThrowIfHasParseErrors(htmlDoc, path);
			return htmlDoc;
		}

		private static string ResolveResourcePath(string? path, string relativeDirectory, string? absoluteDirectory = null) {
			if( string.IsNullOrEmpty(path) ) {
				return relativeDirectory;
			}
			else if( path.StartsWith(".") ) {
				return Path.Combine(relativeDirectory, path);
			}
			else {
				return Path.Combine(
					absoluteDirectory ?? Directory.GetCurrentDirectory(),
					path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				);
			}
		}

		[DoesNotReturn]
		private static void ThrowFileFormatNotSupported(string path) {
			throw new FormatException($"Specified file format is not supported. (path: '{path}')");
		}

		private static void ThrowIfFileNotFound(string path) {
			if( !File.Exists(path) )
				throw new FileNotFoundException($"Specified file was not found. (path: '{path}')");
		}

		private static void ThrowIfHasNoBody(HtmlDocument htmlDoc, string path) {
			if( !htmlDoc.DocumentNode.TrySelect(XPaths.Body, out _) )
				throw new FormatException($"Specified HTML document does not contain a body. (path: '{path}')");
		}

		private static void ThrowIfHasNoHead(HtmlDocument htmlDoc, string path) {
			if( !htmlDoc.DocumentNode.TrySelect(XPaths.Head, out _) )
				throw new FormatException($"Specified HTML document does not contain a head. (path: '{path}')");
		}

		private static void ThrowIfHasParseErrors(HtmlDocument htmlDoc, string path) {
			if( htmlDoc.ParseErrors.Any() ) {
				var sb = new StringBuilder()
					.Append("Specified HtmlDocument contains parse errors (path: '")
					.Append(path)
					.AppendLine("')");
				foreach( var parseError in htmlDoc.ParseErrors )
					sb
						.Append('[').Append(parseError.Code).Append("] ")
						.Append(parseError.Reason)
						.Append(" (Line: ").Append(parseError.Line)
						.Append(" , Position: ").Append(parseError.LinePosition)
						.Append(')').AppendLine();
				throw new FormatException(sb.ToString());
			}
		}

		private (IList<IPage> Items, string? Target) FindNavigationPages(string target, IHtmlGeneratorContext context) {
			switch( target ) {
			case "self": return (context.SiblingsAndSelf, context.Page.Path);
			case "children": return (context.Children, null);
			case "ancestors": return (context.Ancestors, null);
			case "ancestorsAndSelf": return (context.Ancestors.Concat(context.Page).ToList(), context.Page.Path);
			default:
				if( context.Manifest.Pages.TryGetValue(target, out var page) )
					return (context.FromPage(page).SiblingsAndSelf, target);
				else
					throw new FormatException($"Specified navigation target is not valid. (value: '{target}')");
			}
		}

		private void HandleBuildPath(HtmlNode node, string attributeName, string relativeDirectory, string workingDirectory) {
			string path = node.Attributes[attributeName].Value;
			if( IO.Paths.IsRelativeToWorkingDirectory(path)
				&& !path.StartsWith("#")
				&& !path.StartsWith("http") ) {
				if( !path.StartsWith("/") )
					path = "/" + path;
			}
			else if( path.StartsWith(".") ) {
				path = ResolveResourcePath(path, relativeDirectory, workingDirectory);
				path = Path.GetRelativePath(_settings.GetPublicPath(), path);
				if( !path.StartsWith("/") )
					path = "/" + path;
			}
			int indexPos = path.IndexOf(INDEX);
			if( indexPos >= 0 )
				path = path[..indexPos] + path[(indexPos + INDEX.Length)..];
			node.SetAttributeValue(attributeName, path);
		}

		private void HandleBuildPaths(HtmlDocument htmlDoc, IHtmlGeneratorContext context) {
			string workingDirectory = _settings.WorkingDirectory;
			string relativeDirectory = Directory.GetParent(Path.Combine(workingDirectory,context.Page.Source))?.FullName ?? workingDirectory;
			foreach( var node in htmlDoc.DocumentNode.Descendants() ) {
				if( node.Attributes.Contains("href") )
					HandleBuildPath(node, "href", relativeDirectory, workingDirectory);
				if( node.Attributes.Contains("src") )
					HandleBuildPath(node, "src", relativeDirectory, workingDirectory);
			}
		}

		private void HandleInsertion(HtmlDocument htmlDoc, IHtmlGeneratorContext context) {
			var documentNode = htmlDoc.DocumentNode;
			foreach( var destination in documentNode.SelectAll(XPaths.WithAttribute(INSERT_SRC)) ) {
				string srcPath = ResolveResourcePath(destination.Attributes[INSERT_SRC].Value, "", _settings.WorkingDirectory);
				string templateAttribute = destination.GetAttributeValue(TEMPLATE, "");
				var sourceDoc = ReadHtmlDocument(srcPath);
				HandleTokens(sourceDoc, context);
				HandleInsertion(sourceDoc, context);
				var source = sourceDoc.DocumentNode.SelectSingleNode(XPaths.Body);
				if( templateAttribute == "" ) {
					InsertContent(source, destination);
				}
				else {
					string templatePath = ResolveResourcePath(templateAttribute, "", _settings.WorkingDirectory);
					var templateDoc = ReadHtmlDocument(templatePath);
					var template = templateDoc.DocumentNode.SelectSingleNode(XPaths.Body);
					InsertContentWithTemplate(source, template, destination);
				}
				destination.Attributes.Remove(INSERT_SRC);
				destination.Attributes.Remove(TEMPLATE);
			}
		}

		private void HandleNavigation(HtmlDocument htmlDoc, IHtmlGeneratorContext context) {
			var body = htmlDoc.DocumentNode.SelectSingleNode(XPaths.Body);
			foreach( var destination in body.SelectAll(XPaths.WithAttribute(NAVIGATION)) ) {
				string templatePath = context.Page.Source;
				if( destination.Attributes.Contains(TEMPLATE) ) {
					var templateAttribute = destination.Attributes[TEMPLATE].Value;
					templatePath = ResolveResourcePath(templateAttribute, "", _settings.WorkingDirectory);
					var templateDoc = ReadHtmlDocument(templatePath);
					var template = templateDoc.DocumentNode.SelectSingleNode(XPaths.Body);
					foreach( var child in template.ChildNodes )
						destination.AppendChild(child.CloneNode(deep: true));
					destination.Attributes.Remove(TEMPLATE);
				}
				if( destination.TrySelect(XPaths.WithClass(NAV_ITEM), out var navItem) ) {
					if( destination.TrySelect(XPaths.WithClass(NAV_SEP), out var separator) )
						separator.ParentNode.RemoveChild(separator);
					var parent = navItem.ParentNode;
					parent.RemoveChild(navItem);
					var (items, target) = FindNavigationPages(destination.Attributes[NAVIGATION].Value, context);
					HandleNavigation(items, navItem, parent, separator, target);
				}
				else {
					throw new FormatException($"Specified navigation menu does not contain a navitem. (path: '{templatePath}')");
				}
				destination.Attributes.Remove(NAVIGATION);
			}
		}

		private void HandleNavigation(IPage source, HtmlNode template, HtmlNode destination, string? target = null) {
			var node = template.CloneNode(deep: true);
			var sb = new StringBuilder();
			foreach( var attribute in node.Attributes ) {
				sb.Append(attribute.Value);
				ReplaceTokens(sb, source);
				attribute.Value = sb.ToString();
				sb.Clear();
			}
			sb.Append(node.InnerHtml);
			ReplaceTokens(sb, source);
			node.InnerHtml = sb.ToString();
			if( node.TrySelect(XPaths.WithClass(NAV_LINK), out var link) ) {
				if( link.GetAttributeValue("href", "") == target )
					link.SetAttributeValue("aria-current", "page");
				if( link.HasClass("disabled") )
					link.SetAttributeValue("aria-disabled", "true");
				link.RemoveClass("_DISABLED_");
			}
			destination.AppendChild(node);
		}

		private void HandleNavigation(IList<IPage> pages, HtmlNode template, HtmlNode destination, HtmlNode? separator, string? target = null) {
			if( separator == null ) {
				for( int i = 0; i < pages.Count; ++i )
					HandleNavigation(pages[i], template, destination, target);
			}
			else {
				int i = 0;
				for( ; i < pages.Count - 1; ++i ) {
					HandleNavigation(pages[i], template, destination, target);
					destination.AppendChild(separator.CloneNode(deep: true));
				}
				HandleNavigation(pages[i], template, destination, target);
			}
		}

		private void HandleSpoilers(HtmlDocument htmlDoc) {
			var body = htmlDoc.DocumentNode.SelectSingleNode(XPaths.Body);
			foreach( var node in body.SelectAll(XPaths.WithClass("spoiler")) ) {
				var span = htmlDoc.CreateElement("span");
				var children = node.ChildNodes.ToList();
				foreach( var child in children )
					span.AppendChild(child);
				node.RemoveAllChildren();
				node.AppendChild(span);

				span.SetAttributeValue("aria-live", "assertive");
				span.SetAttributeValue("aria-hidden", "true");

				node.SetAttributeValue("role", "button");
				node.SetAttributeValue("title", "Show spoiler");
				node.SetAttributeValue("onclick", "spoiler(event)");
				node.SetAttributeValue("onkeydown", "spoiler(event)");
				node.SetAttributeValue("tabindex", "0");
			}
		}

		private void HandleTableHeaders(HtmlDocument htmlDoc) {
			var body = htmlDoc.DocumentNode.SelectSingleNode(XPaths.Body);
			var theads = body.SelectAll(XPaths.WithTag("thead")).ToList();
			foreach( var thead in theads )
				if( IsTableHeadEmpty(thead) )
					thead.Remove();
		}

		private void HandleTableOfContents(HtmlDocument htmlDoc) {
			var body = htmlDoc.DocumentNode.SelectSingleNode(XPaths.Body);
			foreach( var destination in body.SelectAll(XPaths.WithAttribute(TOC)) ) {
				var target = destination.GetAttributeValue(TOC, "");
				var content = body.SelectSingleNode(XPaths.WithAttribute("id", target));
				InsertTableOfContents(content, destination);
			}
		}

		private void HandleTokens(HtmlDocument htmlDoc, IHtmlGeneratorContext context) {
			var documentNode = htmlDoc.DocumentNode;
			var sb = new StringBuilder(documentNode.InnerHtml);
			ReplaceTokens(sb, context.Page, "PAGE");
			if( context.HasParent )
				ReplaceTokens(sb, context.Parent, "PARENT");
			documentNode.InnerHtml = sb.ToString();
		}

		private static bool IsTableHeadEmpty(HtmlNode thead) {
			var trs = thead.SelectAll(XPaths.WithTag("tr")).ToList();
			bool isHeadEmpty = true;
			for( int i = 0; isHeadEmpty && i < trs.Count; ++i ) {
				var tr = trs[i];
				bool hasTd = tr.SelectAll(XPaths.WithTag("td")).Count > 0;
				if( hasTd ) {
					isHeadEmpty = false;
				}
				else {
					bool isRowEmpty = true;
					var ths = tr.SelectAll(XPaths.WithTag("th"));
					for( int j = 0; isRowEmpty && j < ths.Count; ++j ) {
						var th = ths[j];
						isRowEmpty = th.InnerHtml == "";
					}
					isHeadEmpty = isRowEmpty;
				}
			}
			return isHeadEmpty;
		}

		private void ReplaceTokens(StringBuilder stringBuilder, IPage page) {
			foreach( var (find, replace) in page.Tokens )
				stringBuilder.Replace($"_{find.ToUpper()}_", replace);
			stringBuilder
				.Replace($"_PATH_", page.Path)
				.Replace($"_TITLE_", page.Title);
		}

		private void ReplaceTokens(StringBuilder stringBuilder, IPage page, string prefix) {
			foreach( var (find, replace) in page.Tokens )
				stringBuilder.Replace($"_{prefix}_{find.ToUpper()}_", replace);
			stringBuilder
				.Replace($"_{prefix}_PATH_", page.Path)
				.Replace($"_{prefix}_TITLE_", page.Title);
		}
	}
}