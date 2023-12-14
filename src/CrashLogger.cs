using System.Diagnostics;

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
		}
		
	}
	
	internal sealed class CrashLoggerProvider : ILoggerProvider
	{
		internal CrashLogger Logger;
		
		public ILogger CreateLogger(string categoryName)
		{
			if (Logger is null)
				Logger = new CrashLogger();
			
			return Logger;
		}

		public void Dispose()
		{
			Logger.Dispose();
		}
	}
	
}
