namespace Crash.Server
{
	/// <summary>EndPoints Interface</summary>
	public interface ICrashClient
	{
		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		Task Done(string user);

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		Task DoneRange(IEnumerable<Guid> ids);

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		Task PushChangesThroughStream(IAsyncEnumerable<Change> changes);

		/// <summary>Initialises the latest changes to a connecting client</summary>
		Task InitializeChanges(IEnumerable<Change> changes);

		/// <summary>Initialises the Users to a connecting client</summary>
		Task InitializeUsers(IEnumerable<string> users);

		/// <summary>Updates a User</summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task UpdateUser(string user);
	}
}
