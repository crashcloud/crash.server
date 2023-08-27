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
		internal readonly CrashContext _context;

		/// <summary>Initialize with SqLite DB</summary>
		public CrashHub(CrashContext context)
		{
			_context = context;
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		public async Task Add(Change change)
		{
			// Validate
			if (!HubUtils.IsChangeValid(change) ||
			    !HubUtils.IsPayloadValid(change) ||
			    !change.HasFlag(ChangeAction.Add))
			{
				return;
			}

			// Record
			if (await _context.AddChangeAsync(new ImmutableChange(change)))
			{
				// Update
				await Clients.Others.Add(change);
			}
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		public async Task Update(Change change)
		{
			// Validate
			if (!HubUtils.IsChangeValid(change) ||
			    !HubUtils.IsPayloadValid(change) ||
			    !change.HasFlag(ChangeAction.Update))
			{
				return;
			}

			// Record
			if (await _context.AddChangeAsync(new ImmutableChange(change)))
			{
				// Update
				await Clients.Others.Update(change);
			}
		}

		/// <summary>Delete Item in SqLite DB and notify other clients</summary>
		public async Task Delete(Guid id)
		{
			// Validate
			if (!HubUtils.IsGuidValid(id))
			{
				return;
			}

			// Cannot delete what does not already exist
			if (!_context.TryGetChange(id, out _))
			{
				return;
			}

			// Record
			if (await _context.AddChangeAsync(ChangeFactory.CreateDeleteRecord(id)))
			{
				// Update
				await Clients.Others.Delete(id);
			}

			;
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		public async Task Done(string user)
		{
			// Validate
			if (!HubUtils.IsUserValid(user))
			{
				return;
			}

			// Record
			if (await _context.DoneAsync(user))
			{
				// Update
				await Clients.Others.Done(user);
			}
		}

		/// <summary>Lock Item in SqLite DB and notify other clients</summary>
		public async Task Lock(string user, Guid id)
		{
			// Validate
			if (!HubUtils.IsUserValid(user) || !HubUtils.IsGuidValid(id))
			{
				return;
			}

			// Lock or Unlock impossible if nothing to Lock or Unlock
			if (!_context.TryGetChange(id, out var latestChange))
			{
				return;
			}

			// Record
			var lockChange = ChangeFactory.CreateLockRecord(latestChange.Type, latestChange.Id);
			if (await _context.AddChangeAsync(lockChange))
			{
				// Update
				await Clients.Others.Lock(user, id);
			}
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		public async Task Unlock(string user, Guid id)
		{
			// Validate
			if (!HubUtils.IsUserValid(user) || !HubUtils.IsGuidValid(id))
			{
				return;
			}

			// Lock or Unlock impossible if nothing to Lock or Unlock
			if (!_context.TryGetChange(id, out var latestChange))
			{
				return;
			}

			// Record
			var lockChange = ChangeFactory.CreateUnlockRecord(latestChange.Type, latestChange.Id);
			if (await _context.AddChangeAsync(lockChange))
			{
				// Update
				await Clients.Others.Unlock(user, id);
			}
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		public async Task CameraChange(Change change)
		{
			// validate
			if (!HubUtils.IsChangeValid(change) ||
			    !HubUtils.IsUserValid(change.Owner) ||
			    !change.HasFlag(ChangeAction.Add))
			{
				return;
			}

			// No Recording necessary

			// Update
			var userName = change.Owner;
			var followerIds = _context.Users.Where(u => u.Follows == userName).Select(u => u.Id);
			await Clients.Users(followerIds).CameraChange(change);
		}

		/// <summary>Adds or Updates a User in the User Db</summary>
		public async Task UpdateUser(Change change)
		{
			// validate
			if (!HubUtils.IsChangeValid(change) ||
			    !HubUtils.IsUserValid(change.Owner))
			{
				return;
			}

			var existingUser = _context.Users.FirstOrDefault(r => r.Name == change.Owner);
			if (existingUser is null)
			{
				var user = User.FromChange(change);
				if (user is null || !HubUtils.IsUserValid(user.Name))
				{
					return;
				}

				user.Id = Context.ConnectionId;

				// Update
				await _context.Users.AddAsync(existingUser);
				await _context.SaveChangesAsync();
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

			var changes = _context.GetChanges();
			await Clients.Caller.Initialize(changes);

			var users = _context.Users;
			await Clients.Caller.InitializeUsers(users);
		}
	}
}
