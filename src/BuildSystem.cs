using JavaScriptEngineSwitcher.Core;
using NLog;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace StaticHtmlGenerator;

internal class BuildSystem {
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	private readonly BuildSettings settings;

	public BuildSystem(BuildSettings settings) {
		ArgumentNullException.ThrowIfNull(settings, nameof(settings));
		this.settings = settings;
	}

	public void Build() {
		try {
			using var js = new JavaScript(settings.WorkingDirectory);
			var manifest = LoadManifest();
			var site = SiteContext.Create(manifest);
			var generator = new Generator(js.Engine, settings.Format);
			DistributePublic();
			LoadTemplates(generator);
			LoadComponents(generator);
			GenerateSite(generator, site);
		}
		catch (Exception e) {
			string msg = JavaScript.ReadErrorMessage(e);
			logger.Error(e, "Build error: \"{0}\"", msg);
			throw;
		}
	}

	public void Clean() {
		try {
			var distPath = GetFullPath(settings.Dist);
			if (!Directory.Exists(distPath)) {
				return;
			}
			var buildDirectory = new DirectoryInfo(distPath);
			foreach (var file in buildDirectory.EnumerateFiles()) {
				file.Delete();
			}
			foreach (var directory in buildDirectory.EnumerateDirectories()) {
				directory.Delete(recursive: true);
			}
		}
		catch (Exception e) {
			logger.Error(e, "Clean error: \"{0}\"", e.Message);
			throw;
		}
	}

	private static void Copy(DirectoryInfo source, DirectoryInfo destination) {
		if (source.FullName.Equals(destination.FullName, StringComparison.OrdinalIgnoreCase))
			return;
		if (!Directory.Exists(source.FullName))
			return;
		if (!Directory.Exists(destination.FullName))
			Directory.CreateDirectory(destination.FullName);
		foreach (FileInfo fileInfo in source.GetFiles())
			fileInfo.CopyTo(Path.Combine(destination.ToString(), fileInfo.Name), overwrite: true);
		foreach (DirectoryInfo nextSource in source.GetDirectories()) {
			DirectoryInfo nextDestination = destination.CreateSubdirectory(nextSource.Name);
			Copy(nextSource, nextDestination);
		}
	}

	private static void Minify(string directory) {
		foreach (var path in Directory.EnumerateFiles(directory, "*.html", SearchOption.AllDirectories)) {
			try {
				File.WriteAllText(path, Html.Minify(File.ReadAllText(path)));
			}
			catch (Exception e) {
				logger.Error("Minify error: \"{0}\" (Path: \"{1}\")", e.Message, path);
			}
		}
		foreach (var path in Directory.EnumerateFiles(directory, "*.css", SearchOption.AllDirectories)) {
			try {
				File.WriteAllText(path, Css.Minify(File.ReadAllText(path)));
			}
			catch (Exception e) {
				logger.Error("Minify error: \"{0}\" (Path: \"{1}\")", e.Message, path);
			}
		}
		foreach (var path in Directory.EnumerateFiles(directory, "*.js", SearchOption.AllDirectories)) {
			try {
				File.WriteAllText(path, JavaScript.Minify(File.ReadAllText(path)));
			}
			catch (Exception e) {
				logger.Error("Minify error: \"{0}\" (Path: \"{1}\")", e.Message, path);
			}
		}
	}

	private void DistributePublic() {
		logger.Info("Distributing public files...");
		var publicDirectory = new DirectoryInfo(GetFullPath(settings.Public));
		var distDirectory = new DirectoryInfo(GetFullPath(settings.Dist));
		Copy(publicDirectory, distDirectory);
		if (settings.Format == MarkupFormat.Minified) {
			Minify(distDirectory.FullName);
		}
	}

