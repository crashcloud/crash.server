using Crash.Server.Hubs;
using Crash.Server.Model;

namespace Crash.Server
{

	public class Program
	{
		/// <summary>Creates an instance of the Crash WebApplication</summary>
		public static WebApplication CreateApplication(params string[] args)
		{
			var argHandler = new ArgumentHandler();

			argHandler.EnsureDefaults();
			argHandler.ParseArgs(args);
			if (argHandler.Exit)
			{
				return null;
			}

			if (argHandler.ResetDB && File.Exists(argHandler.DatabaseFileName))
			{
				File.Delete(argHandler.DatabaseFileName);
			}

			var webBuilder = WebApplication.CreateBuilder(args);

			/* TODO : Add back in appsettings
			webBuilder.Services.AddSignalR()
				.AddHubOptions<CrashHub>(hubOptions =>
				{

				})
				.AddJsonProtocol(jsonOptions =>
				{

				});
			i*/

			webBuilder.Services.AddDbContext<CrashContext>(options =>
				options.UseSqlite($"Data Source={argHandler.DatabaseFileName}"));

			// Do we need this?
			webBuilder.WebHost.UseUrls(argHandler.URL);
			webBuilder.Services.AddRazorPages();

			var app = webBuilder.Build();

			app.MapHub<CrashHub>("/Crash");
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.MapRazorPages();

			app.MigrateDatabase<CrashContext>();
			return app;
		}

		public static void Main(string[] args)
		{
			var app = CreateApplication(args);
			app.Run();
		}
	}
}
