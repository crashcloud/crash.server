using System.Diagnostics;

using Crash.Server;
using Crash.Server.Model;

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

builder.Services.AddSignalR();

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
