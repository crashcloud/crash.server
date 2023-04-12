using System.Runtime.CompilerServices;

using Crash.Changes.Extensions;
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
		public async Task Add(string user, Change change)
		{
			if (null == user || user == string.Empty)
				throw new ArgumentNullException($"Input {nameof(user)} is null or empty!");

			if (null == change)
				throw new ArgumentNullException($"Input {nameof(change)} is null");

			try
			{
				_context.Changes.Remove(change);
				_context.Changes.Add(change);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}

			await Clients.Others.Add(user, new Change(change));
		}

		/// <summary>Update Item in SqLite DB and notify other clients</summary>
		public async Task Update(string user, Guid id, Change change)
		{
			if (null == user || user == string.Empty)
				throw new ArgumentNullException($"Input {nameof(user)} is null or empty!");

			if (id == Guid.Empty)
				throw new ArgumentNullException($"Input {nameof(id)} is null");

			if (null == change)
				throw new ArgumentNullException($"Input {nameof(change)} is null");

			try
			{
				var removeChange = _context.Changes.FirstOrDefault(r => r.Id == id);
				if (removeChange != null)
				{
					_context.Changes.Remove(removeChange);
				}
				_context.Changes.Add(new Change(change));
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
			await Clients.Others.Update(user, id, change);
		}

		/// <summary>Delete Item in SqLite DB and notify other clients</summary>
		public async Task Delete(string user, Guid id)
		{
			if (null == user || user == string.Empty)
			{
				Console.WriteLine($"Input {nameof(user)} is null or empty!");
				return;
			}

			if (id == Guid.Empty)
				throw new ArgumentNullException($"Input {nameof(id)} is null");

			try
			{
				var change = _context.Changes.FirstOrDefault(r => r.Id == id);
				if (null == change)
					return;

				_context.Changes.Remove(change);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
			await Clients.Others.Delete(user, id);
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		public async Task Done(string user)
		{
			if (null == user || user == string.Empty)
				throw new ArgumentNullException($"Input {nameof(user)} is null or empty!");

			try
			{
				List<Change> done = new List<Change>();
				foreach (var Change in _context.Changes)
				{
					Change.RemoveAction(ChangeAction.Temporary);
					Change.RemoveAction(ChangeAction.Lock);
					// Change.AddAction(ChangeAction.Unlock);

					done.Add(Change);
				}
				_context.Changes.UpdateRange(done);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
			await Clients.Others.Done(user);
		}

		/// <summary>Lock Item in SqLite DB and notify other clients</summary>
		public async Task Select(string user, Guid id)
		{
			if (null == user || user == string.Empty)
				throw new ArgumentNullException($"Input {nameof(user)} is null or empty!");

			if (id == Guid.Empty)
				throw new ArgumentNullException($"Input {nameof(id)} is null");

			try
			{
				var modSpec = _context.Changes.FirstOrDefault(r => r.Id == id);
				if (modSpec == null)
					return;

				// modSpec.RemoveAction(ChangeAction.Temporary); // THIS COULD HAVE CAUSED ISSUES!
				// ADDING SELECT TO A CHANGE WOULD PREVENT RECIVING!!
				modSpec.RemoveAction(ChangeAction.Unlock);
				modSpec.AddAction(ChangeAction.Lock);

				_context.Changes.Update(modSpec);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
			await Clients.Others.Select(user, id);
		}

		/// <summary>Unlock Item in SqLite DB and notify other clients</summary>
		public async Task Unselect(string user, Guid id)
		{
			if (null == user || user == string.Empty)
				throw new ArgumentNullException($"Input {nameof(user)} is null or empty!");

			if (id == Guid.Empty)
				throw new ArgumentNullException($"Input {nameof(id)} is null");

			try
			{
				var modSpec = _context.Changes.FirstOrDefault(r => r.Id == id);
				if (modSpec == null)
					return;

				modSpec.RemoveAction(ChangeAction.Lock);

				_context.Changes.Update(modSpec);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
			await Clients.Others.Unselect(user, id);
		}

		/// <summary>Add Change to SqLite DB and notify other clients</summary>
		public async Task CameraChange(string user, Change change)
		{
			if (null == user || user == string.Empty)
				throw new ArgumentNullException($"Input {nameof(user)} is null or empty!");

			if (null == change)
				throw new ArgumentNullException($"Input {nameof(change)} is null");

			await Clients.Others.CameraChange(user, change);
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

			var Changes = _context.Changes.ToArray();
			await Clients.Caller.Initialize(Changes);
		}

		/// <summary>The Number of Changes</summary>
		public int Count => _context.Changes.Count();

		internal bool TryGet(Guid changeId, out Change change)
		{
#pragma warning disable CS8601 // Possible null reference assignment.
			change = _context.Changes.FirstOrDefault(c => c.Id == changeId);
#pragma warning restore CS8601 // Possible null reference assignment.

			return change is not default(Change);
		}

		internal IEnumerable<Change> GetChanges() => _context.Changes;
	}
}
