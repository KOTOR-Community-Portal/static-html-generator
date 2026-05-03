using NUglify;
using System;

namespace StaticHtmlGenerator;

internal static class Css {
	public static string Minify(string value) {
		var uglified = Uglify.Css(value);
		if (uglified.HasErrors) {
			throw new FormatException(string.Join("\n", uglified.Errors));
		}
		return uglified.Code;
	}
}