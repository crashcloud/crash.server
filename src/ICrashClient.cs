namespace Crash.Server
{
	/// <summary>EndPoints Interface</summary>
	public interface ICrashClient
	{
		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		Task Done(string user);

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		Task DoneRange(IAsyncEnumerable<Guid> ids);

		/// <summary>
		///     Pushes a single changes at once
		/// </summary>
		Task PushChange(Change change);

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		Task PushChangesThroughStream(IAsyncEnumerable<Change> changes);

		/// <summary>Initialises the latest changes to a connecting client</summary>
		Task InitializeChanges(IAsyncEnumerable<Change> changes);

		/// <summary>Initialises the Users to a connecting client</summary>
		Task InitializeUsers(IAsyncEnumerable<string> users);

		/// <summary>Updates a User</summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task UpdateUser(string user);
	}
}
