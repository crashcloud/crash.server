using Crash.Server.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace Crash.Server.Tests
{
	public class MockCrashHub
	{
		public static CrashHub GenerateHub()
		{
			var logger = GetLogger();
			CrashHub hub = new(GetContext(), logger);

			var mockClients = new Mock<IHubCallerClients<ICrashClient>>();
			var mockClientProxy_All = new Mock<ICrashClient>();
			var mockClientProxy_Others = new Mock<ICrashClient>();
			var mockClientContext = new Mock<HubCallerContext>();

			mockClients.Setup(clients => clients.All).Returns(mockClientProxy_All.Object);
			mockClients.Setup(clients => clients.Others).Returns(mockClientProxy_Others.Object);
			mockClientContext.Setup(c => c.ConnectionId).Returns(Guid.NewGuid().ToString());
			hub.Clients = mockClients.Object;
			hub.Context = mockClientContext.Object;

			return hub;
		}

		internal static CrashContext GetContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<CrashContext>();
			optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
			CrashContext context = new(optionsBuilder.Options);
			return context;
		}

		internal static CrashLogger GetLogger()
		{
			return new CrashLogger();
		}

		public sealed class CrashLogger : Logger<CrashHub>
		{
			public CrashLogger() : base(GetFactory())
			{
			}

			private static ILoggerFactory GetFactory()
			{
				return LoggerFactory.Create(builder =>
				{
					// TODO : Fill out
				});
			}
		}

		public sealed class CrashHubCallerClients : IHubCallerClients<ICrashClient>
		{
			private Dictionary<string, ICrashClient> Connected = new();

			public ICrashClient AllExcept(IReadOnlyList<string> excludedConnectionIds)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient Client(string connectionId)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient Clients(IReadOnlyList<string> connectionIds)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient Group(string groupName)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient Groups(IReadOnlyList<string> groupNames)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient User(string userId)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient Users(IReadOnlyList<string> userIds)
			{
				return new EmptyCrashClient();
			}

			public ICrashClient All { get; } = new EmptyCrashClient();

			public ICrashClient OthersInGroup(string groupName)
			{
				return Others;
			}

			public ICrashClient Caller { get; } = new EmptyCrashClient();
			public ICrashClient Others { get; } = new EmptyCrashClient();
		}
	}

	public class EmptyCrashClient : ICrashClient
	{
		public Task Done(string user) { return Task.FromResult(true); }

		public Task DoneRange(IAsyncEnumerable<Guid> ids) { return Task.FromResult(true); }

		public Task PushChange(Change change) { return Task.FromResult(true); }

		public Task PushChangesThroughStream(IAsyncEnumerable<Change> changeStream) { return Task.FromResult(true); }

		public Task<IAsyncEnumerable<Change>> InitializeChangeStream() { return null; }
		
		public Task InitializeChanges(IAsyncEnumerable<Change> changes) { return null; }

		public Task InitializeUsers(IAsyncEnumerable<string> users) { return Task.FromResult(true); }

		public Task UpdateUser(string user) { return Task.FromResult(true); }

		public Task<string> GetMessage() => Task.FromResult<string>(string.Empty);
		
	}
}
