using NLog;

namespace StaticHtmlGenerator;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Microsoft.Design",
	"IDE1006",
	Justification = "JavaScript naming convention.")
]
internal class JsConsole {
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	public static void log(params object[] args) => logger.Info(string.Join(' ', args));
	public static void info(params object[] args) => logger.Info(string.Join(' ', args));
	public static void debug(params object[] args) => logger.Debug(string.Join(' ', args));
	public static void warn(params object[] args) => logger.Warn(string.Join(' ', args));
	public static void error(params object[] args) => logger.Error(string.Join(' ', args));
}