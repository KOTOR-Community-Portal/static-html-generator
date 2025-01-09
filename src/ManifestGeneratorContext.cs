using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StaticHtmlGenerator {
	public class ManifestGeneratorContext : IHtmlGeneratorContext {
		private readonly IManifest _manifest;
		private readonly IPage _page;
		private readonly ReadOnlyCollection<IPage> _ancestors;
		private readonly ReadOnlyCollection<IPage> _siblingsAndSelf;
		private readonly ReadOnlyCollection<IPage> _children;

		public ManifestGeneratorContext(IManifest manifest, IPage page) {
			ArgumentNullException.ThrowIfNull(manifest, nameof(manifest));
			ArgumentNullException.ThrowIfNull(page, nameof(page));
			_manifest = manifest;
			_page = page;
			var ancestors = new List<IPage>();
			var currentPage = page;
			while( currentPage.HasParent ) {
				var currentParent = manifest.Pages[currentPage.Parent!];
				ancestors.Add(currentParent);
				currentPage = currentParent;
			}
			ancestors.Reverse();
			var children = new List<IPage>();
			_ancestors = new(ancestors);
			List<IPage> siblingsAndSelf;
			if( page.HasParent ) {
				var parentChildren = manifest.Pages[page.Parent!].Children;
				siblingsAndSelf = new(parentChildren.Count);
				foreach( var child in parentChildren )
					siblingsAndSelf.Add(manifest.Pages[child]);
			}
			else {
				siblingsAndSelf = new() { page };
			}
			_siblingsAndSelf = new(siblingsAndSelf);
			foreach( var currentChild in page.Children )
				children.Add(manifest.Pages[currentChild]);
			_children = new(children);
		}

		public IManifest Manifest => _manifest;
		IManifest IHtmlGeneratorContext.Manifest => _manifest;
		IPage IHtmlGeneratorContext.Page => _page;
		public ReadOnlyCollection<IPage> Ancestors => _ancestors;
		IList<IPage> IHtmlGeneratorContext.Ancestors => _ancestors;
		public ReadOnlyCollection<IPage> SiblingsAndSelf => _siblingsAndSelf;
		IList<IPage> IHtmlGeneratorContext.SiblingsAndSelf => _siblingsAndSelf;
		public ReadOnlyCollection<IPage> Children => _children;
		IList<IPage> IHtmlGeneratorContext.Children => _children;

		public IHtmlGeneratorContext FromPage(IPage page) {
			return new ManifestGeneratorContext(_manifest, page);
		}
	}
}