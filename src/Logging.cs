namespace Crash.Server
{
	public static partial class Logging
	{
		// Example
		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Critical,
			Message = "Could not open socket to `{hostName}`")]
		public static partial void CouldNotOpenSocket(
			this ILogger logger, string hostName);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Information,
			Message = "Could not find change : `{id}`")]
		public static partial void ChangeDoesNotExist(
			this ILogger logger, Guid id);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "User is not valid : `{name}`")]
		public static partial void UserIsNotValid(
			this ILogger logger, string name);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "Could not Release")]
		public static partial void CouldNotRelease(
			this ILogger logger);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "Could not lock change : `{id}`")]
		public static partial void CouldNotLockChange(
			this ILogger logger, Guid id);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "Could not unlock change : `{id}`")]
		public static partial void CouldNoUntLockChange(
			this ILogger logger, Guid id);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "Could not remove change")]
		public static partial void CouldNotRemoveChange(
			this ILogger logger);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "Could not add change")]
		public static partial void CouldNotAddChange(
			this ILogger logger);

		[LoggerMessage(
			EventId = 0,
			Level = LogLevel.Error,
			Message = "Change is not valid `{change}")]
		public static partial void ChangeIsNotValid(
			this ILogger logger, IChange change);
	}
}