	private void GeneratePage(Generator generator, PageContext page) {
		string contents = generator.Generate(page);
		string filePath = Path.Combine(GetFullPath(settings.Dist), page.Path);
		string directoryPath = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directoryPath)) {
			Directory.CreateDirectory(directoryPath);
		}
		File.WriteAllText(filePath, contents);
	}

	private void GenerateSite(Generator generator, SiteContext site) {
		generator.LoadSite(site);
		int successCount = 0;
		int errorCount = 0;
		foreach (var (path, page) in site.Pages) {
			try {
				logger.Info("Generating page \"{0}\"...", path);
				GeneratePage(generator, page);
				++successCount;
			}
			catch (Exception e) {
				++errorCount;
				if (e is JsException) {
					string msg = JavaScript.ReadErrorMessage(e);
					logger.Error(e, "Generator script error: \"{0}\" (Path: \"{1}\")", msg, path);
				}
				else {
					logger.Error(e, "Generator error: \"{0}\" (Path: \"{1}\")", e.Message, path);
				}
			}
		}
		logger.Info("Generated {0} {1}.", successCount, successCount == 1 ? "page" : "pages");
		if (errorCount > 0) {
			logger.Warn("Failed to generate {0} {1}.", errorCount, errorCount == 1 ? "page" : "pages");
		}
	}

	private string GetFullPath(string path) {
		return Path.GetFullPath(path, settings.WorkingDirectory);
	}

	private void LoadComponents(Generator generator) {
		int successCount = 0;
		int errorCount = 0;
		var componentsPath = GetFullPath(settings.Components);
		foreach (var path in Directory.EnumerateFiles(componentsPath, "*.js", SearchOption.AllDirectories)) {
			try {
				logger.Info("Loading component \"{0}\"...", path);
				generator.LoadComponent(path);
				++successCount;
			}
			catch (Exception e) {
				++errorCount;
				if (e is JsException) {
					string msg = JavaScript.ReadErrorMessage(e);
					if (JavaScript.TryParseError(msg, out string what, out CodeLocation where)) {
						logger.Error("Component script error: \"{0}\" (Path: \"{1}\", Line: {2}, Column: {3})", what, path, where.Line, where.Column);
					}
					else {
						logger.Error("Component script error: \"{0}\" (Path: \"{1}\")", msg, path);
					}
				}
				else {
					logger.Error(e, "Component error: \"{0}\" (Path: \"{1}\")", e.Message, path);
				}
			}
		}
		logger.Info("Loaded {0} {1}.", successCount, successCount == 1 ? "component" : "components");
		if (errorCount > 0) {
			logger.Warn("Failed to load {0} {1}.", errorCount, errorCount == 1 ? "component" : "components");
		}
	}

	private void LoadTemplates(Generator generator) {
		int successCount = 0;
		int errorCount = 0;
		var templatesPath = GetFullPath(settings.Templates);
		foreach (var path in Directory.EnumerateFiles(templatesPath, "*.js", SearchOption.AllDirectories)) {
			try {
				logger.Info("Loading template \"{0}\"...", path);
				generator.LoadTemplate(path);
				++successCount;
			}
			catch (Exception e) {
				++errorCount;
				if (e is JsException) {
					string msg = JavaScript.ReadErrorMessage(e);
					if (JavaScript.TryParseError(msg, out string what, out CodeLocation where)) {
						logger.Error("Template script error: \"{0}\" (Path: \"{1}\", Line: {2}, Column: {3})", what, path, where.Line, where.Column);
					}
					else {
						logger.Error("Template script error: \"{0}\" (Path: \"{1}\")", msg, path);
					}
				}
				else {
					logger.Error(e, "Template error: \"{0}\" (Path: \"{1}\")", e.Message, path);
				}
			}
		}
		logger.Info("Loaded {0} {1}.", successCount, successCount == 1 ? "template" : "templates");
		if (errorCount > 0) {
			logger.Warn("Failed to load {0} {1}.", errorCount, errorCount == 1 ? "template" : "templates");
		}
	}

	private XDocument LoadManifest() {
		string manifestPath = GetFullPath(settings.Manifest);
		string schemaPath = GetFullPath(settings.ManifestSchema);
		var manifest = XDocument.Load(manifestPath);
		string ns = manifest.Root.GetDefaultNamespace().NamespaceName;
		var schemas = new XmlSchemaSet();
		schemas.Add(ns, new XmlTextReader(schemaPath));
		manifest.Validate(schemas, null);
		return manifest;
	}
}