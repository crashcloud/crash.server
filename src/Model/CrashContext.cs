// https://learn.microsoft.com/en-us/ef/core/modeling/

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

		private Dictionary<Guid, Change> LatestChanges { get; }

		public DbSet<User> Users { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True");
			//optionsBuilder.UseSqlite();
		}

		internal async Task<bool> AddChangeAsync(ImmutableChange changeRecord)
		{
			if (changeRecord.Id == Guid.Empty || changeRecord.UniqueId == Guid.Empty)
			{
				return false;
			}

			// Add to Storage
			await Changes.AddAsync(changeRecord);

			// 
			if (!LatestChanges.ContainsKey(changeRecord.Id))
			{
				LatestChanges.Add(changeRecord.Id, new Change(changeRecord));
			}
			else
			{
				await SetCurrentComputedChange(changeRecord);
			}

			return true;
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
		{
			return LatestChanges.TryGetValue(changeId, out change) && change is not null;
		}

		internal IEnumerable<Change> GetChanges()
		{
			return LatestChanges.Values;
		}

		internal async Task<bool> DoneAsync(string user)
		{
			var result = false;

			// Wrap in a Task.Run call!
			foreach (var latestChange in LatestChanges.Values)
			{
				// Does this include null owners? That's good!
				if (latestChange.Owner != user)
				{
					continue;
				}

				var doneChange = ChangeFactory.CreateDoneRecord(latestChange);
				result &= await AddChangeAsync(doneChange);
			}

			return result;
		}
	}
}
