using System;
using System.Collections.Generic;
using System.Linq;

namespace StaticHtmlGenerator.Collections {
	public static class Extensions {
		public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, params T[] others) {
			ArgumentNullException.ThrowIfNull(others, nameof(others));
			return items.Concat(others as IEnumerable<T>);
		}

		public static IEnumerable<T> EnumerateMoreThanOnce<T>(this IEnumerable<T> items) {
			return items is ICollection<T> || items is IReadOnlyCollection<T>
				? items
				: items.ToList();
		}

		public static int EstimateCapacity<T>(this IEnumerable<T> items) {
			if( items is ICollection<T> collection )
				return collection.Count;
			else if( items is IReadOnlyCollection<T> readOnlyCollection )
				return readOnlyCollection.Count;
			else
				return 0;
		}

		public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
			foreach( T item in items ) {
				if( !predicate(item) ) {
					yield return item;
				}
				else {
					yield return item;
					yield break;
				}
			}
		}
	}
}