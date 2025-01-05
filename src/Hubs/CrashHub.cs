using System.Runtime.CompilerServices;

using Crash.Changes.Extensions;
using Crash.Server.Data;
using Crash.Server.Model;
using Crash.Server.Security;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[assembly: InternalsVisibleTo("Crash.Server.Tests")]

namespace Crash.Server.Hubs
{
	///<summary>Server Implementation of ICrashClient EndPoints</summary>
	[Authorize(Roles = Roles.ViewOnlyRoleName)]
	public sealed class CrashHub : Hub<ICrashClient>
	{

		// TODO: Make this configurable
		internal const string CrashGeometryChange = "CRASH.GEOMETRYCHANGE";
		internal const string CrashCameraChange = "CRASH.CAMERACHANGE";
		internal const string CrashDoneChange = "CRASH.DONECHANGE";

		internal CrashContext Database { get; }
		internal ILogger<CrashHub> Logger { get; }

		public CrashHub(CrashContext database, ILogger<CrashHub> logger)
		{
			Database = database;
			Logger = logger;
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		private async Task<bool> Add(IChange change)
		{
			// Record
			await Database.AddChangeAsync(new ImmutableChange(change));
			return true;
		}

		/// <summary>Delete Item in SqLite DB and notify other clients</summary>
		private async Task<bool> Remove(string user, Guid id)
		{
			if (!this.ChangeExistsInDatabase(id, out _)) return false;

			// Record
			return await Database.AddChangeAsync(ChangeFactory.CreateDeleteRecord(user, id));
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		private async Task<bool> Done(string user)
		{
			// Record
			if (!await Database.DoneAsync(user))
			{
				Logger.CouldNotRelease();
				return false;
			}

			// Update
			await Clients.Others.Done(user);
			return true;
		}

		/// <summary>Lock Item in SqLite DB and notify other clients</summary>
		/// NOTE : A user is required, we need to know WHO locked it
		private async Task<bool> Lock(string user, Guid id)
		{
			if (!this.ChangeExistsInDatabase(id, out var existingChange)) return false;

			// Record
			var lockChange = ChangeFactory.CreateLockRecord(existingChange.Type, existingChange.Id, user);
			await Database.AddChangeAsync(lockChange);
			return true;
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		/// NOTE : An admin might unlock, hence, a user is not required. Validation should be done elsewhere
		private async Task<bool> Unlock(Guid id)
		{
			if (!this.ChangeExistsInDatabase(id, out var existingChange)) return false;

			// Record
			var lockChange = ChangeFactory.CreateUnlockRecord(existingChange.Type, existingChange.Id);
			await Database.AddChangeAsync(lockChange);
			return true;
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		private async Task<bool> Transform(IChange change)
		{
			if (!this.IsPayloadEmpty(change)) return false;

			// Record
			await Database.AddChangeAsync(new ImmutableChange(change));
			return true;
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		private async Task<bool> Update(IChange change)
		{
			if (!this.IsPayloadEmpty(change)) return false;

			await Database.AddChangeAsync(new ImmutableChange(change));
			return true;
		}

		// Is this how you open a connection with the server?
		// Or do you stream?
		[Authorize(Roles = Roles.ViewOnlyRoleName)]
		public async Task RequestUsers()
		{
			var users = Database.GetUsers().ToAsyncEnumerable();
			await Clients.Caller.InitializeUsers(users);
		}

		// TODO : Save the last Camera alongside the User so when you log in it is where you left off
		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		private async Task<bool> CameraChange(Change change)
		{
			try
			{
				// Update
				var userName = change.Owner;

				var followerIds = Database.Users.AsNoTracking().Where(u => u.Name == u.Follows)
													.Select(u => u.Id).ToArray();
				await Clients.Users(followerIds.Where(id => !string.IsNullOrEmpty(id))!).PushChange(change);

				// TODO : This might stop Cameras sending
				// await Clients.Users(followerIds.Where(id => !string.IsNullOrEmpty(id))).PushChangesThroughStream(change);
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
				return false;
			}

			return true;
		}

		[Authorize(Roles = Roles.EditRoleName)]
		public async Task PushChange(Change change)
		{
			if (!await PushChangeOnly(change)) return;
			await Clients.Others.PushChange(change);
		}

		// https://learn.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-8.0#client-to-server-streaming
		// A hub method automatically becomes a client-to-server streaming hub method when it accepts IAsyncEnumerable<T>.
		[Authorize(Roles = Roles.EditRoleName)]
		public async Task PushChangesThroughStream(IAsyncEnumerable<Change> changeStream)
		{
			await foreach (var item in changeStream)
			{
				await PushChangeOnly(item);
			}

			await Clients.Others.PushChangesThroughStream(changeStream);
		}

		private async Task<bool> PushChangeOnly(Change change)
		{
			var type = change?.Type?.ToUpperInvariant();

			var result = type switch
			{
				null or "" => Result.Bool(false),

				CrashCameraChange => change.Action switch
				{
					ChangeAction.Add => Result.Bool(await CameraChange(change)),
					_ => Result.Bool(false, $"No action defined for {change.Action}")
				},

				CrashGeometryChange or CrashDoneChange or _ => Result.Bool(await HandleDefaultChange(change!)),
			};

			if (result.ResultError is not null)
				Logger.Exception(result.ResultError);

			return result.ResultValue;
		}

		private static List<ChangeAction> GetChangesInActionableOrder(ChangeAction action)
		{
			List<ChangeAction> changes = [];

			// Add Absolutely must be first!
			if (action.HasFlag(ChangeAction.Add)) { changes.Add(ChangeAction.Add); }

			// Remove should probably be first-ish too
			if (action.HasFlag(ChangeAction.Remove)) { changes.Add(ChangeAction.Remove); }

			// Locked & Unlocked can be in any order
			if (action.HasFlag(ChangeAction.Locked)) { changes.Add(ChangeAction.Locked); }
			if (action.HasFlag(ChangeAction.Unlocked)) { changes.Add(ChangeAction.Unlocked); }

			// Update & Transform can be in any order
			if (action.HasFlag(ChangeAction.Update)) { changes.Add(ChangeAction.Update); }
			if (action.HasFlag(ChangeAction.Transform)) { changes.Add(ChangeAction.Transform); }

			// Temporary can be in any order
			if (action.HasFlag(ChangeAction.Temporary)) { changes.Add(ChangeAction.Temporary); }

			// Last to go
			if (action.HasFlag(ChangeAction.Release)) { changes.Add(ChangeAction.Release); }

			return changes;
		}

		private async Task<bool> HandleDefaultChange(Change change)
		{
			if (!this.IsUserValid(change)) return false;
			var currentActions = new List<ChangeAction>();

			var orderedActions = GetChangesInActionableOrder(change.Action);

			List<bool> results = [];
			foreach (var action in orderedActions)
			{
				bool result = action switch
				{
					// TODO : Chain Validation in here!
					ChangeAction.Locked => await Lock(change.Owner, change.Id),
					ChangeAction.Unlocked => await Unlock(change.Id),
					ChangeAction.Remove => await Remove(change.Owner, change.Id),
					ChangeAction.Release => await Done(change.Owner),
					ChangeAction.Add => await Add(change),
					ChangeAction.Update => await Update(change),
					ChangeAction.Transform => await Transform(change),

					// No action needed
					ChangeAction.Temporary => true,

					ChangeAction.None or _ => false,
				};

				if (!result)
					Logger.CouldNotAddChange();

				results.Add(result);
			}

			return results.All(r => r);
		}

		/// <summary>On Connected send user Changes from DB</summary>
		[Authorize(Roles = Roles.ViewOnlyRoleName)]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public override async Task OnConnectedAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			var changes = Database.GetChanges();
			var changeStream = changes.Select(c => new Change(c)).ToAsyncEnumerable();

			// TODO : Fix connection bug with changes that cannot serialize changes async etc.
			// await Clients.Caller.InitializeChanges(changeStream);

			var users = Database.GetUsers().ToAsyncEnumerable();
			// await Clients.Caller.InitializeUsers(users);
		}

		[Authorize(Roles = Roles.ViewOnlyRoleName)]
		public override Task OnDisconnectedAsync(Exception exception)
		{
			if (exception is not null)
				Logger.Exception(exception);

			return Task.CompletedTask;
		}

	}
}
