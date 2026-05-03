using NLog;
using System; 
using System.IO;

namespace StaticHtmlGenerator;

public static class Program {
	private const string LOGS_PATH = "logs",
						 SETTINGS_PATH = "settings.json";

	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	public static void Main(string[] args) {
		Setup();
		try {
			var buildSettings = LoadBuildSettings(SETTINGS_PATH);
			var buildSystem = new BuildSystem(buildSettings);
			Clean(buildSystem);
			Build(buildSystem);
		}
		catch (Exception e) {
			logger.Fatal(e, "An unexpected error occurred. Exiting...");
		}
		finally {
			Shutdown();
		}
	}

	private static void Build(BuildSystem buildSystem) {
		logger.Info("Build...");
		buildSystem.Build();
		logger.Info("Finished building.");
	}

	private static void Clean(BuildSystem buildSystem) {
		logger.Info("Clean...");
		buildSystem.Clean();
		logger.Info("Finished cleaning.");
	}

	private static string GetDebugLogFileName() {
		return $"{DateTime.Now:yyyy-MM-ddTHH-mm-ss}_{Guid.NewGuid():n}.txt";
	}

	private static void Setup() {
		LogManager.Setup().LoadConfiguration(builder => {
			builder
				.ForLogger()
				.FilterMinLevel(LogLevel.Info)
				.WriteToConsole("${longdate}|${level:uppercase=true}|${message}");
			builder
				.ForLogger()
				.FilterMinLevel(LogLevel.Debug)
				.WriteToFile(Path.Combine(LOGS_PATH, GetDebugLogFileName()));
		});
	}

	private static void Shutdown() {
		LogManager.Shutdown();
	}

	private static BuildSettings LoadBuildSettings(string path) {
		logger.Info("Loading build settings...");
		if (!File.Exists(path)) {
			var settings = new BuildSettings();
			logger.Info("Loaded default build settings.");
			return settings;
		}
		try {
			var json = File.ReadAllText(path);
			var settings = JavaScript.FromJson<BuildSettings>(json);
			logger.Info("Loaded build settings from file \"{0}\".", path);
			return settings;
		}
		catch (Exception e) {
			logger.Error("Error loading build settings: \"{0}\"", e.Message);
			throw;
		}
	}
}