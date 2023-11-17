namespace Crash.Server.Hubs
{
	/// <summary>Validation Utils for arguments in the Hub</summary>
	internal static class HubUtils
	{
		private const string InvalidUserMessage = "User is null or empty!";

		private const string InvalidChangeMessage = "Inputted Change is null";

		private const string InvalidGuidMessage = "Inputted Change Id is Guid.Empty";

		private const string InvalidPayloadMessage = "Payload is Invalid";

		/// <summary>Validates a user as being not empty or null</summary>
		internal static bool IsUserValid(string user)
		{
			if (!string.IsNullOrEmpty(user))
			{
				return true;
			}

			Console.WriteLine(InvalidUserMessage);
			return false;
		}

		/// <summary>Validates a Change as having the minimum required values</summary>
		internal static bool IsChangeValid(IChange change)
		{
			if (change is not null &&
			    change.Id != Guid.Empty &&
			    change.Action != ChangeAction.None)
			{
				return true;
			}

			Console.WriteLine(InvalidChangeMessage);
			return false;
		}

		/// <summary>Validates a Guid as being not default</summary>
		internal static bool IsGuidValid(Guid changeId)
		{
			if (Guid.Empty != changeId)
			{
				return true;
			}

			Console.WriteLine(InvalidGuidMessage);
			return false;
		}

		/// <summary>Validates a Payload as being not empty</summary>
		internal static bool IsPayloadValid(IChange change)
		{
			if (!string.IsNullOrEmpty(change?.Payload))
			{
				return true;
			}

			Console.WriteLine(InvalidUserMessage);
			return false;
		}
	}
}
