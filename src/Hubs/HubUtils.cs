namespace Crash.Server.Hubs
{
	
	
	/// <summary>
	/// Validation Utils for arguments in the Hub
	/// </summary>
	public class HubUtils
	{
		
		const string InvalidUserMessage = "User is null or empty!";
		internal static bool IsUserValid(string user)
		{
			if (!string.IsNullOrEmpty(user)) return true;
			Console.WriteLine(InvalidUserMessage);
			return false;
		}

		const string InvalidChangeMessage = $"Inputted Change is null";
		internal static bool IsChangeValid(Change change)
		{
			if (change is not null) return true;
			Console.WriteLine(InvalidChangeMessage);
			return false;
		}

		const string InvalidGuidMessage = $"Inputted Change Id is Guid.Empty";
		internal static bool IsGuidValid(Guid changeId)
		{
			if (Guid.Empty != changeId) return true;
			
			Console.WriteLine(InvalidGuidMessage);
			return false;
		}
	}
}
