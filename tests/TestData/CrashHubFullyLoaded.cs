using Crash.Server.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crash.Server.Tests
{
	public class MockCrashHub
	{
		public static CrashHub GenerateHub()
		{
			CrashHub hub = new(GetContext(), GetLogger());
			hub.Clients = new CrashHubCallerClients();

			return hub;
		}

		private static CrashContext GetContext()
		{
			var optionsBuilder = new DbContextOptionsBuilder<CrashContext>();
			optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
			CrashContext context = new(optionsBuilder.Options);
			return context;
		}

		private static CrashLogger GetLogger()
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
		public Task Done(string user) { return Task.CompletedTask; }

		public Task DoneRange(IEnumerable<Guid> ids) { return Task.CompletedTask; }

		public Task PushIdenticalChanges(IEnumerable<Guid> ids, Change change) { return Task.CompletedTask; }

		public Task PushChange(Change change) { return Task.CompletedTask; }

		public Task PushChanges(IEnumerable<Change> changes) { return Task.CompletedTask; }

		public Task InitializeChanges(IEnumerable<Change> changes) { return Task.CompletedTask; }

		public Task InitializeUsers(IEnumerable<string> users) { return Task.CompletedTask; }

		public Task UpdateUser(string user) { return Task.CompletedTask; }
	}
}
