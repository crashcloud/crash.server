using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Crash.Server
{
	public sealed class CrashLogger : ILogger, IDisposable
	{

		public record struct LogMessage(LogLevel Level, string Message, string EventId, Exception? RealException = null);

		private readonly LogLevel _currentLevel;
		private readonly List<LogMessage> _logMessages;
		public IReadOnlyList<LogMessage> Messages => _logMessages;

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return this;
		}

		public CrashLogger(LogLevel loggingLevel = LogLevel.Information)
		{
			_currentLevel = Debugger.IsAttached ? LogLevel.Trace : loggingLevel;
			_logMessages = new List<LogMessage>();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= _currentLevel;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var eventItem = eventId.Name?.Split(".")?.LastOrDefault() ?? string.Empty;
			var formattedMessage = MessageSimplifier(formatter.Invoke(state, exception));

			_logMessages.Add(new(logLevel, formattedMessage, eventItem!, exception));
		}

		private static string MessageSimplifier(string message)
		{
			// Executed DbCommand (5ms) -> 5ms
			message = Regex.Replace(message, @"Executed DbCommand \(([\da-z]+)\)", "$1");
			message = message.Replace("Parameters=[], ", "");
			message = message.Replace("CommandType='Text', ", "");
			message = message.Replace("CommandTimeout='30'", "");
			message = message.Replace("[]", "");

			return message;
		}

		public static string LogToString(LogLevel logLevel) => logLevel switch
		{
			LogLevel.Trace => "[TRC]",
			LogLevel.Debug => "[DBG]",
			LogLevel.Information => "[INF]",
			LogLevel.Warning => "[WRN]",
			LogLevel.Error => "[ERR]",
			LogLevel.Critical => "[CRT]",

			_ => "[???]"
		};

		// https://yeun.github.io/open-color/
		public static string LogColour(LogLevel logLevel) => logLevel switch
		{
			LogLevel.Trace => "#495057",
			LogLevel.Debug => "#339af0",
			LogLevel.Information => "#40c057",
			LogLevel.Warning => "#fcc419",
			LogLevel.Error => "#f76707",
			LogLevel.Critical => "#e03131",

			_ => "black"
		};

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

	}

	internal sealed class CrashLoggerProvider : ILoggerProvider
	{
		internal CrashLogger _logger;

		internal CrashLoggerProvider(LogLevel loggingLevel)
		{
			_logger = new CrashLogger(loggingLevel);
		}

		public ILogger CreateLogger(string categoryName)
		{
			_logger ??= new CrashLogger();

			return _logger;
		}

		public void Dispose()
		{
			_logger.Dispose();
		}
	}

}
