namespace Crash.Server.Hubs
{
	/// <summary>
	///     Validation Utils for arguments in the Hub
	/// </summary>
	public class HubUtils
	{
		private const string InvalidUserMessage = "User is null or empty!";

		private const string InvalidChangeMessage = "Inputted Change is null";

		private const string InvalidGuidMessage = "Inputted Change Id is Guid.Empty";

		internal static bool IsUserValid(string user)
		{
			if (!string.IsNullOrEmpty(user))
			{
				return true;
			}

			Console.WriteLine(InvalidUserMessage);
			return false;
		}

		internal static bool IsChangeValid(Change change)
		{
			if (change is not null)
			{
				return true;
			}

			Console.WriteLine(InvalidChangeMessage);
			return false;
		}

		internal static bool IsGuidValid(Guid changeId)
		{
			if (Guid.Empty != changeId)
			{
				return true;
			}

			Console.WriteLine(InvalidGuidMessage);
			return false;
		}
	}
}
