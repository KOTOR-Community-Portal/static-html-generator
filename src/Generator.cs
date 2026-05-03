using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using JavaScriptEngineSwitcher.Core;
using System;
using System.IO;

namespace StaticHtmlGenerator;

internal class Generator {
	private readonly IJsEngine engine;
	private SiteContext site = null;
	private readonly Func<IHtmlDocument, string> toString;

	public Generator(IJsEngine engine, MarkupFormat format = MarkupFormat.None) {
		ArgumentNullException.ThrowIfNull(engine, nameof(engine));
		this.engine = engine;
		toString = format switch {
			MarkupFormat.Minified => Html.Minify,
			MarkupFormat.Pretty => FormatExtensions.Prettify,
			_ => (doc) => doc.ToHtml()
		};
	}

	public string Generate(PageContext page) {
		ArgumentNullException.ThrowIfNull(page, nameof(page));
		if (site == null) {
			throw new InvalidOperationException("The site must be loaded before a page can be generated");
		}
		var doc = CreateDocument();
		engine.EmbedHostObject("__doc", doc);
		engine.CallFunction("__loadPage", JavaScript.ToJson(page));
		doc.DocumentElement.SetAttribute("lang", "en");
		doc.DocumentElement.InnerHtml = engine.CallFunction<string>(page.Template);
		doc.Sanitize();
		return toString(doc);
	}

	public void LoadComponent(string path) {
		ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
		var code = engine.CallFunction<string>("__parseComponents", File.ReadAllText(path));
		engine.Execute(code);
	}

	public void LoadSite(SiteContext site) {
		ArgumentNullException.ThrowIfNull(site, nameof(site));
		this.site = site;
		engine.CallFunction("__loadSite", JavaScript.ToJson(site));
	}

	public void LoadTemplate(string path) {
		ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
		var code = engine.CallFunction<string>("__parseTemplates", File.ReadAllText(path));
		engine.Execute(code);
	}

	private static IHtmlDocument CreateDocument() {
		var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = true });
		return parser.ParseDocument("<!DOCTYPE html></html>");
	}
}