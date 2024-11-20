namespace Crash.Server
{
	public static partial class Logging
	{

		[LoggerMessage(
			EventId = (int)LogLevel.Critical,
			Level = LogLevel.Critical,
			Message = "{message}")]
		public static partial void Critical(
			this ILogger logger, string message);

		[LoggerMessage(
			EventId = (int)LogLevel.Error,
			Level = LogLevel.Error,
			Message = "{message}")]
		public static partial void Error(
			this ILogger logger, string message);

		[LoggerMessage(
			EventId = (int)LogLevel.Information,
			Level = LogLevel.Information,
			Message = "{message}")]
		public static partial void Information(
			this ILogger logger, string message);

		[LoggerMessage(
			EventId = (int)LogLevel.Warning,
			Level = LogLevel.Warning,
			Message = "{message}")]
		public static partial void Warning(
			this ILogger logger, string message);

		[LoggerMessage(
			EventId = (int)LogLevel.Debug,
			Level = LogLevel.Debug,
			Message = "{message}")]
		public static partial void Debug(
			this ILogger logger, string message);

		[LoggerMessage(
			EventId = (int)LogLevel.Trace,
			Level = LogLevel.Trace,
			Message = "{message}")]
		public static partial void Trace(
			this ILogger logger, string message);


		// Example
		[LoggerMessage(
			EventId = 100,
			Level = LogLevel.Critical,
			Message = "Could not open socket to `{hostName}`")]
		public static partial void CouldNotOpenSocket(
			this ILogger logger, string hostName);

		[LoggerMessage(
			EventId = 101,
			Level = LogLevel.Information,
			Message = "Could not find change : `{id}`")]
		public static partial void ChangeDoesNotExist(
			this ILogger logger, Guid id);

		[LoggerMessage(
			EventId = 102,
			Level = LogLevel.Error,
			Message = "User is not valid : `{name}`")]
		public static partial void UserIsNotValid(
			this ILogger logger, string name);

		[LoggerMessage(
			EventId = 103,
			Level = LogLevel.Error,
			Message = "Could not Release")]
		public static partial void CouldNotRelease(
			this ILogger logger);

		[LoggerMessage(
			EventId = 104,
			Level = LogLevel.Error,
			Message = "Could not lock change : `{id}`")]
		public static partial void CouldNotLockChange(
			this ILogger logger, Guid id);

		[LoggerMessage(
			EventId = 105,
			Level = LogLevel.Error,
			Message = "Could not unlock change : `{id}`")]
		public static partial void CouldNoUntLockChange(
			this ILogger logger, Guid id);

		[LoggerMessage(
			EventId = 106,
			Level = LogLevel.Error,
			Message = "Could not remove change")]
		public static partial void CouldNotRemoveChange(
			this ILogger logger);

		[LoggerMessage(
			EventId = 107,
			Level = LogLevel.Error,
			Message = "Could not add change")]
		public static partial void CouldNotAddChange(
			this ILogger logger);

		[LoggerMessage(
			EventId = 108,
			Level = LogLevel.Error,
			Message = "Change is not valid `{change}")]
		public static partial void ChangeIsNotValid(
			this ILogger logger, IChange change);
	}
}
