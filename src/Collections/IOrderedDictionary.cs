using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StaticHtmlGenerator.Collections {
	public interface IOrderedDictionary<K, V> :
		IReadOnlyOrderedDictionary<K, V>,
		IList<KeyValuePair<K, V>>,
		IDictionary<K, V>
		where K : notnull {
		new int Count { get; }
		new IList<K> Keys { get; }
		new IList<V> Values { get; }
		new V this[K key] { get; set; }
		new bool ContainsKey(K key);
		void OrderBy(IEnumerable<K> keys);
		void SetItem(int index, KeyValuePair<K, V> item);
		new bool TryGetValue(K key, [MaybeNullWhen(false)] out V value);
	}
}