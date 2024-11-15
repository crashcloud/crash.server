using System.Diagnostics;

namespace Crash.Server
{
	internal sealed class CrashLogger : ILogger, IDisposable
	{
		private readonly LogLevel _currentLevel;
		private readonly List<string> _logMessages;
		public IReadOnlyList<string> Messages => _logMessages;
		
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return this;
		}

		public CrashLogger(LogLevel loggingLevel = LogLevel.Information)
		{
			_currentLevel = Debugger.IsAttached ? LogLevel.Trace : loggingLevel;
			_logMessages = new List<string>();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= _currentLevel;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var _eventId = eventId.Name;
			var formattedMessage = formatter.Invoke(state, exception);
			var message = $"{logLevel} : {formattedMessage} : {_eventId}";

			_logMessages.Add(message);
		}
		
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
