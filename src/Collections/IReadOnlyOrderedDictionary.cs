using System.Collections.Generic;

namespace StaticHtmlGenerator.Collections {
	public interface IReadOnlyOrderedDictionary<K, V> :
		IReadOnlyList<KeyValuePair<K, V>>,
		IReadOnlyDictionary<K, V>
		where K : notnull {
		new int Count { get; }
		new IReadOnlyList<K> Keys { get; }
		new IReadOnlyList<V> Values { get; }
		KeyValuePair<K, V> GetItem(int index);
	}
}