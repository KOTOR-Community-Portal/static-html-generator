using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace StaticHtmlGenerator {
	public class Manifest : IManifest {
		private readonly ReadOnlyDictionary<string, IPage> _pages;

		public Manifest(IEnumerable<ManifestPage> pages) {
			ArgumentNullException.ThrowIfNull(pages, nameof(pages));
			var tempPages = new Dictionary<string, IPage>();
			foreach( var page in pages )
				tempPages[page.Path] = page;
			_pages = new(tempPages);
		}

		public IReadOnlyDictionary<string, IPage> Pages => _pages;

		public static Manifest Load(XDocument xml) {
			var pages = new List<ManifestPage>();
			var manifest = xml.Root!;
			var ns = manifest.GetDefaultNamespace();
			foreach( var pageElement in manifest.Element(ns + "pages")!.Elements(ns + "page") )
				LoadPageAndDescendants(pageElement, parent: null, pages);
			return new(pages);
		}

		private static ManifestPage LoadPageAndDescendants(XElement currentElement, string? parent, IList<ManifestPage> pages) {
			var ns = currentElement.GetDefaultNamespace();
			string path = currentElement.Attribute("path")!.Value;
			string title = currentElement.Element(ns + "title")!.Value;
			string template = currentElement.Element(ns + "template")!.Value;
			var tokens = new List<KeyValuePair<string, string>>();
			var children = new List<string>();
			foreach( var tokenElement in currentElement.Elements(ns + "token") )
				tokens.Add(new(tokenElement.Attribute("name")!.Value, tokenElement.Value));
			var subpages = currentElement.Element(ns + "subpages");
			if( subpages != null ) {
				foreach( var subpageElement in subpages.Elements(ns + "page") ) {
					var subpage = LoadPageAndDescendants(subpageElement, path, pages);
					children.Add(subpage.Path);
				}
			}
			var page = new ManifestPage(path, title, template, parent, children, tokens);
			pages.Add(page);
			return page;
		}
	}
}