using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace StaticHtmlGenerator {
	public static class Program {
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static void Main(string[] args) {
			string workingDirectory = Directory.GetCurrentDirectory();

			LogManager.Setup().LoadConfiguration(builder => {
				builder
					.ForLogger()
					.FilterMinLevel(LogLevel.Info)
					.WriteToConsole();
				builder
					.ForLogger()
					.FilterMinLevel(LogLevel.Debug)
					.WriteToFile(Path.Combine(workingDirectory, "logs", GetDebugLogFileName()));
			});

			string schemaPath = Path.Combine(workingDirectory, "manifest.xsd");
			string xmlPath = Path.Combine(workingDirectory, "manifest.xml");
			string ns = "https://kotor.neocities.org/v1.0/manifest";

			var schemas = new XmlSchemaSet();
			schemas.Add(ns, new XmlTextReader(schemaPath));
			var manifestXml = XDocument.Load(xmlPath);
			bool error = false;
			manifestXml.Validate(schemas, (sender, e) => {
				error = true;
				Logger.Error(e.Message);
			});
			if( !error )
				Logger.Info("Manifest validated.");

			var manifest = Manifest.Load(manifestXml);
			var todo = new Dictionary<string, IHtmlGeneratorContext>();
			foreach( var page in manifest.Pages.Values )
				todo[page.Path] = new ManifestGeneratorContext(manifest, page);
			var settings = new HtmlGeneratorSettings {
				WorkingDirectory = workingDirectory
			};
			var generator = new HtmlGenerator(settings);
			generator.Clean();
			foreach( var (path, context) in todo ) {
				if( path != "" ) {
					Logger.Info("Generating '{0}'", context.Page.Path);
					try {
						var template = manifest.Pages[path].Source;
						var htmlDoc = generator.Open(template);
						var htmlPath = Path.Combine(workingDirectory, settings.Build, context.Page.Path);
						generator.Generate(htmlDoc, context);
						Directory.CreateDirectory(Path.GetDirectoryName(htmlPath)!);
						File.WriteAllText(
							htmlPath,
							htmlDoc.DocumentNode.OuterHtml
						);
					}
					catch( Exception ex ) {
						Logger.Warn(ex);
					}
				}
			}
			generator.Build();

			LogManager.Shutdown();
		}

		private static string GetDebugLogFileName() {
			return Guid.NewGuid().ToString("n")	+ ".txt";
		}
	}
}