using Microsoft.EntityFrameworkCore;

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
		public DbSet<Change> Changes { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True");
			//optionsBuilder.UseSqlite();
		}
	}
}

