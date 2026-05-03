using System.Collections.Generic;
using System.IO;

namespace StaticHtmlGenerator;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Microsoft.Design",
	"IDE1006",
	Justification = "JavaScript naming convention.")
]
internal class JsFileSystem {
	private readonly string workingDirectory;

	public JsFileSystem(string workingDirectory) {
		this.workingDirectory = workingDirectory;
	}

	public bool exists(string path) {
		return File.Exists(realpath(path));
	}

	public string readFile(string path) {
		return File.ReadAllText(realpath(path));
	}

	public IEnumerable<string> readdir(string path, bool recursive) {
		var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		return Directory.EnumerateFiles(path, "*", searchOption);
	}

	public string realpath(string path) {
		return Path.GetFullPath(path, workingDirectory);
	}

	public void writeFile(string path, string contents) {
		File.WriteAllText(realpath(path), contents);
	}
}