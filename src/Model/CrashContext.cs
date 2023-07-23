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

		// Change this to a DbSet of Changes. (ServerChange)
		// Create a change wrapper?
		/// <summary>The Set of Changes</summary>
		public DbSet<ImmutableChange> Changes { get; set; }

		private Dictionary<Guid, Change> LatestChanges { get; init; }

		public DbSet<User> Users { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True");
			//optionsBuilder.UseSqlite();
		}

		internal async Task AddChangeAsync(ImmutableChange change)
		{
			await Changes.AddAsync(change);
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

