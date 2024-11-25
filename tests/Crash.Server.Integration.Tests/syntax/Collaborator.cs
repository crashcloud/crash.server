using System.Text.Json;

using Crash.Changes;

namespace Crash.Server.Integration.Tests;

public class Collaborator
{

	private TestClient Client { get; }

	public CrashAssert Assert { get; }

	public string Name { get; }

	internal Collaborator(string name)
	{
		Name = name;
		Client = new TestClient();
		Assert = new CrashAssert();
		Client.OnRecievedChanges += async (changes) =>
		{
			await foreach (var change in changes)
			{
				Assert.AddItem(change);
			};
		};
	}

	internal static async Task<Collaborator> Create(string name)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty", nameof(name));
		var collab = new Collaborator(name);
		await collab.Client.StartAsync();
		return collab;
	}

	public async Task<Change> Release(Change change)
	{
		var releaseChange = new Change()
		{
			Owner = Name,
			Id = change.Id,
			Type = change.Type,
			Action = ChangeAction.Release
		};

		var changes = new List<Change> { releaseChange };
		await Client.StreamChangesAsync(changes.ToAsyncEnumerable());

		return change;
	}

	public async Task<Change> SendBox() => await SendGeometry("box");

	public async Task<Change> SendCone() => await SendGeometry("cone");

	public async Task<Change> SendSphere() => await SendGeometry("sphere");

	public async Task<Change> SendCylinder() => await SendGeometry("cylinder");

	private async Task<Change> SendGeometry(string name)
	{
		var packet = new PayloadPacket()
		{
			Data = name,
		};
		Change change = new()
		{
			Owner = Name,
			Payload = JsonSerializer.Serialize(packet),
			Type = "Geometry",
			Action = ChangeAction.Add
		};

		var changes = new List<Change> { change };
		await Client.StreamChangesAsync(changes.ToAsyncEnumerable());

		return change;
	}

	public async Task<Change> SendJunk()
	{
		Change change = new()
		{
			Owner = Name,
			Payload = string.Empty,
			Type = "Junk",
			Action = ChangeAction.Temporary
		};

		var changes = new List<Change> { change };
		await Client.StreamChangesAsync(changes.ToAsyncEnumerable());

		return change;
	}

	public async Task Delete(Change change)
	{
		var deleteChange = new Change()
		{
			Owner = Name,
			Id = change.Id,
			Type = change.Type,
			Action = ChangeAction.Remove
		};

		var changes = new List<Change> { deleteChange };
		await Client.StreamChangesAsync(changes.ToAsyncEnumerable());
		Assert.AddItem(deleteChange);
	}

}
