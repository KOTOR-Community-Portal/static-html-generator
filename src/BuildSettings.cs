using StaticHtmlGenerator;
using System.IO;

namespace StaticHtmlGenerator;
internal class BuildSettings {
	private const string DEFAULT_MANIFEST_PATH        = "manifest.xml",
						 DEFAULT_MANIFEST_SCHEMA_PATH = "manifest.xsd",
						 DEFAULT_TEMPLATES_PATH       = "templates",
						 DEFAULT_COMPONENTS_PATH      = "components",
						 DEFAULT_PUBLIC_PATH          = "public",
						 DEFAULT_DIST_PATH            = "dist";

	private string workingDirectory = Directory.GetCurrentDirectory();

	public string WorkingDirectory {
		get => workingDirectory;
		set => workingDirectory = string.IsNullOrEmpty(value)
			? Directory.GetCurrentDirectory()
			: value;
	}

	public string Manifest { get; set; } = DEFAULT_MANIFEST_PATH;

	public string ManifestSchema { get; set; } = DEFAULT_MANIFEST_SCHEMA_PATH;

	public string Templates { get; set; } = DEFAULT_TEMPLATES_PATH;

	public string Components { get; set; } = DEFAULT_COMPONENTS_PATH;

	public string Public { get; set; } = DEFAULT_PUBLIC_PATH;

	public string Dist { get; set; } = DEFAULT_DIST_PATH;

	public MarkupFormat Format { get; set; }
}