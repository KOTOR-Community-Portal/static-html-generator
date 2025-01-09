using System;
using System.Collections.Generic;

namespace StaticHtmlGenerator {
	public interface IHtmlGeneratorContext {
		IManifest Manifest { get; }
		IPage Page { get; }
		IList<IPage> Ancestors { get; }
		bool HasParent => Ancestors.Count > 0;
		IPage Parent => HasParent
			? Ancestors[^1]
			: throw new InvalidOperationException("There is no parent page in the specified context.");
		IList<IPage> SiblingsAndSelf { get; }
		bool HasChildren => Children.Count > 0;
		IList<IPage> Children { get; }
		IHtmlGeneratorContext FromPage(IPage page);
	}
}