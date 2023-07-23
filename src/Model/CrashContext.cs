using Crash.Changes.Extensions;

namespace Crash.Server.Model
{

	/// <summary>Implementation of DbContext to be used as SqLite DB Session</summary>
	public sealed class CrashContext : DbContext
	{

		/// <summary>Default Constructor</summary>
		public CrashContext(DbContextOptions<CrashContext> options) : base(options)
		{
			LatestChanges = new Dictionary<Guid, Change>();
		}

		/// <summary>The Set of Changes</summary>
		public DbSet<ImmutableChange> Changes { get; set; }

		private Dictionary<Guid, Change> LatestChanges { get; init; }

		public DbSet<User> Users { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True");
			//optionsBuilder.UseSqlite();
		}

		internal async Task AddChangeAsync(ImmutableChange changeRecord)
		{
			if (changeRecord.Id == Guid.Empty || changeRecord.UniqueId == Guid.Empty)
				return;

			await Changes.AddAsync(changeRecord);

			if (!LatestChanges.ContainsKey(changeRecord.Id))
			{
				LatestChanges.Add(changeRecord.Id, new Change(changeRecord));
			}
			else
			{
				await SetCurrentComputedChange(changeRecord);
			}
		}

		private async Task SetCurrentComputedChange(IChange newChange)
		{
			if (!LatestChanges.TryGetValue(newChange.Id, out var current))
				return;

			// Set Payload
			if (!string.IsNullOrEmpty(newChange.Payload))
			{
				current.Payload = newChange.Payload;
			}

			// Update Add/Remove
			SetToggleableChange(newChange, ChangeAction.Add, ChangeAction.Remove);

			// Toggle Temporary)
			if (newChange.HasFlag(ChangeAction.Temporary))
			{
				current.AddAction(ChangeAction.Temporary);
			}
			else
			{
				current.RemoveAction(ChangeAction.Temporary);
			}

			// Update Lock/Unlock
			SetToggleableChange(newChange, ChangeAction.Lock, ChangeAction.Unlock);

			// Update steps ??
			// Transform steps ??
		}

		private void SetToggleableChange(IChange change, ChangeAction affirmative, ChangeAction negative)
		{
			if (change.HasFlag(affirmative))
			{
				CurrentChange.RemoveAction(negative);
				CurrentChange.AddAction(affirmative);
			}
			else if (change.HasFlag(negative))
			{
				CurrentChange.RemoveAction(affirmative);
				CurrentChange.AddAction(negative);
			}
		}

		internal bool TryGetChange(Guid changeId, out Change? change)
			=> (LatestChanges.TryGetValue(changeId, out change));

		internal IEnumerable<Change> GetChanges()
			=> LatestChanges.Values;

		internal async Task DoneAsync(string user)
		{
			// Wrap in a Task.Run call!
			foreach (var latestChange in LatestChanges)
			{
				// Doest this include null owners? That's good!
				if (latestChange.Value.Owner != user)
					continue;

				// Do we need to check for Temporary?

				var doneChange = new ImmutableChange
				{
					Action = 0,
					Id = latestChange.Value.Id,
					Owner = latestChange.Value.Owner,
					Stamp = DateTime.UtcNow,
					Type = latestChange.Value.Type,
				};
				doneChange.RemoveAction(ChangeAction.Temporary);
				doneChange.RemoveAction(ChangeAction.Lock);
				doneChange.RemoveAction(ChangeAction.Unlock);

				await AddChangeAsync(doneChange);
			}
		}

	}

}

