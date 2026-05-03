using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using NUglify;
using NUglify.Html;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StaticHtmlGenerator;

internal static partial class Html {
	private const string DEFAULT_NAME = "index",
						 DEFAULT_EXT = ".html";

	private static readonly string[] booleanAttributes = new string[] {
		"allowfullscreen",
		"alpha",
		"async",
		"autofocus",
		"autoplay",
		"checked",
		"controls",
		"default",
		"defer",
		"disabled",
		"formnovalidate",
		"inert",
		"ismap",
		"itemscope",
		"loop",
		"multiple",
		"muted",
		"nomodule",
		"novalidate",
		"open",
		"playsinline",
		"readonly",
		"required",
		"reversed",
		"selected",
		"shadowrootclonable",
		"shadowrootcustomelementregistry",
		"shadowrootdelegatesfocus",
		"shadowrootserializable",
		"truespeed",
	};

	private static readonly Dictionary<string, List<string>> emptyAttributes = new() {
		{  "aria-current", new(){ "false" } },
		{  "class", new() { "" } },
		{  "style", new() { "" } },
	};

	private static readonly HtmlSettings minifySettings = new() {
		ShortBooleanAttribute = false
	};

	public static string Minify(string value) {
		var uglified = Uglify.Html(value, minifySettings);
		if (uglified.HasErrors) {
			throw new FormatException(string.Join("\n", uglified.Errors));
		}
		return uglified.Code;
	}

	public static string Minify(this IHtmlDocument document) {
		return Minify(document.ToHtml());
	}

	public static void Sanitize(this IHtmlDocument document) {
		RemoveEmptyLines(document);
		TrimScripts(document);
		RemoveEmptyAttributes(document);
		FixBooleanAttributes(document);
		FixUrls(document);
		FixExternalLinks(document);
	}

	[GeneratedRegex("""^\s*\n""", RegexOptions.Multiline)]
	private static partial Regex EmptyLineRegex();

	private static void FixBooleanAttributes(IHtmlDocument document) {
		foreach (var attribute in booleanAttributes) {
			foreach (var element in document.Body.QuerySelectorAll($"['{attribute}']")) {
				var value = element.GetAttribute(attribute)?.ToLower();
				if (value == "true") {
					element.SetAttribute(attribute, "");
				}
				else if (value == "false") {
					element.RemoveAttribute(attribute);
				}
			}
		}
	}

	private static void FixExternalLinks(IHtmlDocument document) {
		foreach (var e in document.Body.QuerySelectorAll("a[href]")) {
			var href = e.GetAttribute("href") ?? "";
			if (!(href.StartsWith("/") || href.StartsWith("#")) && !e.HasAttribute("rel")) {
				e.SetAttribute("rel", "external nofollow");
			}
		}
	}

	private static void FixUrls(IHtmlDocument document) {
		foreach (var e in document.QuerySelectorAll("[href]"))
			e.SetAttribute("href", ResolveUrl(e.GetAttribute("href")));
		foreach (var e in document.QuerySelectorAll("[src]"))
			e.SetAttribute("src", ResolveUrl(e.GetAttribute("src")));
	}

	private static void RemoveEmptyAttributes(IHtmlDocument document) {
		foreach (var (attribute, values) in emptyAttributes) {
			foreach (var element in document.Body.QuerySelectorAll($"['{attribute}']")) {
				var value = element.GetAttribute(attribute)?.Trim().ToLower();
				if (values.Contains(value)) {
					element.RemoveAttribute(attribute);
				}
			}
		}
	}

	private static string ResolveUrl(string url) {
		if (!url.StartsWith("/") && Uri.IsWellFormedUriString(url, UriKind.Relative))
			url = "/" + url;
		if (url.StartsWith("/")) {
			var parts = url.Split("#");
			var path = parts[0];
			var fragment = parts.Length > 1 ? parts[1] : "";
			if (path.EndsWith(DEFAULT_NAME + DEFAULT_EXT))
				url = path[..(path.Length - DEFAULT_NAME.Length - DEFAULT_EXT.Length)] + fragment;
			else if (path.EndsWith(DEFAULT_EXT))
				url = path[..(path.Length -DEFAULT_EXT.Length)] + fragment;
		}
		return url;
	}

	private static void RemoveEmptyLines(IHtmlDocument document) {
		document.Head.InnerHtml = EmptyLineRegex().Replace(document.Head.InnerHtml, "");
		document.Body.InnerHtml = EmptyLineRegex().Replace(document.Body.InnerHtml, "");
	}

	private static void TrimScripts(IHtmlDocument document) {
		foreach (var element in document.QuerySelectorAll("script")) {
			element.InnerHtml = element.InnerHtml.Trim();
		}
	}
}