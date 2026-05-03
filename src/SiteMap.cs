using System;
using System.Collections.Generic;

namespace StaticHtmlGenerator;

internal class SiteMap {
	internal class Entry {
		private readonly List<Entry> children = new();

		public string Path { get; set; }

		public List<Entry> Children => children;
	}

	private readonly Dictionary<string, Entry> entries = new();
	private readonly Entry root;

	public Entry Root => root;

	public Entry this[string path] => entries[path];

	public IReadOnlyDictionary<string, Entry> Entries => entries;

	public SiteMap() {
		root = new Entry();
	}

	public SiteMap(IEnumerable<(string path, string parent)> items) {
		ArgumentNullException.ThrowIfNull(items, nameof(items));
		foreach (var (path, _) in items) {
			if (path == null) {
				throw new ArgumentException("Path cannot be null.", nameof(items));
			}
			entries[path] = new Entry() { Path = path };
		}
		foreach (var (path, parent) in items) {
			var entry = entries[path];
			if (parent == null) {
				if (root != null) {
					throw new ArgumentException("Site map may contain only one root entry.", nameof(items));
				}
				root = entry;
			}
			else {
				var parentEntry = entries[parent];
				parentEntry.Children.Add(entry);
			}
		}
		if (root == null) {
			throw new ArgumentException("Site map must have a root entry.", nameof(items));
		}
	}
}