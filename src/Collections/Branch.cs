using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StaticHtmlGenerator.Collections {
	public class Branch<T> : IBranch<T> {
		private readonly List<T> _items;
		private int? _hashCode;

		public Branch() : base() {
			_items = new();
		}

		public Branch(IEnumerable<T> items) {
			ArgumentNullException.ThrowIfNull(items, nameof(items));
			_items = new(items);
		}

		public T this[int index] => _items[index];

		T IBranch<T>.this[int index] {
			get => _items[index];
			set => Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		T IList<T>.this[int index] {
			get => _items[index];
			set => Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		public int Count => _items.Count;

		public bool IsReadOnly => true;

		public static bool operator ==(Branch<T>? left, Branch<T>? right) {
			if( left is null )
				return right is null;
			else if( right is null )
				return false;
			else
				return CheckEquals(left, right);
		}

		public static bool operator !=(Branch<T>? left, Branch<T>? right) {
			return !(left == right);
		}

		void ICollection<T>.Add(T item) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		void ICollection<T>.Clear() {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		public bool Contains(T item) {
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			_items.CopyTo(array, arrayIndex);
		}

		public bool Equals(IBranch<T>? other) {
			return other is not null && CheckEquals(this, other);
		}

		public override bool Equals(object? obj) {
			return obj is IBranch<T> other && CheckEquals(this, other);
		}

		public IEnumerator<T> GetEnumerator() {
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _items.GetEnumerator();
		}

		public override int GetHashCode() {
			return _hashCode ??= CalcHashCode();
		}

		private int CalcHashCode() {
			const int PRIME1 = 17;
			const int PRIME2 = 23;
			int hashCode = PRIME1;
			if( typeof(T).IsValueType ) {
				for( int i = 0; i < _items.Count; ++i )
					hashCode += PRIME2 * _items[i]!.GetHashCode();
			}
			else {
				for( int i = 0; i < _items.Count; ++i ) {
					var item = _items[i];
					hashCode += item == null ? PRIME2 : PRIME2 * item.GetHashCode();
				}
			}
			return hashCode;
		}

		public int IndexOf(T item) {
			return _items.IndexOf(item);
		}

		public void Insert(int index, T item) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		bool ICollection<T>.Remove(T item) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
			return default;
		}

		public void RemoveAt(int index) {
			Exceptions.NotSupported.ThrowIfReadOnly(true);
		}

		private static bool CheckEquals(IBranch<T> branch1, IBranch<T> branch2) {
			if( branch1.Count != branch2.Count )
				return false;
			var comparer = EqualityComparer<T>.Default;
			for( int i = 0; i < branch1.Count; ++i )
				if( !comparer.Equals(branch1[i], branch2[i]) )
					return false;
			return true;
		}
	}

	public static class Branch {
		public static IEnumerable<T> GetCommonAncestors<T>(this IBranch<T> branch, IBranch<T> other) {
			var comparer = EqualityComparer<T>.Default;
			int count = Math.Min(branch.Count, other.Count);
			int index = 0;
			while( index < count && comparer.Equals(branch[index], other[index]) )
				yield return other[index++];
		}
	}
}