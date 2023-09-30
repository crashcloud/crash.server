using System.Runtime.CompilerServices;

using Crash.Changes.Extensions;
using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;

[assembly: InternalsVisibleTo("Crash.Server.Tests")]

namespace Crash.Server.Hubs
{
	///<summary>Server Implementation of ICrashClient EndPoints</summary>
	public sealed class CrashHub : Hub<ICrashClient>
	{
		// TODO: Make this configurable
		internal const string CrashGeometryChange = "CRASH.GEOMETRYCHANGE";
		internal const string CrashCameraChange = "CRASH.CAMERACHANGE";
		internal const string CrashDoneChange = "CRASH.DONECHANGE";

		internal readonly CrashContext Database;
		private readonly ILogger<CrashHub> Logger;

		public CrashHub(CrashContext database, ILogger<CrashHub> logger = null)
		{
			Database = database;
			Logger = logger;
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		private async Task Add(IChange change)
		{
			// Validate
			if (!HubUtils.IsChangeValid(change) ||
			    !HubUtils.IsPayloadValid(change) ||
			    !change.HasFlag(ChangeAction.Add))
			{
				Logger.CouldNotAddChange();
				return;
			}

			// Record
			await Database.AddChangeAsync(new ImmutableChange(change));
		}

		/// <summary>Delete Item in SqLite DB and notify other clients</summary>
		private async Task Remove(Guid id)
		{
			// Validate
			if (!HubUtils.IsGuidValid(id))
			{
				Logger.CouldNotRemoveChange();
				return;
			}

			// Cannot delete what does not already exist
			if (!Database.TryGetChange(id, out _))
			{
				Logger.ChangeDoesNotExist(id);
				return;
			}

			// Record
			await Database.AddChangeAsync(ChangeFactory.CreateDeleteRecord(id));
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		private async Task Done(string user)
		{
			// Validate
			if (!HubUtils.IsUserValid(user))
			{
				Logger.UserIsNotValid(user);
				return;
			}

			// Record
			if (!await Database.DoneAsync(user))
			{
				Logger.CouldNotRelease();
				return;
			}

			// Update
			await Clients.Others.Done(user);
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		private async Task DoneRange(IEnumerable<Guid> ids)
		{
			// Record
			if (await Database.DoneAsync(ids))
			{
				// Update
				await Clients.Others.DoneRange(ids);
			}
		}

		/// <summary>Lock Item in SqLite DB and notify other clients</summary>
		private async Task Lock(string user, Guid id)
		{
			// Lock or Unlock impossible if nothing to Lock or Unlock
			if (!Database.TryGetChange(id, out var latestChange))
			{
				Logger.ChangeDoesNotExist(id);
				return;
			}

			// Record
			var lockChange = ChangeFactory.CreateLockRecord(latestChange.Type, latestChange.Id);
			await Database.AddChangeAsync(lockChange);
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		private async Task Unlock(string user, Guid id)
		{
			// Validate
			if (!HubUtils.IsUserValid(user))
			{
				Logger.UserIsNotValid(user);
				return;
			}

			if (!HubUtils.IsGuidValid(id))
			{
				return;
			}

			// Lock or Unlock impossible if nothing to Lock or Unlock
			if (!Database.TryGetChange(id, out var latestChange))
			{
				Logger.ChangeDoesNotExist(id);
				return;
			}

			// Record
			var lockChange = ChangeFactory.CreateUnlockRecord(latestChange.Type, latestChange.Id);
			await Database.AddChangeAsync(lockChange);
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		private async Task Transform(IChange change)
		{
			// Record
			await Database.AddChangeAsync(new ImmutableChange(change));
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		private async Task Update(IChange change)
		{
			await Database.AddChangeAsync(new ImmutableChange(change));
		}

		// Is this how you open a connection with the server?
		// Or do you stream?
		public async Task RequestUsers()
		{
			var users = Database.GetUsers();
			await Clients.Caller.InitializeUsers(users);
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		private async Task CameraChange(Change change)
		{
			if (change.Owner is null)
			{
				Logger.UserIsNotValid(change.Owner);
				return;
			}

			// Update
			var userName = change.Owner;

			var followerIds = Database.Users.Where(u => u.Follows == userName).Select(u => u.Id).ToArray();
			await Clients.Users(followerIds).PushChange(change);
		}

		private static IEnumerable<Change> MultiplyChange(IEnumerable<Guid> ids, Change change)
		{
			var changes = new Change[ids.Count()];
			for (var i = 0; i < ids.Count(); i++)
			{
				changes[i] = new Change(change) { Id = change.Id };
			}

			return changes;
		}

		public async Task PushIdenticalChanges(IEnumerable<Guid> ids, Change change)
		{
			switch (change.Type)
			{
				case CrashDoneChange:
					await DoneRange(ids);
					break;

				default:
					await Task.WhenAll(MultiplyChange(ids, change).Select(PushChangeOnly));
					break;
			}

			await Clients.Others.PushIdenticalChanges(ids, change);
		}

		public async Task PushChange(Change change)
		{
			await PushChangeOnly(change);
			await Clients.Others.PushChange(change);
		}

		public async Task PushChanges(IEnumerable<Change> changes)
		{
			await Task.WhenAll(changes.Select(PushChangeOnly));
			await Clients.Others.PushChanges(changes);
		}

		private async Task PushChangeOnly(Change change)
		{
			if (change is null)
			{
				throw new ArgumentNullException("Change was null!");
			}

			if (change.Type is null)
			{
				throw new ArgumentNullException("Change.Type was null!");
			}

			var type = change?.Type?.ToUpperInvariant();
			switch (type)
			{
				// TODO : Other types of objects MUST (eventually) also work! What about 3rd party plugins?
				// TODO : Add tests for this effect!
				case CrashGeometryChange:
				default:
					{
						var task = change.Action switch
						{
							ChangeAction.Add => Add(change),
							ChangeAction.Add | ChangeAction.Temporary => Add(change),
							ChangeAction.Locked => Lock(change.Owner, change.Id),
							ChangeAction.Unlocked => Unlock(change.Owner, change.Id),
							ChangeAction.Remove => Remove(change.Id),
							ChangeAction.Update => Update(change),
							ChangeAction.Transform => Transform(change),
							_ => Task.CompletedTask
						};

						await task;
						return;
					}
				case CrashCameraChange:
					{
						var task = change.Action switch
						{
							ChangeAction.Add => CameraChange(change),
							_ => Task.CompletedTask
						};
						await task;
						return;
					}
				case CrashDoneChange:
					{
						var task = change.Action switch
						{
							ChangeAction.Release => Done(change.Owner),
							_ => Task.CompletedTask
						};
						await task;
						return;
					}
			}
		}

		/// <summary>Adds or Updates a User in the User Db</summary>
		internal async Task UpdateUser(Change change)
		{
			// validate
			if (!HubUtils.IsChangeValid(change))
			{
				Logger.ChangeIsNotValid(change);
				return;
			}

			if (!HubUtils.IsUserValid(change.Owner))
			{
				Logger.UserIsNotValid(change.Owner);
				return;
			}

			var existingUser = Database.Users.FirstOrDefault(r => r.Name == change.Owner);
			if (existingUser is null)
			{
				var user = User.FromChange(change);
				if (user is null || !HubUtils.IsUserValid(user.Name))
				{
					Logger.UserIsNotValid(user?.Name);
					return;
				}

				user.Id = Context.ConnectionId;

				// Update
				await Database.Users.AddAsync(existingUser);
				await Database.SaveChangesAsync();
			}

			// TODO : What if user is not null?

			// TODO : Is this required currently?
			// Useful for connected/disconnected
			// await Clients.Others.UpdateUser(change);
		}

		/// <summary>On Connected send user Changes from DB</summary>
		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();

			var changes = Database.GetChanges();
			await Clients.Caller.InitializeChanges(changes.Select(c => new Change(c)));

			var users = Database.GetUsers();
			await Clients.Caller.InitializeUsers(users);
		}
	}
}
