using System.Collections;
using System.Collections.Concurrent;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Crash.Server.Integration.Tests;

public class Bombard
{
	public ConcurrentDictionary<int, TestClient> Clients { get; private set; }

	public void Setup(int count)
	{
		Clients = new();
		var ints = Enumerable.Range(0, count).ToArray();
		for (int i = 0; i < count; i++)
		{
			Clients.TryAdd(i, new TestClient());
		}
	}

	[TearDown]
	public async Task TearDown()
	{
		await Parallel.ForEachAsync(Clients, async (kvp, token) =>
		{
			if (token.IsCancellationRequested) throw new TestCanceledException();

			await kvp.Value.StopAsync();
		});
	}

	[Test]
	[TestCaseSource(nameof(Loads))]
	public async Task UnleashBombardment(int clientCount)
	{
		Setup(clientCount);

		await Parallel.ForEachAsync(Clients, async (kvp, token) =>
		{
			if (token.IsCancellationRequested) throw new TestCanceledException();
			var client = kvp.Value;
			await client.StartAsync();
			Assert.That(client.Connection.State, Is.EqualTo(HubConnectionState.Connected).After(5).Seconds.PollEvery(100));
		});


		await TearDown();
	}

	private static IEnumerable<int> Loads
	{
		get
		{
			yield return 1;
			yield return 5;
			yield return 10;
			yield return 50;
			yield return 100;
		}
	}
}
