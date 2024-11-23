﻿namespace Crash.Server
{
	/// <summary>EndPoints Interface</summary>
	public interface ICrashClient
	{
		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		Task<bool> Done(string user);

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		Task<bool> DoneRange(IAsyncEnumerable<Guid> ids);

		/// <summary>
		///     Pushes a single changes at once
		/// </summary>
		Task<bool> PushChange(Change change);

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		Task<bool> PushChangesThroughStream(IAsyncEnumerable<Change> changes);

		/// <summary>Initialises the latest changes to a connecting client</summary>
		Task<bool> InitializeChanges(IAsyncEnumerable<Change> changes);

		/// <summary>Initialises the Users to a connecting client</summary>
		Task<bool> InitializeUsers(IAsyncEnumerable<string> users);

	}
}
