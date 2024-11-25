using System.Security.Cryptography.Xml;

using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Crash.Server.Integration.Tests;

public class CrashSyntax
{
	public async Task Test()
	{
		var s = Scenario.Create();
		var c = await s.AddCollaboratorAsync("Jake");
		var change = await c.SendBox();

		var c2 = await s.AddCollaboratorAsync("Rachel");
		var c3 = await s.AddCollaboratorAsync("Mel");

		var i = await c.SendBox();

		c2.Assert.Recieved(change);
		c3.Assert.Recieved(change);

		c.Assert.CanEdit(change);
		c2.Assert.CannotEdit(change);
		c3.Assert.CannotEdit(change);

		c.Assert.Created(change);
		await c.Release(change);

		c.Assert.CanEdit(change);
		c2.Assert.CanEdit(change);
		c3.Assert.CanEdit(change);
	}
}
