// https://learn.microsoft.com/en-us/ef/core/modeling/

using Crash.Server.Hubs;

namespace Crash.Server.Model
{
	/// <summary>Implementation of DbContext to be used as SqLite DB Session</summary>
	public sealed class CrashContext : DbContext
	{
		private readonly ILogger<CrashHub> Logger;

		/// <summary>Default Constructor</summary>
		public CrashContext(DbContextOptions<CrashContext> options, ILogger<CrashHub> logger = null) : base(options)
		{
			Logger = logger;
			SaveChangesFailed += OnSaveChangesFailed;
		}

		/// <summary>The History of Changes</summary>
		public DbSet<ImmutableChange> Changes { get; set; }

		/// <summary>The Latest Changes</summary>
		public DbSet<MutableChange> LatestChanges { get; set; }

		/// <summary>The Users</summary>
		public DbSet<User> Users { get; set; }

		private void OnSaveChangesFailed(object? sender, SaveChangesFailedEventArgs e)
		{
			// TODO: Handle Failures
			;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True");
			//optionsBuilder.UseSqlite();
		}

		internal async Task<bool> AddChangeAsync(ImmutableChange changeRecord)
		{
			if (changeRecord.Id == Guid.Empty || changeRecord.UniqueId == Guid.Empty)
			{
				Logger.ChangeIsNotValid(changeRecord);
				return false;
			}

			// Add to Storage
			await Changes.AddAsync(changeRecord);

			if (TryGetChange(changeRecord.Id, out _))
			{
				await SetCurrentComputedChange(changeRecord);
			}
			else
			{
				await LatestChanges.AddAsync(new MutableChange(changeRecord));
			}

			if (!Users.Any(c => c.Name == changeRecord.Owner) &&
			    !string.IsNullOrEmpty(changeRecord.Owner))
			{
				await Users.AddAsync(new User { Name = changeRecord.Owner, Id = "", Follows = "" });
			}

			var added = await SaveChangesAsync();
			return true;
		}

		private async Task SetCurrentComputedChange(ImmutableChange newChange)
		{
			var latestChange = await LatestChanges.FindAsync(newChange.Id);

			if (latestChange is null)
			{
				// TODO : This doesn't have the full payload packet!!
				await LatestChanges.AddAsync(new MutableChange(newChange));
				await SaveChangesAsync();
				return;
			}

			// TODO : Does this have the full payload packet??!!
			var change = ChangeFactory.CombineRecords(latestChange, newChange);
			LatestChanges.Remove(latestChange);
			await LatestChanges.AddAsync(change);
			await SaveChangesAsync();
		}

		internal bool TryGetChange(Guid changeId, out MutableChange? change)
		{
			change = LatestChanges.Find(changeId);
			return change is not null;
		}

		// TODO : All LatestChnges MUST be a combination
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
			var result = true;

			// TODO : Wrap in a Task.Run call!
			foreach (var latestChange in LatestChanges.Where(c =>
				         c.Owner == user &&
				         c.Action.HasFlag(ChangeAction.Temporary)))
			{
				var doneChange = ChangeFactory.CreateDoneRecord(latestChange);
				var addResult = await AddChangeAsync(doneChange);
				result &= addResult;
			}

			await SaveChangesAsync();

			return result;
		}

		internal async Task<bool> DoneAsync(IEnumerable<Guid> ids)
		{
			var result = true;

			foreach (var id in ids)
			{
				if (!TryGetChange(id, out var latestChange))
				{
					continue;
				}

				if (latestChange.HasFlag(ChangeAction.Temporary))
				{
					continue;
				}

				var doneRecord = ChangeFactory.CreateDoneRecord(latestChange);
				result = await AddChangeAsync(doneRecord);
			}

			await SaveChangesAsync();

			return result;
		}
	}
}
