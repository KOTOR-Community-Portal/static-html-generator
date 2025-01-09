using StaticHtmlGenerator.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace StaticHtmlGenerator {
	public class HtmlGeneratorSettings {
		private const string DEFAULT_BUILD_PATH = "build",
							 DEFAULT_PUBLIC_PATH = "public";

		private string _workingDirectory = Directory.GetCurrentDirectory();

		private string _build = DEFAULT_BUILD_PATH;

		private string _public = DEFAULT_PUBLIC_PATH;

		private readonly Dictionary<string, string> _pathMap = new();

		public string WorkingDirectory {
			get => _workingDirectory;
			set {
				value = Paths.Normalize(value ?? "");
				if( value == "" ) {
					_workingDirectory = Directory.GetCurrentDirectory();
				}
				else {
					_workingDirectory = value;
				}
			}
		}

		public string Build {
			get => _build;
			set {
				value = Paths.Normalize(value ?? "");
				if( value == "" ) {
					_build = DEFAULT_BUILD_PATH;
				}
				else {
					ThrowIfNotRelativePath(value);
					ThrowIfSamePath(nameof(Build), value, nameof(Public), _public);
					_public = value;
				}
			}
		}

		public string Public {
			get => _public;
			set {
				value = Paths.Normalize(value ?? "");
				if( value == "" ) {
					_public = DEFAULT_PUBLIC_PATH;
				}
				else {
					ThrowIfNotRelativePath(value);
					ThrowIfSamePath(nameof(Public), value, nameof(Build), _build);
					_public = value;
				}
			}
		}

		public string GetBuildPath() {
			return Path.Combine(_workingDirectory, _build);
		}

		public string GetPublicPath() {
			return Path.Combine(_workingDirectory, _public);
		}

		private static void ThrowIfNotRelativePath(string path) {
			if( Path.IsPathRooted(path) )
				throw new ArgumentException("Path must be relative.");
		}

		private static void ThrowIfSamePath(string name1, string path1, string name2, string path2) {
			if( Paths.Equals(path1, path2) )
				throw new ArgumentException($"Paths cannot be the same. (Properties: '{name1}' and '{name2}')");
		}
	}
}