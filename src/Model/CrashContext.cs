// https://learn.microsoft.com/en-us/ef/core/modeling/

namespace Crash.Server.Model
{
	/// <summary>Implementation of DbContext to be used as SqLite DB Session</summary>
	public sealed class CrashContext : DbContext
	{

		/// <summary>Default Constructor</summary>
		public CrashContext(DbContextOptions<CrashContext> options) : base(options)
		{

		}

		/// <summary>The Set of Changes</summary>
		public DbSet<ImmutableChange> Changes { get; set; }

		public DbSet<MutableChange> LatestChanges { get; set; }

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
			if (!TryGetChange(changeRecord.Id, out _))
			{
				await LatestChanges.AddAsync(new MutableChange(changeRecord));
			}
			else
			{
				await SetCurrentComputedChange(changeRecord);
			}

			if (!Users.Any(c => c.Name == changeRecord.Owner))
			{
				await Users.AddAsync(new User() { Name = changeRecord.Owner, Id = "", Follows = "" });
			}

			var added = await SaveChangesAsync();
			return added == 1;
		}

		private async Task SetCurrentComputedChange(ImmutableChange newChange)
		{
			var latestChange = await LatestChanges.FindAsync(newChange.Id);

			if (latestChange is null)
			{
				await LatestChanges.AddAsync(new MutableChange(newChange));
				await SaveChangesAsync();
				return;
			}

			var change = ChangeFactory.CombineRecords(latestChange, newChange);
			LatestChanges.Update(change);
			await SaveChangesAsync();
		}

		internal bool TryGetChange(Guid changeId, out MutableChange? change)
		{
			change = LatestChanges.Find(changeId);
			return change is not null;
		}

		internal IEnumerable<MutableChange> GetChanges()
		{
			return LatestChanges.ToArray();
		}

		internal IEnumerable<string> GetUsers()
		{
			return Users.Select(u => u.Name).ToArray();
		}

		internal async Task<bool> DoneAsync(string user)
		{
			var result = false;

			// Wrap in a Task.Run call!
			foreach (var latestChange in LatestChanges)
			{
				// Does this include null owners? That's good!
				if (latestChange.Owner != user)
				{
					continue;
				}

				var doneChange = ChangeFactory.CreateDoneRecord(latestChange);
				result &= await AddChangeAsync(doneChange);
			}

			await SaveChangesAsync();

			return result;
		}
	}
}
