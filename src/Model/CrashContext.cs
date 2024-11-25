// https://learn.microsoft.com/en-us/ef/core/modeling/

using Crash.Changes.Extensions;
using Crash.Server.Hubs;
using Crash.Server.Pages;

using Microsoft.EntityFrameworkCore.Design;

namespace Crash.Server.Model
{
	/// <summary>Implementation of DbContext to be used as SqLite DB Session</summary>
	public sealed class CrashContext : DbContext
	{

		/// <summary>Default Constructor</summary>
		public CrashContext(DbContextOptions<CrashContext> options) : base(options)
		{
			SaveChangesFailed += OnSaveChangesFailed;
		}

		/// <summary>The History of Changes</summary>
		public DbSet<ImmutableChange> Changes { get; set; }

		/// <summary>The Latest Changes</summary>
		public DbSet<MutableChange> LatestChanges { get; set; }

		public DbSet<ManageableUser> ManageableUsers { get; set; }

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
				// Logger.ChangeIsNotValid(changeRecord);
				return false;
			}

			if (string.IsNullOrEmpty(changeRecord.Owner))
			{
				// Logger.UserIsNotValid(changeRecord.Owner);
				return false;
			}

			// Add to Storage
			await Changes.AddAsync(changeRecord);
			await SetCurrentComputedChange(changeRecord);

			var noTracking = Users.AsNoTracking();
			if (!noTracking.Any(c => c.Name == changeRecord.Owner))
			{
				await Users.AddAsync(new User { Name = changeRecord.Owner, Id = "", Follows = "" });
				await SaveChangesAsync();
				return true;
			}

			return false;
		}

		private async Task SetCurrentComputedChange(ImmutableChange newChange)
		{
			if (!TryGetChange(newChange.Id, out var latestChange))
			{
				var mutable = ChangeFactory.CreateMutableFromChange(newChange);
				await LatestChanges.AddAsync(mutable);
				await SaveChangesAsync();
				return;
			}

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

		internal IEnumerable<MutableChange> GetChanges()
		{
			// We don't need to send Removed Changes
			return LatestChanges.AsNoTracking().Where(c => !c.Action.HasFlag(ChangeAction.Remove)).ToArray();
		}

		internal IEnumerable<string> GetUsers()
		{
			return Users.AsNoTracking().Select(u => u.Name).ToArray();
		}

		internal async Task<bool> DoneAsync(string user)
		{
			var result = true;

			var temporaryChanges = LatestChanges.AsNoTracking().Where(c =>
				c.Owner == user &&
				c.Action.HasFlag(ChangeAction.Temporary));

			await temporaryChanges.ForEachAsync(async change =>
			{
				var doneChange = ChangeFactory.CreateDoneRecord(change);
				var addResult = await AddChangeAsync(doneChange);
				result &= addResult;
			});

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

				if (!latestChange.HasFlag(ChangeAction.Temporary))
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

	public class CrashContextFactory : IDesignTimeDbContextFactory<CrashContext>
	{
		public CrashContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<CrashContext>();
			optionsBuilder.UseSqlite();

			return new CrashContext(optionsBuilder.Options);
		}
	}
}
