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
		///     Pushes an Update/Transform/Payload which applies to many Changes
		///     An example of this is arraying the same item or deleting many items at once
		/// </summary>
		/// <param name="ids">The records to update</param>
		/// <param name="change">The newest changes</param>
		Task PushIdenticalChanges(IEnumerable<Guid> ids, Change change);

		/// <summary>Pushes a single Change</summary>
		Task PushChange(Change change);

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		Task PushChanges(IEnumerable<Change> changes);

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
