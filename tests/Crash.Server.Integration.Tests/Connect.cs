namespace Crash.Server.Integration.Tests;

public class Connect : BaseTest
{

	// Crash Client does not have a recieve method during a session. Whoops!

	[Test]
	public async Task SimpleBoxSend()
	{
		var s = Scenario.Create();
		var c = await s.AddCollaboratorAsync("Jake");
		var change = await c.SendBox();
		var c2 = await s.AddCollaboratorAsync("Rachel");
		var c3 = await s.AddCollaboratorAsync("Mel");

		c.Assert.CanEdit(change).And.Created(change);

		c2.Assert.Recieved(change).And.CannotEdit(change);
		c3.Assert.Recieved(change).And.CannotEdit(change);

		await c.Release(change);

		c.Assert.CanEdit(change);
		c2.Assert.CanEdit(change);
		c3.Assert.CanEdit(change);

		await c2.Delete(change);
		c.Assert.Deleted(change);
		c2.Assert.Deleted(change);
		c3.Assert.Deleted(change);
	}

}
