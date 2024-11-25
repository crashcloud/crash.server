using System.Diagnostics;
using System.Net.WebSockets;

using Crash.Changes;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Crash.Server.Integration.Tests;

public class TestClient
{

	private const string Address = "https://oasis-testing-b7f6hfa3ewaqgwdk.canadacentral-01.azurewebsites.net/crash";

	internal HubConnection Connection { get; }

	public TestClient()
	{
		Connection = GetHubConnection(new Uri(Address));
		RegisterConnections();
	}

	private static void LoggingConfigurer(ILoggingBuilder loggingBuilder)
	{
		var logLevel = Debugger.IsAttached ? LogLevel.Trace : LogLevel.Information;
		loggingBuilder.SetMinimumLevel(logLevel);
		var loggingProvider = new CrashLoggerProvider();
		loggingBuilder.AddProvider(loggingProvider);
	}

	/// <summary>Creates a connection to the Crash Server</summary>
	private static HubConnection GetHubConnection(Uri url)
	{
		return new HubConnectionBuilder()
			 .WithUrl(url)
			 .ConfigureLogging(LoggingConfigurer)
			 .WithAutomaticReconnect(new[]
						 {
						 TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100),
						 TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)
						 })
			 .Build();
	}

	public async Task StartAsync()
	{
		await Connection.StartAsync();
	}

	/// <summary>Stops the Connection</summary>
	public async Task StopAsync()
	{
		await Connection?.StopAsync();
	}

	/// <summary>Registers Local Events responding to Server calls</summary>
	private void RegisterConnections()
	{
		Connection.On<IEnumerable<Change>>(INITIALIZE, InitializeChangesAsync);
		Connection.On<IEnumerable<string>>(INITIALIZEUSERS, InitializeUsersAsync);
		Connection.On<IAsyncEnumerable<Change>>(PUSH_STREAM, SendChangesThroughStream);

		Connection.Closed += ConnectionClosedAsync;
	}

	private async Task ConnectionClosedAsync(Exception? arg)
	{
		if (string.IsNullOrEmpty(arg?.Message)) return;
		Assert.Fail(arg.Message);
	}

	private async Task SendChangesThroughStream(IAsyncEnumerable<Change> changeStream)
	{
		await Connection.SendAsync(PUSH_STREAM, changeStream);
	}

	#region Push to Server

	/// <summary>
	/// Pushes a Change to the Server
	/// </summary>
	public async Task StreamChangesAsync(IAsyncEnumerable<Change> changeStream)
	{
		try
		{
			await Connection.InvokeAsync(PUSH_STREAM, changeStream);
		}
		catch (Exception ex)
		{
			Assert.Fail(ex.Message);
		}
	}

	#endregion

	#region Recieve from Server

	/// <summary>
	/// Recieves a Change from the Server
	/// </summary>
	public async Task RecieveChangesAsync(IAsyncEnumerable<Change> changeStream)
	{
		OnRecievedChanges?.Invoke(changeStream);
	}

	public event Func<IAsyncEnumerable<Change>, Task> OnRecievedChanges;

	public event Func<IEnumerable<Change>, Task> OnInitializeChanges;

	public event Func<IEnumerable<string>, Task> OnInitializeUsers;

	private bool Initialised { get; set; }
	private bool InitialisedUsers { get; set; }
	private async Task InitializeChangesAsync(IEnumerable<Change> changes)
	{
		if (Initialised) return;
		Initialised = true;

		await OnInitializeChanges.Invoke(changes);
	}

	private async Task InitializeUsersAsync(IEnumerable<string> users)
	{
		if (InitialisedUsers) return;
		InitialisedUsers = true;

		await OnInitializeUsers.Invoke(users);
	}

	private async Task InitChangesAsync(IEnumerable<Change> changes)
	{
		OnInitializeChanges -= InitChangesAsync;
	}

	private async Task InitUsersAsync(IEnumerable<string> users)
	{
		OnInitializeUsers -= InitUsersAsync;
	}

	#endregion

	#region consts

	private const string PUSH_STREAM = "PushChangesThroughStream";
	private const string INITIALIZE = "InitializeChanges";
	private const string INITIALIZEUSERS = "InitializeUsers";

	#endregion

}
