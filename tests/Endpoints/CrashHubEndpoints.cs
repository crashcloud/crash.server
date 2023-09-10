using Crash.Server.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Crash.Server.Tests.Endpoints
{
	public abstract class CrashHubEndpoints
	{
		private const string payload = "Payload Example";
		protected CrashContext? _crashContext;
		internal CrashHub? _crashHub;

		public static IEnumerable ValidAddChanges
		{
			get
			{
				for (var i = 0; i < 5; i++)
				{
					yield return new Change
					{
						Id = Guid.NewGuid(),
						Owner = Path.GetRandomFileName().Replace(".", ""),
						Type = CrashHub.CrashGeometryChange,
						Action = ChangeAction.Add | ChangeAction.Temporary,
						Stamp = DateTime.UtcNow,
						Payload = JsonSerializer.Serialize(new PayloadPacket())
					};
				}
			}
		}

		public static IEnumerable InvalidAddChanges
		{
			get
			{
				var validPayload = JsonSerializer.Serialize(new PayloadPacket());
				var invalidPayload = string.Empty;

				yield return new Change
				{
					Id = Guid.Empty,
					Owner = Path.GetRandomFileName().Replace(".", ""),
					Type = CrashHub.CrashGeometryChange,
					Action = ChangeAction.Add | ChangeAction.Temporary,
					Stamp = DateTime.UtcNow,
					Payload = invalidPayload
				};

				yield return new Change
				{
					Id = Guid.Empty,
					Owner = Path.GetRandomFileName().Replace(".", ""),
					Type = CrashHub.CrashGeometryChange,
					Action = ChangeAction.Add | ChangeAction.Temporary,
					Stamp = DateTime.UtcNow,
					Payload = validPayload
				};

				yield return new Change
				{
					Id = new Guid(),
					Owner = Path.GetRandomFileName().Replace(".", ""),
					Type = CrashHub.CrashGeometryChange,
					Action = ChangeAction.Add | ChangeAction.Temporary,
					Stamp = DateTime.UtcNow,
					Payload = validPayload
				};

				yield return new Change
				{
					Id = new Guid(),
					Owner = Path.GetRandomFileName().Replace(".", ""),
					Type = CrashHub.CrashGeometryChange,
					Action = ChangeAction.Remove,
					Stamp = DateTime.UtcNow,
					Payload = validPayload
				};

				yield return new Change
				{
					Id = new Guid(),
					Owner = Path.GetRandomFileName().Replace(".", ""),
					Type = CrashHub.CrashGeometryChange,
					Action = ChangeAction.Remove | ChangeAction.Locked,
					Stamp = DateTime.UtcNow,
					Payload = validPayload
				};
			}
		}

		public static IEnumerable RandomChanges
		{
			get
			{
				var random = TestContext.CurrentContext.Random;

				var scenarioCount = 10;
				var randomOwnerCount = 5;
				var changeCount = 100;

				var owners = new string[randomOwnerCount];
				for (var i = 0; i < randomOwnerCount; i++)
				{
					owners[i] = Path.GetRandomFileName().Replace(".", "");
				}

				for (var i = 0; i < scenarioCount; i++)
				{
					var changes = new List<Change>(changeCount);
					for (var j = 0; j < changeCount; j++)
					{
						var ownerIndex = random.Next(0, randomOwnerCount);
						var randomOwner = owners[ownerIndex];

						changes.Add(new Change
						{
							Id = Guid.NewGuid(),
							Action = ChangeAction.Add | getRandomAction(),
							Owner = randomOwner,
							Payload = payload,
							Stamp = DateTime.UtcNow,
							Type = CrashHub.CrashGeometryChange
						});
					}

					yield return changes;
				}
			}
		}

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

		internal static DbContextOptions<CrashContext> GetMockOptions()
		{
			var mockOptions = new DbContextOptionsBuilder<CrashContext>()
				.UseInMemoryDatabase("test")
				.Options;

			return mockOptions;
		}

		private void SetUpContext()
		{
			// Create a mock DbContextOptions object
			var mockOptions = GetMockOptions();

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

		private static ChangeAction getRandomAction()
		{
			var random = TestContext.CurrentContext.Random;
			var changes = Enum.GetValues<ChangeAction>();

			var action = ChangeAction.None;

			var startIndex = random.Next(0, changes.Length);
			var numOfChanges = random.Next(startIndex, changes.Length);
			for (var i = startIndex; i < numOfChanges; i++)
			{
				action |= changes[i];
			}

			return action;
		}
	}
}
