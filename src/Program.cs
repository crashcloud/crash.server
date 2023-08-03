using System.Diagnostics;

using Crash.Server;
using Crash.Server.Model;
using Crash.Server.Settings;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

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

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<CrashContext>();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			var accessToken = context.Request.Query["access_token"];

			// If the request is for our hub...
			var path = context.HttpContext.Request.Path;
			if (!string.IsNullOrEmpty(accessToken) &&
				(path.StartsWithSegments("/hubs/chat")))
			{
				// Read the token out of the query string
				context.Token = accessToken;
			}
			return Task.CompletedTask;
		}
	};
});

builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();

// Do we need this?
// builder.WebHost.UseUrls(argHandler.URL);

var app = builder.Build();

app.UseAuthentication();

// TODO : Make a nice little webpage
app.MapGet("/", () => "Welcome to Crash!");
app.MapHub<CrashHub>("/Crash");

if (Debugger.IsAttached)
{
	app.MapGet("/debug", () => "Debugging is enabled!");
}

app.MigrateDatabase<CrashContext>();
app.Run();
