using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StaticHtmlGenerator {
	public class ManifestPage : IPage {
		private readonly string _path;
		private readonly string _title;
		private readonly string _template;
		private readonly string? _parent;
		private readonly ReadOnlyCollection<string> _children;
		private readonly ReadOnlyDictionary<string, string> _tokens;

		public ManifestPage(string path, string title, string template, string? parent, IEnumerable<string> children, IEnumerable<KeyValuePair<string, string>> tokens) {
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			ArgumentNullException.ThrowIfNull(template, nameof(template));
			ArgumentNullException.ThrowIfNull(children, nameof(children));
			_path = path;
			_title = title;
			_template = template;
			_parent = parent;
			_children = new List<string>(children).AsReadOnly();
			_tokens = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(tokens));
		}

		public string Path => _path;
		public string Title => _title;
		public string Source => _template;
		IDictionary<string, string> IPage.Tokens => _tokens;
		public ReadOnlyDictionary<string, string> Tokens => _tokens;
		public string? Parent => _parent;
		public bool HasParent => _parent != null;
		public ReadOnlyCollection<string> Children => _children;
		IList<string> IPage.Children => _children;
		public bool HasChildren => _children.Count > 0;
	}
}