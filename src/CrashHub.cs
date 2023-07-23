using System.Runtime.CompilerServices;

using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;

[assembly: InternalsVisibleTo("Crash.Server.Tests")]
namespace Crash.Server
{

	///<summary>Server Implementation of ICrashClient EndPoints</summary>
	public sealed class CrashHub : Hub<ICrashClient>
	{
		readonly CrashContext _context;

		/// <summary>Initialize with SqLite DB</summary>
		public CrashHub(CrashContext context)
		{
			_context = context;
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		public async Task Add(Change change)
		{
			if (InvalidChange(change)) return;

			try
			{
				await _context.AddChangeAsync(new ImmutableChange(change));
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				return;
			}

			await Clients.Others.Add(change);
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		public async Task Update(Change change)
		{
			if (InvalidChange(change)) return;

			try
			{
				await _context.AddChangeAsync(new ImmutableChange(change));
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				return;
			}
			await Clients.Others.Update(change);
		}

		/// <summary>Delete Item in SqLite DB and notify other clients</summary>
		public async Task Delete(Guid id)
		{
			if (InvalidGuid(id)) return;

			try
			{
				await _context.AddChangeAsync(new ImmutableChange
				{
					Id = id,
					Action = ChangeAction.Remove,
					Stamp = DateTime.UtcNow,
				});
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				return;
			}
			await Clients.Others.Delete(id);
		}


		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		public async Task Done(string user)
		{
			if (InvalidUser(user)) return;

			try
			{
				// TODO: hmm
				await _context.DoneAsync(user);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				return;
			}
			await Clients.Others.Done(user);
		}

		/// <summary>Lock Item in SqLite DB and notify other clients</summary>
		public async Task Lock(string user, Guid id)
			=> await ToggleLock(user, id, ChangeAction.Lock, () => Clients.Others.Lock(user, id));

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		public async Task Unlock(string user, Guid id)
			=> await ToggleLock(user, id, ChangeAction.Unlock, () => Clients.Others.Unlock(user, id));

		private async Task ToggleLock(string user, Guid id, ChangeAction lockStatus, Func<Task> others)
		{
			if (InvalidUser(user) || InvalidGuid(id))
				return;

			try
			{
				await _context.AddChangeAsync(new ImmutableChange
				{
					Id = id,
					Action = lockStatus,
					Stamp = DateTime.UtcNow,
				});
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				return;
			}
			await others();
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		public async Task CameraChange(Change change)
		{
			if (InvalidChange(change)) return;

			string? userName = change.Owner;
			var followerIds = _context.Users.Where(u => u.Follows == userName).Select(u => u.Id);
			await Clients.Users(followerIds).CameraChange(change);
		}

		/// <summary>Adds or Updates a User in the User Db</summary>
		public async Task UpdateUser(Change change)
		{
			var existingUser = _context.Users.FirstOrDefault(r => r.Name == change.Owner);
			if (null == existingUser)
			{
				User? user = User.FromChange(change);
				if (null == user || string.IsNullOrEmpty(user.Name)) return;
				user.Id = Context.ConnectionId;

				_context.Users.Add(existingUser);
				await _context.SaveChangesAsync();
			}

			// TODO : Is this required currently?
			// Useful for connected/disconnected
			// await Clients.Others.UpdateUser(change);
		}

		/// <summary>User disconnects</summary>
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			// Does this account for not reconnecting?
			return base.OnDisconnectedAsync(exception);
		}

		/// <summary>On Connected send user Changes from DB</summary>
		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();

			var changes = _context.GetChanges();
			await Clients.Caller.Initialize(changes);

			var users = _context.Users;
			await Clients.Caller.InitializeUsers(users);
		}

		/// <summary>The Number of Changes</summary>
		public int Count => _context.Changes.Count();

		internal bool TryGet(Guid changeId, out Change? change)
			=> _context.TryGetChange(changeId, out change);

		internal IEnumerable<Change> GetChanges() => _context.GetChanges();

		#region Validity Checks

		const string InvalidUserMessage = "Inputted user is null or empty!";
		private static bool InvalidUser(string user)
		{
			if (!string.IsNullOrEmpty(user)) return false;
			Console.WriteLine(InvalidUserMessage);
			return true;
		}

		const string InvalidChangeMessage = $"Inputted Change is null";
		private static bool InvalidChange(Change change)
		{
			if (change is not null) return false;
			Console.WriteLine(InvalidChangeMessage);
			return true;
		}

		const string InvalidGuidMessage = $"Inputtd Change Id is Guid.Empty";
		private static bool InvalidGuid(Guid changeId)
		{
			if (Guid.Empty != changeId) return false;
			Console.WriteLine(InvalidGuidMessage);
			return true;
		}

		#endregion

	}
}
