using System.Diagnostics;

using Crash.Server;
using Crash.Server.Model;
using Crash.Server.Settings;

using Microsoft.EntityFrameworkCore;

// TODO : Improve logging

var builder = WebApplication.CreateBuilder(args);
var argHandler = new ArgumentHandler();

argHandler.EnsureDefaults();
argHandler.ParseArgs(args);
if (argHandler.Exit)
{
	return;
}

var config = new ConfigHandler();

builder.Services.AddSignalR()
	.AddHubOptions<CrashHub>((hubOptions) =>
		config.Crash.SignalR.BuildCrashHubConfig(hubOptions))
	.AddJsonProtocol((jsonOptions) =>
		config.Crash.SignalR.BuildJsonConfig(jsonOptions));

builder.Services.AddDbContext<CrashContext>(options =>
			   options.UseSqlite($"Data Source={argHandler.DatabaseFileName}"));

// Do we need this?
// builder.WebHost.UseUrls(argHandler.URL);

var app = builder.Build();

// TODO : Make a nice little webpage
app.MapGet("/", () => "Welcome to Crash!");
app.MapHub<CrashHub>("/Crash");

if (Debugger.IsAttached)
{
	app.MapGet("/debug", () => "Debugging is enabled!");
}

app.MigrateDatabase<CrashContext>();
app.Run();
