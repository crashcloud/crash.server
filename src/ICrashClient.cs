namespace Crash.Server
{
	/// <summary>EndPoints Interface</summary>
	public interface ICrashClient
	{
		/// <summary>Updates the Change</summary>
		Task Update(Change change);

		/// <summary>Adds a Change</summary>
		Task Add(Change change);

		/// <summary>Deletes a Change</summary>
		Task Delete(Guid id);

		/// <summary>Informs when a user has Released</summary>
		Task Done(string user);

		/// <summary>Selects a Change</summary>
		Task Lock(string user, Guid id);

		/// <summary>Unselects a Change</summary>
		Task Unlock(string user, Guid id);

		/// <summary>On Init</summary>
		Task Initialize(IEnumerable<Change> changes);

		/// <summary>Initialises Users</summary>
		Task InitializeUsers(IEnumerable<string> users);

		Task UpdateUser(Change change);

		/// <summary>Communicates View Changes</summary>
		Task CameraChange(Change change);
	}
}
