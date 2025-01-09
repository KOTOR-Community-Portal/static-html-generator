using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StaticHtmlGenerator.IO {
	public static class Extensions {
		public static IEnumerable<string> GetNamesFromDirectoryToRoot(this DirectoryInfo info) {
			yield return info.Name;
			while( info.Parent != null ) {
				info = info.Parent;
				yield return info.Name;
			}
		}

		public static IEnumerable<string> GetNamesFromRootToDirectory(this DirectoryInfo info) {
			return info.GetNamesFromDirectoryToRoot().Reverse();
		}

		public static IEnumerable<string> GetFullNamesFromDirectoryToRoot(this DirectoryInfo info) {
			yield return info.FullName;
			while( info.Parent != null ) {
				info = info.Parent;
				yield return info.FullName;
			}
		}

		public static IEnumerable<string> GetFullNamesFromRootToDirectory(this DirectoryInfo info) {
			return info.GetFullNamesFromDirectoryToRoot().Reverse();
		}
	}
}