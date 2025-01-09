namespace StaticHtmlGenerator.Html {
	public static class XPaths {
		public static readonly string Body = "/html/body";
		public static readonly string Head = "/html/head";

		public static string WithAttribute(string name) {
			return $".//*[@{name}]";
		}

		public static string WithAttribute(string name, string value) {
			return $".//*[@{name}='{value}']";
		}

		public static string WithClass(string name) {
			return $".//*[contains(@class, '{name}')]";
		}

		public static string WithTag(string tag) {
			return $".//{tag}";
		}
	}
}