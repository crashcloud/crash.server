namespace Crash.Server
{

	/// <summary>EndPoints Interface</summary>
	public interface ICrashClient
	{

		// TODO : Change to Just Change
		/// <summary>Updates the Change</summary>
		Task Update(string user, Guid id, Change Change);

		// TODO : Change to just Change
		/// <summary>Adds a Change</summary>
		Task Add(string user, Change Change);

		// TODO : Change to Just ID
		/// <summary>Deletes a Change</summary>
		Task Delete(string user, Guid id);

		/// <summary>Informs when a user has Released</summary>
		Task Done(string user);

		/// <summary>Selects a Change</summary>
		Task Select(string user, Guid id);

		/// <summary>Unselects a Change</summary>
		Task Unselect(string user, Guid id);

		// TODO : Change to IEnumerable?
		/// <summary>On Init</summary>
		Task Initialize(Change[] changes);

		/// <summary>Initialises Users</summary>
		// Task InitializeUsers(User[] users])

		// TODO : Change to just Change
		/// <summary>Communicates View Changes</summary>
		Task CameraChange(string user, Change Change);

		Task UpdateUser(Change change);

	}
}
