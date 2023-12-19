using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace Crash.Server
{
	public class CrashLogger : ILogger, IDisposable
	{
		private readonly LogLevel _currentLevel;
		private readonly List<string> _logMessages;
		public IReadOnlyList<string> Messages => _logMessages;
		
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return this;
		}

		public CrashLogger()
		{
			_currentLevel = Debugger.IsAttached ? LogLevel.Trace : LogLevel.Information;
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
