using System.Text.RegularExpressions;

namespace StaticHtmlGenerator.Html {
	public static class Text {
		public static string CleanPath(string path) {
			path = Regex.Replace(path, @"[\/\\]", "_");
			path = Regex.Replace(path, @"[^A-Za-z0-9\s-_]", "");
			path = Regex.Replace(path, @"\s+", " ").Trim();
			path = Regex.Replace(path, @"[\s]", "_");
			return path;
		}
	}
}