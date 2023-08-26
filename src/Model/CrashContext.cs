using System.Text.Json;

using Crash.Changes.Extensions;
using Crash.Geometry;

// https://learn.microsoft.com/en-us/ef/core/modeling/
namespace Crash.Server.Model
{

	/// <summary>Implementation of DbContext to be used as SqLite DB Session</summary>
	public sealed class CrashContext : DbContext
	{

		/// <summary>Default Constructor</summary>
		public CrashContext(DbContextOptions<CrashContext> options) : base(options)
		{
			LatestChanges = new ();
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

		private async Task SetCurrentComputedChange(ImmutableChange newChange)
		{
			// Set as is if not there?
			if (!LatestChanges.TryGetValue(newChange.Id, out var current))
			{
				LatestChanges.Add(newChange.Id, new Change(newChange));
				return;
			}

			var change = ChangeFactory.CombineRecords(current, newChange);

			LatestChanges[newChange.Id] = change;
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
				// Does this include null owners? That's good!
				if (latestChange.Value.Owner != user)
					continue;
				
				var doneChange = ChangeFactory.CreateDoneRecord(latestChange.Value);
				await AddChangeAsync(doneChange);
			}
		}

	}

}

