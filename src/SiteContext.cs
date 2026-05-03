using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StaticHtmlGenerator;

internal class SiteContext {
	private readonly Dictionary<string, PageContext> pages = new();
	private readonly SiteMap siteMap = new();

	public IReadOnlyDictionary<string, PageContext> Pages => pages;

	public SiteMap SiteMap => siteMap;

	private SiteContext(IList<PageContext> items) {
		var siteMapItems = new List<(string, string)>();
		foreach (var page in items) {
			string path = page.Path;
			string parent = page.Parent;
			pages[path] = page;
			siteMapItems.Add((path, parent));
		}
		siteMap = new(siteMapItems);
	}

	public static SiteContext Create(IList<PageContext> pages) {
		ArgumentNullException.ThrowIfNull(pages, nameof(pages));
		return new(pages);
	}

	public static SiteContext Create(XDocument xml) {
		var manifest = xml.Root;
		var ns = manifest.GetDefaultNamespace();
		var pages = new List<PageContext>();
		void AddPageAndDescendants(XElement currentElement, string parent) {
			string path = currentElement.Attribute("path").Value;
			string title = currentElement.Element(ns + "title").Value;
			string template = currentElement.Element(ns + "template").Value;
			var page = new PageContext() {
				Path = path,
				Title = title,
				Parent = parent,
				Template = template
			};
			var tokens = page.Tokens;
			foreach (var tokenElement in currentElement.Elements(ns + "token")) {
				var name = tokenElement.Attribute("name").Value;
				tokens[name] = tokenElement.Value;
			}
			pages.Add(page);
			var subpages = currentElement.Element(ns + "subpages");
			if (subpages != null) {
				foreach (var subpageElement in subpages.Elements(ns + "page")) {
					AddPageAndDescendants(subpageElement, page.Path);
				}
			}
		}
		foreach (var pageElement in manifest.Element(ns + "pages").Elements(ns + "page")) {
			AddPageAndDescendants(pageElement, parent: null);
		}
		return new SiteContext(pages);
	}
}