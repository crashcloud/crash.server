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
		internal readonly ILogger<CrashHub> Logger;

		public CrashHub(CrashContext database, ILogger<CrashHub> logger)
		{
			Database = database;
			Logger = logger;
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Add(IChange change)
		{
			// Validate
			if (!HubUtils.IsChangeValid(change) ||
				!change.HasFlag(ChangeAction.Add))
			{
				Logger.CouldNotAddChange();
				return Result.Err<bool>($"Change {change} is not valid!");
			}

			// Record
			await Database.AddChangeAsync(new ImmutableChange(change));
			return Result.Ok(true);
		}

		/// <summary>Delete Item in SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Remove(Guid id)
		{
			// Cannot delete what does not already exist
			if (!Database.TryGetChange(id, out _))
			{
				Logger.ChangeDoesNotExist(id);
				return Result.Err<bool>($"Change {id} does not exist!");
			}

			// Record
			await Database.AddChangeAsync(ChangeFactory.CreateDeleteRecord(id));
			return Result.Ok(true);
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Done(string user)
		{
			// Validate
			if (!HubUtils.IsUserValid(user))
			{
				Logger.UserIsNotValid(user);
				return Result.Err<bool>($"User {user} is not valid!");
			}

			// Record
			if (!await Database.DoneAsync(user))
			{
				Logger.CouldNotRelease();
				return Result.Err<bool>($"Could not release changes for {user}!");
			}

			// Update
			await Clients.Others.Done(user);
			return Result.Ok(true);
		}

		// TODO : Does this not require a user?
		/// <summary>Lock Item in SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Lock(string user, Guid id)
		{
			// Validate
			if (!HubUtils.IsUserValid(user))
			{
				Logger.UserIsNotValid(user);
				return Result.Err<bool>($"User {user} is not valid!");
			}

			// Lock or Unlock impossible if nothing to Lock or Unlock
			if (!Database.TryGetChange(id, out var latestChange))
			{
				Logger.ChangeDoesNotExist(id);
				return Result.Err<bool>($"Change {id} does not exist!");
			}

			// Record
			var lockChange = ChangeFactory.CreateLockRecord(latestChange.Type, latestChange.Id);
			await Database.AddChangeAsync(lockChange);
			return Result.Ok(true);
		}

		// TODO : Does this not require a user?
		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Unlock(string user, Guid id)
		{
			// Validate
			if (!HubUtils.IsUserValid(user))
			{
				Logger.UserIsNotValid(user);
				return Result.Err<bool>($"User {user} is not valid!");
			}

			// Lock or Unlock impossible if nothing to Lock or Unlock
			if (!Database.TryGetChange(id, out var latestChange))
			{
				Logger.ChangeDoesNotExist(id);
				return Result.Err<bool>($"Change {id} does not exist!");
			}

			// Record
			var lockChange = ChangeFactory.CreateUnlockRecord(latestChange.Type, latestChange.Id);
			await Database.AddChangeAsync(lockChange);
			return Result.Ok(true);
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Transform(IChange change)
		{
			// Record
			await Database.AddChangeAsync(new ImmutableChange(change));
			return Result.Ok(true);
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> Update(IChange change)
		{
			await Database.AddChangeAsync(new ImmutableChange(change));
			return Result.Ok(true);
		}

		// Is this how you open a connection with the server?
		// Or do you stream?
		public async Task RequestUsers()
		{
			var users = Database.GetUsers();
			await Clients.Caller.InitializeUsers(users);
		}

		// TODO : Save the last Camera alongside the User so when you log in it is where you left off
		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		private async Task<Result<bool>> CameraChange(Change change)
		{
			if (change.Owner is null)
			{
				Logger.UserIsNotValid(change.Owner);
				return Result.Err<bool>($"User {change.Owner} is not valid!");
			}

			// Update
			var userName = change.Owner;

			var followerIds = Database.Users.AsNoTracking().Where(u => u.Name.Equals(u.Follows, StringComparison.OrdinalIgnoreCase))
													.Select(u => u.Id).ToArray();
			await Clients.Users(followerIds.Where(id => !string.IsNullOrEmpty(id))!).PushChange(change);

			// TODO : This might stop Cameras sending
			// await Clients.Users(followerIds.Where(id => !string.IsNullOrEmpty(id))).PushChangesThroughStream(change);
			return Result.Ok(true);
		}

		public async Task PushChange(Change change)
		{
			await PushChangeOnly(change);
			await Clients.Others.PushChange(change);
		}

		// https://learn.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-8.0#client-to-server-streaming
		// A hub method automatically becomes a client-to-server streaming hub method when it accepts IAsyncEnumerable<T>.
		public async Task PushChangesThroughStream(IAsyncEnumerable<Change> changeStream)
		{
			await foreach (var item in changeStream)
			{
				await PushChangeOnly(item);
			}

			await Clients.Others.PushChangesThroughStream(changeStream);
		}

		private async Task<Result<bool>> PushChangeOnly(Change change)
		{
			if (change is null)
			{
				Logger.ChangeIsNotValid(change);
				return Result.Err<bool>(new ArgumentNullException(nameof(change)));
			}

			if (change.Type is null)
			{
				Logger.ChangeIsNotValid(change);
				return Result.Err<bool>(new ArgumentNullException($"{nameof(change)}.Type was null!"));
			}

			var type = change?.Type?.ToUpperInvariant();
			return type switch
			{
				CrashCameraChange => change.Action switch
				{
					ChangeAction.Add => await CameraChange(change),
					_ => Result.Err<bool>($"No action defined for {change.Action}")
				},

				// TODO : Add tests to check 3rd Party Plugins work
				CrashGeometryChange or CrashDoneChange or _ => await HandleDefaultChange(change!),
			};
		}

		private static List<ChangeAction> GetChangesInActionableOrder(ChangeAction action)
		{
			List<ChangeAction> changes = new();

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

		private async Task<Result<bool>> HandleDefaultChange(Change change)
		{
			var currentActions = new List<ChangeAction>();

			var orderedActions = GetChangesInActionableOrder(change.Action);

			List<Result<bool>> results = new();
			foreach (var action in orderedActions)
			{
				var result = action switch
				{
					ChangeAction.Locked => await Lock(change.Owner, change.Id),
					ChangeAction.Unlocked => await Unlock(change.Owner, change.Id),
					ChangeAction.Remove => await Remove(change.Id),
					ChangeAction.Release => await Done(change.Owner),
					ChangeAction.Add => await Add(change),
					ChangeAction.Update => await Update(change),
					ChangeAction.Transform => await Transform(change),

					ChangeAction.None or ChangeAction.Temporary => Result.Err<bool>($"No action defined for {action}"),

					_ => Result.Err<bool>(new NotImplementedException($"No action defined for {change.Action}")),
				};

				results.Add(result);
			}

			var errors = results!.Where(r => !r.IsSuccess).Select(r => r.ResultError);
			if (errors.Any())
				return Result.Ok(true);

			return Result.Err<bool>($"Multiple errors {string.Join("\n", errors)}");
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

			var existingUser = await Database.Users.FindAsync(change.Owner);
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
			var changeStream = changes.Select(c => new Change(c));
			await Clients.Caller.InitializeChanges(changeStream);

			var users = Database.GetUsers();
			await Clients.Caller.InitializeUsers(users);
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			if (exception is null)
				return Task.CompletedTask;

			var disconnectedMessage = $"Exception : {exception.Message}\n" +
										 $"Inner : {exception?.InnerException?.Message}\n" +
										 $"Source : {exception.Source}\n" +
										 $"Trace : {exception.StackTrace}\n" +
										 $"Data : {string.Join(", ", exception.Data)}";

			Logger.Critical(disconnectedMessage);

			return base.OnDisconnectedAsync(exception);
		}
	}
}
