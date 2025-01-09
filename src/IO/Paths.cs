using StaticHtmlGenerator.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticHtmlGenerator.IO {
	public static class Paths {
		public static bool Equals(string first, string second) {
			ArgumentNullException.ThrowIfNull(first, nameof(first));
			ArgumentNullException.ThrowIfNull(second, nameof(second));
			return Path.GetRelativePath(first, second) == ".";
		}

		public static IEnumerable<string> GetNamesFromRootToDirectory(string path) {
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			return new DirectoryInfo(path).GetNamesFromRootToDirectory();
		}

		public static IEnumerable<string> GetNamesFromDirectoryToRoot(string path) {
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			return new DirectoryInfo(path).GetNamesFromDirectoryToRoot();
		}

		public static IEnumerable<string> GetFullNamesFromRootToDirectory(string path) {
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			return new DirectoryInfo(path).GetFullNamesFromRootToDirectory();
		}

		public static IEnumerable<string> GetFullNamesFromDirectoryToRoot(string path) {
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			return new DirectoryInfo(path).GetFullNamesFromDirectoryToRoot();
		}

		public static IEnumerable<string> GetRelatedNames(string first, string second) {
			ArgumentNullException.ThrowIfNull(first, nameof(first));
			ArgumentNullException.ThrowIfNull(second, nameof(second));
			var names1 = GetNamesFromRootToDirectory(first).ToList();
			var names2 = GetNamesFromRootToDirectory(second).ToList();
			for( int i = 0, count = Math.Min(names1.Count, names2.Count); i < count; ++i )
				if( string.Equals(names1[i], names2[i], StringComparison.OrdinalIgnoreCase) )
					yield return names1[i];
				else
					yield break;
		}

		public static IEnumerable<string> GetRelatedFullNames(string first, string second) {
			ArgumentNullException.ThrowIfNull(first, nameof(first));
			ArgumentNullException.ThrowIfNull(second, nameof(second));
			var fullNames1 = new DirectoryInfo(first).GetFullNamesFromRootToDirectory().ToList();
			var fullNames2 = new DirectoryInfo(second).GetFullNamesFromRootToDirectory().ToList();
			for( int i = 0, count = Math.Min(fullNames1.Count, fullNames2.Count); i < count; ++i )
				if( string.Equals(fullNames1[i], fullNames2[i], StringComparison.OrdinalIgnoreCase) )
					yield return fullNames1[i];
				else
					yield break;
		}

		public static bool IsRelativeToWorkingDirectory(string path) {
			return path != "."
				&& path != ".."
				&& !path.StartsWith(".." + Path.DirectorySeparatorChar)
				&& !path.StartsWith(".." + Path.AltDirectorySeparatorChar)
				&& !Path.IsPathRooted(path);
		}
		
		public static string Normalize(string path) {
			return Path
				.GetFullPath(new Uri(path.Trim()).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}
	}
}