using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace StaticHtmlGenerator.Collections {
	public class OrderedDictionary<K, V> :
		IOrderedDictionary<K, V>,
		IOrderedDictionary
		where K : notnull {
		private readonly Dictionary<K, V> _dictionary;
		private readonly List<KeyValuePair<K, V>> _list;
		private readonly KeyList _keys;
		private readonly ValueList _values;

		public OrderedDictionary() {
			_dictionary = new();
			_list = new();
			_keys = new KeyList(this);
			_values = new ValueList(this);
		}

		public OrderedDictionary(IDictionary<K, V> dictionary) :
			this(dictionary, null) { }

		public OrderedDictionary(IDictionary<K, V> dictionary, IEqualityComparer<K>? comparer) {
			ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
			_dictionary = new(dictionary.Count, comparer);
			_list = new(dictionary.Count);
			_keys = new KeyList(this);
			_values = new ValueList(this);
			DoAddRange(dictionary);
		}

		public OrderedDictionary(IEnumerable<KeyValuePair<K, V>> collection) :
			this(collection, null) { }

		public OrderedDictionary(IEnumerable<KeyValuePair<K, V>> collection, IEqualityComparer<K>? comparer) {
			ArgumentNullException.ThrowIfNull(collection, nameof(collection));
			int capacity = collection.EstimateCapacity();
			_dictionary = new(capacity, comparer);
			_list = new(capacity);
			_keys = new KeyList(this);
			_values = new ValueList(this);
			DoAddRange(collection);
		}

		public OrderedDictionary(IEqualityComparer<K>? comparer) :
			this(0, comparer) { }

		public OrderedDictionary(int capacity) :
			this(capacity, null) { }

		public OrderedDictionary(int capacity, IEqualityComparer<K>? comparer) {
			_dictionary = new(capacity, comparer);
			_list = new(capacity);
			_keys = new KeyList(this);
			_values = new ValueList(this);
		}

		public IEqualityComparer<K> Comparer => _dictionary.Comparer;

		public int Count => _dictionary.Count;

		public KeyList Keys => _keys;
		IList<K> IOrderedDictionary<K, V>.Keys => _keys;
		IReadOnlyList<K> IReadOnlyOrderedDictionary<K, V>.Keys => _keys;
		ICollection<K> IDictionary<K, V>.Keys => _keys;
		IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => _keys;
		ICollection IDictionary.Keys => _keys;

		public ValueList Values => _values;
		IList<V> IOrderedDictionary<K, V>.Values => _values;
		IReadOnlyList<V> IReadOnlyOrderedDictionary<K, V>.Values => _values;
		ICollection<V> IDictionary<K, V>.Values => _values;
		IEnumerable<V> IReadOnlyDictionary<K, V>.Values => _values;
		ICollection IDictionary.Values => _values;

		bool IDictionary.IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;

		bool IDictionary.IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;
		bool ICollection<KeyValuePair<K, V>>.IsReadOnly => ((ICollection<KeyValuePair<K, V>>)_dictionary).IsReadOnly;

		bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		public V this[K key] {
			get => GetValue(key);
			set => SetValue(key, value);
		}

		object? IDictionary.this[object key] {
			get => GetValue(key);
			set => SetValue(key, value);
		}

		object? IOrderedDictionary.this[int index] {
			get => GetValueAt(index);
			set => SetValueAt(index, value);
		}

		KeyValuePair<K, V> IList<KeyValuePair<K, V>>.this[int index] {
			get => GetItem(index);
			set => SetItem(index, value);
		}

		KeyValuePair<K, V> IReadOnlyList<KeyValuePair<K, V>>.this[int index] => _list[index];

		public void Add(K key, V value) {
			ThrowIfKeyIsNull(key, nameof(key));
			ThrowIfKeyIsDuplicate(key, _dictionary.ContainsKey, nameof(key));
			DoAdd(key, value);
		}

		public void Add(KeyValuePair<K, V> item) {
			ThrowIfKeyIsNull(item.Key, nameof(item));
			ThrowIfKeyIsDuplicate(item.Key, _dictionary.ContainsKey, nameof(item));
			DoAdd(item.Key, item.Value);
		}

		void IDictionary.Add(object key, object? value) {
			K k = Exceptions.Argument.ThrowIfNotOfType<K>(key, nameof(key));
			ThrowIfKeyIsNull(k, nameof(key));
			ThrowIfKeyIsDuplicate(k, _dictionary.ContainsKey, nameof(key));
			V? v = Exceptions.Argument.ThrowIfNotOfType<V>(value, nameof(value));
			DoAdd(k, v);
		}

		public void AddRange(IEnumerable<KeyValuePair<K, V>> collection) {
			ArgumentNullException.ThrowIfNull(collection, nameof(collection));
			var list = collection.EnumerateMoreThanOnce();
			ThrowIfAnyKeyIsInvalid(list, _dictionary.Keys.ToHashSet(), nameof(collection));
			DoAddRange(list);
		}

		public ReadOnlyOrderedDictionary<K, V> AsReadOnly() {
			return new(this);
		}

		public void Clear() {
			_dictionary.Clear();
			_list.Clear();
		}

		public bool Contains(KeyValuePair<K, V> item) {
			ThrowIfKeyIsNull(item.Key, nameof(item));
			return _dictionary.Contains(item);
		}

		bool IDictionary.Contains(object key) {
			K k = Exceptions.Argument.ThrowIfNotOfType<K>(key, nameof(key));
			ThrowIfKeyIsNull(k);
			return _dictionary.ContainsKey(k);
		}

		public bool ContainsKey(K key) {
			ThrowIfKeyIsNull(key, nameof(key));
			return _dictionary.ContainsKey(key);
		}

		public bool ContainsValue(V value) {
			return _dictionary.ContainsValue(value);
		}

		public void CopyTo(Array array, int index) {
			((ICollection)_dictionary).CopyTo(array, index);
		}

		void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
			((ICollection<KeyValuePair<K, V>>)_dictionary).CopyTo(array, arrayIndex);
		}

		public void EnsureCapacity(int capacity) {
			_dictionary.EnsureCapacity(capacity);
			_list.EnsureCapacity(capacity);
		}

		public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _list.GetEnumerator();
		}

		IDictionaryEnumerator IOrderedDictionary.GetEnumerator() {
			return new Enumerator(_list.GetEnumerator());
		}

		IDictionaryEnumerator IDictionary.GetEnumerator() {
			return new Enumerator(_list.GetEnumerator());
		}

		public KeyValuePair<K, V> GetItem(int index) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count, nameof(index));
			return _list[index];
		}

		public int IndexOf(KeyValuePair<K, V> item) {
			return _list.FindIndex(x =>
				_dictionary.Comparer.Equals(x.Key, item.Key)
				&& EqualityComparer<V>.Default.Equals(x.Value, item.Value)
			);
		}

		public void Insert(int index, KeyValuePair<K, V> item) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count + 1, nameof(index));
			ThrowIfKeyIsNull(item.Key, nameof(item));
			ThrowIfKeyIsDuplicate(item.Key, _dictionary.ContainsKey, nameof(item));
			DoInsert(index, item.Key, item.Value);
		}

		void IOrderedDictionary.Insert(int index, object key, object? value) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count + 1, nameof(index));
			K k = Exceptions.Argument.ThrowIfNotOfType<K>(key, nameof(key));
			ThrowIfKeyIsNull(k, nameof(key));
			ThrowIfKeyIsDuplicate(k, _dictionary.ContainsKey, nameof(key));
			V? v = Exceptions.Argument.ThrowIfNotOfType<V>(value, nameof(key));
			DoInsert(index, k, v);
		}

		public void InsertRange(int index, IEnumerable<KeyValuePair<K, V>> collection) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, _dictionary.Count, nameof(index));
			ArgumentNullException.ThrowIfNull(collection, nameof(collection));
			var list = collection.EnumerateMoreThanOnce();
			ThrowIfAnyKeyIsInvalid(list, usedKeys: null, nameof(collection));
			DoInsertRange(index, list);
		}

		public void OrderBy(IEnumerable<K> keys) {
			ArgumentNullException.ThrowIfNull(keys, nameof(keys));
			var list = keys.EnumerateMoreThanOnce();
			Exceptions.Argument.ThrowIfCountNotEqual(list.Count(), Count, nameof(keys));
			ThrowIfAnyOrderKeyIsInvalid(list, _dictionary.ContainsKey, usedKeys: null, nameof(keys));
			DoOrderBy(keys);
		}

		public bool Remove(K key) {
			ThrowIfKeyIsNull(key, nameof(key));
			if( _dictionary.ContainsKey(key) ) {
				DoRemove(key);
				return true;
			}
			else {
				return false;
			}
		}

		public bool Remove(KeyValuePair<K, V> item) {
			ThrowIfKeyIsNull(item.Key, nameof(item));
			if( _dictionary.ContainsKey(item.Key) ) {
				int index = IndexOf(item);
				DoRemoveAt(index);
				return true;
			}
			else {
				return false;
			}
		}

		void IDictionary.Remove(object key) {
			K k = Exceptions.Argument.ThrowIfNotOfType<K>(key, nameof(key));
			ThrowIfKeyIsNull(k, nameof(key));
			DoRemove(k);
		}

		public void RemoveAt(int index) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count, nameof(index));
			DoRemoveAt(index);
		}

		public void RemoveRange(int index, int count) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count, nameof(index));
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(count, Count - index + 1, nameof(index));
			DoRemoveRange(index, count);
		}

		public void SetItem(int index, KeyValuePair<K, V> item) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count, nameof(index));
			ThrowIfKeyIsNull(item.Key, nameof(item));
			K currentKey = _keys[index];
			if( _dictionary.Comparer.Equals(item.Key, currentKey) ) {
				DoSetItemWithReplacement(index, currentKey, item.Value);
			}
			else {
				ThrowIfKeyIsDuplicate(item.Key, _dictionary.ContainsKey, nameof(item));
				DoSetItemWithNew(index, currentKey, item.Key, item.Value);
			}
		}

		public void TrimExcess() {
			_dictionary.TrimExcess();
			_list.TrimExcess();
		}

		public void TrimExcess(int capacity) {
			_dictionary.TrimExcess(capacity);
			if( capacity == Count )
				_list.TrimExcess();
		}

		public bool TryAdd(K key, V value) {
			ThrowIfKeyIsNull(key, nameof(key));
			if( !_dictionary.ContainsKey(key) ) {
				DoAdd(key, value);
				return true;
			}
			else {
				return false;
			}
		}

		public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value) {
			return _dictionary.TryGetValue(key, out value);
		}

		private static void ThrowIfAnyKeyIsInvalid(IEnumerable<KeyValuePair<K, V>> collection, ISet<K>? usedKeys = null, string? paramName = null) {
			usedKeys ??= new HashSet<K>();
			foreach( var (key, _) in collection ) {
				ThrowIfKeyIsNull(key, paramName);
				ThrowIfKeyIsDuplicate(key, usedKeys.Contains, paramName);
				usedKeys.Add(key);
			}
		}

		private static void ThrowIfAnyOrderKeyIsInvalid(IEnumerable<KeyValuePair<K, V>> collection, Func<K, bool> exists, ISet<K>? usedKeys = null, string? paramName = null) {
			usedKeys ??= new HashSet<K>();
			foreach( var (key, _) in collection ) {
				ThrowIfOrderKeyIsInvalid(key, exists, usedKeys.Contains, paramName);
				usedKeys.Add(key);
			}
		}

		private static void ThrowIfAnyOrderKeyIsInvalid(IEnumerable<K> keys, Func<K, bool> exists, ISet<K>? usedKeys = null, string? paramName = null) {
			usedKeys ??= new HashSet<K>();
			foreach( K key in keys ) {
				ThrowIfOrderKeyIsInvalid(key, exists, isDuplicate: usedKeys.Contains, paramName: paramName);
				usedKeys.Add(key);
			}
		}

		private static void ThrowIfKeyDoesNotExist(K key, Func<K, bool> isValid, string? paramName = null) {
			if( !isValid(key) )
				Exceptions.Argument.Throw($"The given key \"{key}\" was not present in the dictionary.", paramName);
		}

		private static void ThrowIfKeyIsDuplicate(K key, Func<K, bool> contains, string? paramName = null) {
			if( contains(key) )
				Exceptions.Argument.Throw($"The given key \"{key}\" was already present in the dictionary.", paramName);
		}

		private static void ThrowIfKeyIsNull(K? key, string? paramName = null) {
			if( key == null )
				throw new ArgumentNullException(paramName, "Keys cannot be null.");
		}

		private static void ThrowIfOrderKeyIsInvalid(K key, Func<K, bool> exists, Func<K, bool> isDuplicate, string? paramName = null) {
			ThrowIfKeyIsNull(key, paramName);
			ThrowIfKeyDoesNotExist(key, exists, paramName);
			ThrowIfKeyIsDuplicate(key, isDuplicate, paramName);
		}

		private void DoAdd(K key, V? value) {
			_dictionary.Add(key, value!);
			_list.Add(new(key, value!));
		}

		private void DoAddRange(IEnumerable<KeyValuePair<K, V>> collection) {
			DoInsertRange(Count, collection);
		}

		private void DoInsert(int index, K key, V? value) {
			_dictionary.Add(key, value!);
			_list.Insert(index, new(key, value!));
		}

		private void DoInsertRange(int index, IEnumerable<KeyValuePair<K, V>> collection) {
			foreach( var (key, value) in collection )
				_dictionary.Add(key, value);
			_list.InsertRange(index, collection);
		}

		private void DoOrderBy(IEnumerable<K> keys) {
			_list.Clear();
			foreach( K key in keys )
				_list.Add(new(key, _dictionary[key]));
		}

		private void DoRemove(K key) {
			int index = _list.FindIndex(x => Comparer.Equals(key, x.Key));
			_dictionary.Remove(key);
			_list.RemoveAt(index);
		}

		private void DoRemoveAt(int index) {
			_dictionary.Remove(_list[index].Key);
			_list.RemoveAt(index);
		}

		private void DoRemoveRange(int index, int count) {
			for( int i = index; i < index + count; ++i )
				_dictionary.Remove(_list[i].Key);
			_list.RemoveRange(index, count);
		}

		private void DoSetItemWithNew(int index, K oldKey, K newKey, V? value) {
			_dictionary.Remove(oldKey);
			_dictionary.Add(newKey, value!);
			_list[index] = new(newKey, value!);
		}

		private void DoSetItemWithReplacement(int index, K key, V? value) {
			_dictionary[key] = value!;
			_list[index] = new(key, value!);
		}

		private void DoSetValue(K key, V? value) {
			if( _dictionary.ContainsKey(key) ) {
				int index = _keys.IndexOf(key);
				DoSetItemWithReplacement(index, key, value);
			}
			else {
				DoAdd(key, value);
			}
		}

		private V GetValue(K key) {
			ThrowIfKeyIsNull(key, nameof(key));
			ThrowIfKeyDoesNotExist(key, _dictionary.ContainsKey, nameof(key));
			return _dictionary[key];
		}

		private V GetValue(object key) {
			K k = Exceptions.Argument.ThrowIfNotOfType<K>(key, nameof(key));
			return GetValue(k);
		}

		private V GetValueAt(int index) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count, nameof(index));
			return _list[index].Value;
		}

		private void SetValue(object? key, object? value) {
			K? k = Exceptions.Argument.ThrowIfNotOfType<K>(key);
			ThrowIfKeyIsNull(k, nameof(key));
			V? v = Exceptions.Argument.ThrowIfNotOfType<V>(value, nameof(value));
			DoSetValue(k!, v);
		}

		private void SetValue(K key, V? value) {
			ThrowIfKeyIsNull(key, nameof(key));
			DoSetValue(key, value);
		}

		private void SetValueAt(int index, object? value) {
			Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, Count, nameof(index));
			V? v = Exceptions.Argument.ThrowIfNotOfType<V>(value, nameof(value));
			DoSetItemWithReplacement(index, _list[index].Key, v);
		}

		public readonly struct Enumerator : IEnumerator<KeyValuePair<K, V>>, IDictionaryEnumerator {
			private readonly IEnumerator<KeyValuePair<K, V>> _enumerator;

			internal Enumerator(IEnumerator<KeyValuePair<K, V>> collection) {
				_enumerator = collection;
			}

			public bool MoveNext() {
				return _enumerator.MoveNext();
			}

			public KeyValuePair<K, V> Current => _enumerator.Current;

			public void Dispose() { }

			object? IEnumerator.Current => new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);

			void IEnumerator.Reset() {
				_enumerator.Reset();
			}

			DictionaryEntry IDictionaryEnumerator.Entry => new(_enumerator.Current.Key, _enumerator.Current.Value);

			object IDictionaryEnumerator.Key => _enumerator.Current.Key;

			object? IDictionaryEnumerator.Value => _enumerator.Current.Value;
		}

		public sealed class KeyList : IList<K>, IReadOnlyList<K>, ICollection {
			private readonly IReadOnlyOrderedDictionary<K, V> _dictionary;
			private readonly IEqualityComparer<K> _comparer;

			public KeyList(OrderedDictionary<K, V> dictionary) :
				this(dictionary, dictionary.Comparer) { }

			public KeyList(IReadOnlyOrderedDictionary<K, V> dictionary, IEqualityComparer<K>? comparer = null) {
				ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
				_dictionary = dictionary;
				_comparer = comparer ?? EqualityComparer<K>.Default;
			}

			public int Count => _dictionary.Count;

			public IEqualityComparer<K> Comparer => _comparer;

			bool ICollection<K>.IsReadOnly => true;

			bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

			object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

			public K this[int index] {
				get => Pairs[index].Key;
				set => Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			private IReadOnlyList<KeyValuePair<K, V>> Pairs => _dictionary;

			void IList<K>.Insert(int index, K item) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			void ICollection<K>.Add(K item) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			void ICollection<K>.Clear() {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			public bool Contains(K item) {
				return _dictionary.ContainsKey(item);
			}

			public void CopyTo(K[] array, int arrayIndex) {
				ArgumentNullException.ThrowIfNull(array, nameof(array));
				Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(arrayIndex, array.Length, nameof(arrayIndex));
				Exceptions.Argument.ThrowIfCountLessThan(array.Length, arrayIndex + Count, nameof(array));
				for( int i = 0; i < Count; ++i )
					array[arrayIndex + i] = this[i];
			}

			void ICollection.CopyTo(Array array, int index) {
				ArgumentNullException.ThrowIfNull(array, nameof(array));
				Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, array.Length, nameof(index));
				Exceptions.Argument.ThrowIfCountLessThan(array.Length, index + Count, nameof(array));
				for( int i = 0; i < Count; ++i )
					array.SetValue(this[i], i);
			}

			public IEnumerator<K> GetEnumerator() {
				return GetKeys().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetKeys().GetEnumerator();
			}

			public int IndexOf(K item) {
				if( Contains(item) )
					for( int i = 0; i < Count; ++i )
						if( _comparer.Equals(this[i], item) )
							return i;
				return -1;
			}

			bool ICollection<K>.Remove(K item) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
				return false;
			}
			void IList<K>.RemoveAt(int index) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			private IEnumerable<K> GetKeys() {
				foreach( var (key, _) in Pairs )
					yield return key;
			}
		}

		public sealed class ValueList : IList<V>, IReadOnlyList<V>, ICollection {
			private readonly IReadOnlyOrderedDictionary<K, V> _dictionary;

			public ValueList(IReadOnlyOrderedDictionary<K, V> dictionary) {
				ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
				_dictionary = dictionary;
			}

			public int Count => _dictionary.Count;

			bool ICollection<V>.IsReadOnly => true;

			bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

			object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

			public V this[int index] {
				get => Pairs[index].Value;
				set => Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			private IReadOnlyList<KeyValuePair<K, V>> Pairs => _dictionary;

			void ICollection<V>.Add(V item) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			void ICollection<V>.Clear() {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			public bool Contains(V item) {
				return IndexOf(item) >= 0;
			}

			public void CopyTo(V[] array, int arrayIndex) {
				ArgumentNullException.ThrowIfNull(array, nameof(array));
				Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(arrayIndex, array.Length, nameof(arrayIndex));
				Exceptions.Argument.ThrowIfCountLessThan(array.Length, arrayIndex + Count, nameof(array));
				for( int i = 0; i < Count; ++i )
					array[arrayIndex + i] = this[i];
			}

			void ICollection.CopyTo(Array array, int index) {
				ArgumentNullException.ThrowIfNull(array, nameof(array));
				Exceptions.ArgumentOutOfRange.ThrowIfIndexOutOfRange(index, array.Length, nameof(index));
				Exceptions.Argument.ThrowIfCountLessThan(array.Length, index + Count, nameof(array));
				for( int i = 0; i < Count; ++i )
					array.SetValue(this[i], i);
			}

			void IList<V>.Insert(int index, V item) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			public IEnumerator<V> GetEnumerator() {
				return GetValues().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetValues().GetEnumerator();
			}

			public int IndexOf(V item) {
				for( int i = 0; i < Count; ++i )
					if( EqualityComparer<V>.Default.Equals(this[i], item) )
						return i;
				return -1;
			}

			bool ICollection<V>.Remove(V item) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
				return false;
			}

			void IList<V>.RemoveAt(int index) {
				Exceptions.NotSupported.ThrowIfReadOnly(true);
			}

			private IEnumerable<V> GetValues() {
				foreach( var (_, value) in Pairs )
					yield return value;
			}
		}
	}
}