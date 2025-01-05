using Microsoft.AspNetCore.SignalR.Client;

namespace Crash.Server.Integration.Tests;

public abstract class BaseTest
{
	public TestClient Client { get; private set; }

	private bool Initialised { get; set; } = false;
	private bool InitialisedUsers { get; set; } = false;

	[SetUp]
	public async Task Setup()
	{
		Client = new TestClient();
		Client.OnInitializeChanges += async (changes) =>
		{
			Initialised = true;
		};
		Client.OnInitializeUsers += async (users) =>
		{
			InitialisedUsers = true;
		};

		await Client.StartAsync();
		Assert.That(Client.Connection.State, Is.EqualTo(HubConnectionState.Connected).After(5).Seconds.PollEvery(100));
		Assert.That(Initialised, Is.True.After(5).Seconds.PollEvery(100));
		Assert.That(InitialisedUsers, Is.True.After(5).Seconds.PollEvery(100));
	}

	[TearDown]
	public async Task TearDown()
	{
		await Client.StopAsync();
		Assert.That(Client.Connection.State, Is.EqualTo(HubConnectionState.Disconnected).After(5).Seconds.PollEvery(100));
	}

}
