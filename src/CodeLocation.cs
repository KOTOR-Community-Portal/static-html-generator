using System.Text.Json.Serialization;

namespace StaticHtmlGenerator;

internal readonly struct CodeLocation {
	public int Line { get; }
	public int Column { get; }

	[JsonConstructor]
	public CodeLocation(int line, int column) {
		Line = line;
		Column = column;
	}
}