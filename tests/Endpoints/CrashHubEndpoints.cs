using System.Collections;

using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Crash.Server.Tests.Endpoints
{

	public abstract class CrashHubEndpoints
	{
		protected CrashHub? _crashHub;
		protected CrashContext? _crashContext;

		[SetUp]
		public void Init()
		{
			SetUpContext();
		}

		[TearDown]
		public void Cleanup()
		{
			_crashHub = null;
			_crashContext = null;
		}

		private void SetUpContext()
		{
			// Create a mock DbContextOptions object
			var mockOptions = new DbContextOptionsBuilder<CrashContext>()
				.UseInMemoryDatabase(databaseName: "test")
				.Options;

			_crashContext = new CrashContext(mockOptions);

			var hubContext = new Mock<ICrashClient>();
			var clientProxy = new Mock<IClientProxy>();
			var mockClientContext = new Mock<HubCallerContext>();

			_crashHub = new CrashHub(_crashContext);

			var mockClients = new Mock<IHubCallerClients<ICrashClient>>();
			var mockClientProxy_All = new Mock<ICrashClient>();
			var mockClientProxy_Others = new Mock<ICrashClient>();

			mockClients.Setup(clients => clients.All).Returns(mockClientProxy_All.Object);
			mockClients.Setup(clients => clients.Others).Returns(mockClientProxy_Others.Object);
			mockClientContext.Setup(c => c.ConnectionId).Returns(Guid.NewGuid().ToString());
			_crashHub.Clients = mockClients.Object;
			_crashHub.Context = mockClientContext.Object;
		}

		public static IEnumerable ValidChanges
		{
			get
			{
				for (var i = 0; i < 100; i++)
				{
					yield return new Change()
					{
						Id = Guid.NewGuid(),
						Owner = Path.GetRandomFileName().Replace(".", ""),
						Payload = "payload",
						Type = nameof(Change),
						Action = ChangeAction.Add | ChangeAction.Temporary,
						Stamp = DateTime.UtcNow
					};
				}
			}
		}

		const string payload = "Payload Example";
		public static IEnumerable RandomChanges
		{
			get
			{
				var random = TestContext.CurrentContext.Random;

				int scenarioCount = 10;
				int randomOwnerCount = 5;
				int changeCount = 100;

				string[] owners = new string[randomOwnerCount];
				for (int i = 0; i < randomOwnerCount; i++)
				{
					owners[i] = Path.GetRandomFileName().Replace(".", "");
				}

				for (int i = 0; i < scenarioCount; i++)
				{
					List<Change> changes = new List<Change>(changeCount);
					for (int j = 0; j < changeCount; j++)
					{
						int ownerIndex = random.Next(0, randomOwnerCount);
						string randomOwner = owners[ownerIndex];

						changes.Add(new Change()
						{
							Id = Guid.NewGuid(),
							Action = getRandomAction(),
							Owner = randomOwner,
							Payload = payload,
							Stamp = DateTime.UtcNow,
							Type = nameof(Change)
						});
					}

					yield return changes;
				}
			}
		}

		private static ChangeAction getRandomAction()
		{
			var random = TestContext.CurrentContext.Random;
			var changes = Enum.GetValues<ChangeAction>();

			ChangeAction action = ChangeAction.None;

			int startIndex = random.Next(0, changes.Length);
			int numOfChanges = random.Next(startIndex, changes.Length);
			for (int i = startIndex; i < numOfChanges; i++)
			{
				action |= changes[i];
			}

			return action;
		}
	}
}
