namespace Crash.Server.Integration.Tests;

public class Scenario
{

	private Dictionary<string, Collaborator> Collaborators { get; } = new(StringComparer.OrdinalIgnoreCase);

	public static Scenario Create()
	{
		var scenario = new Scenario();
		return scenario;
	}

	public async Task<Collaborator> AddCollaboratorAsync(string name)
	{
		var collab = await Collaborator.Create(name);
		Collaborators.Add(name, collab);
		return collab;
	}

	public Collaborator GetCollaborator(string name) => Collaborators[name];

}
