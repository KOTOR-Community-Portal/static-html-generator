using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StaticHtmlGenerator.Collections {
	public class ReadOnlyOrderedDictionary<K, V> :
		ReadOnlyDictionary<K, V>,
		IOrderedDictionary<K, V>,
		IReadOnlyOrderedDictionary<K, V>
		where K : notnull {
		private readonly IOrderedDictionary<K, V> _dictionary;
		private readonly OrderedDictionary<K, V>.KeyList _keys;
		private readonly OrderedDictionary<K, V>.ValueList _values;

		public ReadOnlyOrderedDictionary(OrderedDictionary<K, V> dictionary) :
			this(dictionary, dictionary.Comparer) { }

		public ReadOnlyOrderedDictionary(IOrderedDictionary<K, V> dictionary, IEqualityComparer<K>? comparer = null) :
			base(dictionary) {
			_dictionary = dictionary;
			_keys = new(dictionary, comparer);
			_values = new(dictionary);
		}

		new public OrderedDictionary<K, V>.KeyList Keys => _keys;
		IList<K> IOrderedDictionary<K, V>.Keys => _keys;
		IReadOnlyList<K> IReadOnlyOrderedDictionary<K, V>.Keys => _keys;

		new public OrderedDictionary<K, V>.ValueList Values => _values;
		IList<V> IOrderedDictionary<K, V>.Values => _values;
		IReadOnlyList<V> IReadOnlyOrderedDictionary<K, V>.Values => _values;


		KeyValuePair<K, V> IList<KeyValuePair<K, V>>.this[int index] {
			get => _dictionary.GetItem(index);
			set => Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		V IOrderedDictionary<K, V>.this[K key] {
			get => _dictionary[key];
			set => Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		public KeyValuePair<K, V> this[int index] => _dictionary.GetItem(index);

		public KeyValuePair<K, V> GetItem(int index) {
			return _dictionary.GetItem(index);
		}

		public int IndexOf(KeyValuePair<K, V> item) {
			return _dictionary.IndexOf(item);
		}

		void IList<KeyValuePair<K, V>>.Insert(int index, KeyValuePair<K, V> item) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		void IOrderedDictionary<K, V>.OrderBy(IEnumerable<K> keys) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		void IList<KeyValuePair<K, V>>.RemoveAt(int index) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		void IOrderedDictionary<K, V>.SetItem(int index, KeyValuePair<K, V> item) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}
	}
}