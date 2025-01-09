using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StaticHtmlGenerator.Html {
	public class HtmlBranch : IReadOnlyCollection<HtmlNode> {
		private readonly HtmlNode ancestor;
		private readonly HtmlNode descendant;
		private readonly int count;

		public HtmlBranch(HtmlNode ancestor, HtmlNode descendant) {
			ArgumentNullException.ThrowIfNull(ancestor, nameof(ancestor));
			ArgumentNullException.ThrowIfNull(descendant, nameof(descendant));
			var currentNode = descendant;
			count = 1;
			while( currentNode != null && currentNode != ancestor ) {
				++count;
				currentNode = currentNode.ParentNode;
			}
			if( currentNode != ancestor )
				ThrowNotDescendant(nameof(ancestor), nameof(descendant));
			this.ancestor = ancestor;
			this.descendant = descendant;
		}

		private HtmlBranch(HtmlNode ancestor, HtmlNode descendant, int count) {
			this.ancestor = ancestor;
			this.descendant = descendant;
			this.count = count;
		}

		public HtmlNode Ancestor => ancestor;
		public HtmlNode Descendant => descendant;
		public int Count => count;

		public static HtmlBranch FromAscending(IEnumerable<HtmlNode> nodes) {
			if( nodes is IList<HtmlNode> list ) {
				return new(list[^1], list[0]);
			}
			else if( nodes is IReadOnlyList<HtmlNode> readOnlyList ) {
				return new(readOnlyList[^1], readOnlyList[0]);
			}
			else {
				list = nodes.ToList();
				return new(list[^1], list[0]);
			}
		}

		public static HtmlBranch FromDescending(IEnumerable<HtmlNode> nodes) {
			if( nodes is IList<HtmlNode> list ) {
				return new(list[0], list[^1]);
			}
			else if( nodes is IReadOnlyList<HtmlNode> readOnlyList ) {
				return new(readOnlyList[0], readOnlyList[^1]);
			}
			else {
				list = nodes.ToList();
				return new(list[0], list[^1]);
			}
		}

		public static HtmlBranch WithoutAncestor(HtmlNode ancestor, HtmlNode descendant) {
			ArgumentNullException.ThrowIfNull(ancestor, nameof(ancestor));
			ArgumentNullException.ThrowIfNull(descendant, nameof(descendant));
			if( ancestor == descendant )
				ThrowAncestorIsDescendant(nameof(ancestor), nameof(descendant));
			var currentNode = descendant;
			var currentParent = currentNode.ParentNode;
			int count = 1;
			while( currentParent != null && currentParent != ancestor ) {
				++count;
				currentNode = currentNode.ParentNode;
				currentParent = currentNode.ParentNode;
			}
			if( currentParent != ancestor )
				ThrowNotDescendant(nameof(ancestor), nameof(descendant));
			return new(currentNode, descendant, count);
		}

		public void DetermineRelation(HtmlBranch other, ICollection<HtmlNode> related, ICollection<HtmlNode> unrelated) {
			var family = GetAscending().ToHashSet();
			var otherFamily = other.GetAscending().ToList();

			for( int i = 0; i < otherFamily.Count; ++i ) {
				var currentNode = otherFamily[i];
				if( !family.Contains(currentNode) )
					unrelated.Add(currentNode);
				else
					related.Add(currentNode);
			}
			/*
			var family = GetAscending().ToHashSet();
			var relatives = other.GetAscending().SkipWhile(x =>
			{
				bool related = family.Contains(x);
				if (!related)
					unrelated.Add(x);
				return !related;
			});
			foreach( var relative in relatives )
				related.Add(relative);
			*/
		}

		public IEnumerable<HtmlNode> FindCommonAncestors(HtmlBranch other) {
			var members = new HashSet<HtmlNode>(GetAscending());
			return other.GetDescending().TakeWhile(x => members.Contains(x));
		}

		public IEnumerable<HtmlNode> GetAscending() {
			var currentNode = descendant;
			while( currentNode != ancestor ) {
				yield return currentNode;
				currentNode = currentNode.ParentNode;
			}
			yield return ancestor;
		}

		public IList<HtmlNode> GetDescending() {
			var nodes = GetAscending().ToList();
			nodes.Reverse();
			return nodes;
		}

		public IEnumerator<HtmlNode> GetEnumerator() {
			return GetAscending().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetAscending().GetEnumerator();
		}

		private static void ThrowAncestorIsDescendant(string ancestorParamName, string descendantParamName) {
			Exceptions.Argument.Throw("The specified ancestor and descendant are the same node.", ancestorParamName, descendantParamName);
		}

		private static void ThrowNotDescendant(string ancestorParamName, string descendantParamName) {
			Exceptions.Argument.Throw("The specified descendant does not descend from the specified ancestor.", ancestorParamName, descendantParamName);
		}
	}
}