using AngleSharp.Html.Parser;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using Jint.Native;
using NUglify.JavaScript;
using NUglify;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StaticHtmlGenerator;

internal partial class JavaScript : IDisposable {
	private static readonly JsonSerializerOptions jsonSerializationOptions = new(JsonSerializerDefaults.Web) {
		Converters = { new JsonStringEnumConverter() }
	};

	public IJsEngine Engine { get; }

	public JavaScript(string workingDirectory = null) {
		workingDirectory = string.IsNullOrEmpty(workingDirectory)
			? Directory.GetCurrentDirectory()
			: workingDirectory;
		Engine = CreateEngine();
		InitializeEngine(workingDirectory);
	}

	public static T FromJson<T>(string value) {
		return JsonSerializer.Deserialize<T>(value, jsonSerializationOptions);
	}

	public static string Minify(string value) {
		var codeSettings = new CodeSettings() {
			PreserveFunctionNames = true,
			RemoveUnneededCode = false,
			StrictMode = true,
		};
		var result = Uglify.Js(value, codeSettings);
		if (result.HasErrors) {
			throw new FormatException(string.Join("\n", result.Errors));
		}
		return result.Code;
	}

	public static string ReadErrorMessage(Exception e) {
		string msg = e.Message;
		string prefix = "Error:";
		int start = msg.StartsWith(prefix) ? prefix.Length : 0;
		int end = msg.IndexOf('\n');
		return (end < 0 ? msg[start..] : msg[start..end]).Trim();
	}

	public static string ToJson<T>(T value) {
		return JsonSerializer.Serialize(value, jsonSerializationOptions);
	}

	public static bool TryParseError(string msg, out string what, out CodeLocation where) {
		try {
			var match = ErrorRegex().Match(msg);
			if (match.Success) {
				what = match.Groups[1].Value.TrimEnd();
				where = FromJson<CodeLocation>(match.Groups[2].Value);
				return true;
			}
		}
		catch { }
		what = default;
		where = default;
		return false;
	}

	private static IJsEngine CreateEngine() {
		var switcher = JsEngineSwitcher.Current;
		switcher.EngineFactories.AddJint(new JintSettings() {
			StrictMode = true
		});
		return switcher.CreateEngine(JintJsEngine.EngineName);
	}

	[GeneratedRegex("""^(.*)({.*})$?""", RegexOptions.Multiline)]
	private static partial Regex ErrorRegex();

	public void Dispose() {
		Engine.Dispose();
	}

	private void EmbedUtil<T>(string key, T value) {
		string itemName = $"__{typeof(T).Name}";
		Engine.EmbedHostObject(itemName, value);
		Engine.CallFunction("__loadUtil", key, itemName);
	}

	private void InitializeEngine(string workingDirectory) {
		Engine.EmbedHostObject("console", new JsConsole());
		Engine.ExecuteResource("js/index.js", typeof(JavaScript).Assembly);
		EmbedUtil("fs", new JsFileSystem(workingDirectory));
		EmbedUtil("html", new JsHtmlHelper());
	}
}