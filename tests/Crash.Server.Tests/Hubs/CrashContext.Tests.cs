using Crash.Changes.Utils;
using Crash.Server.Hubs;
using Crash.Server.Tests.Utils;

using NUnit.Framework.Legacy;

namespace Crash.Server.Tests.Hubs
{
	[Parallelizable(ParallelScope.None)]
	public class CrashContextTests
	{
		private readonly List<User> _users =
		[
			new User("Lukas"), new User("Morteza"), new User("Curtis")
		];

		private CrashContext context;

		[TearDown]
		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await context.DisposeAsync();
			context?.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
			context = MockCrashHub.GetContext();
		}

		private ImmutableChange GenerateChange()
		{
			var change = new ImmutableChange
			{
				Id = Guid.NewGuid(),
				Action = ChangeAction.Add,
				Owner = "Callum",
				Stamp = DateTime.UtcNow,
				Type = CrashHub.CrashGeometryChange,
				Payload = "Payload"
			};

			return change;
		}

		[Test]
		public async Task AddChangeAsync_Tests()
		{
			var change = GenerateChange();

			var changeCount = context.Changes.Count();
			await context.AddChangeAsync(change);
			Assert.That(context.Changes.Count(), Is.EqualTo(changeCount + 1));
		}

		[Test]
		public async Task GetChanges_TwoInputsCombinedIntoOne()
		{
			var user = Path.GetRandomFileName().Replace(".", "");
			var addPacket = new PayloadPacket() { Data = "Example Payload" };
			var addChange = new ImmutableChange
			{
				Id = Guid.NewGuid(),
				Action = ChangeAction.Add | ChangeAction.Temporary,
				Payload = JsonSerializer.Serialize(addPacket),
				Type = CrashHub.CrashGeometryChange,
				Owner = user
			};
			var releaseChange = new ImmutableChange
			{
				Id = addChange.Id,
				Action = ChangeAction.Release,
				Type = CrashHub.CrashGeometryChange,
				Owner = user
			};

			var changeCount = context.Changes.Count();
			var latestChangeCount = context.LatestChanges.Count();
			await context.AddChangeAsync(addChange);
			await context.AddChangeAsync(releaseChange);

			Assert.That(context.Changes.Count(), Is.EqualTo(changeCount + 2));
			Assert.That(context.LatestChanges.Count(), Is.EqualTo(latestChangeCount + 1));

			var changes = context.GetChanges();
			var combinedChange = changes.Last();

			Assert.That(combinedChange.Action.HasFlag(ChangeAction.Temporary), Is.False);
			Assert.That(combinedChange.Id, Is.EqualTo(addChange.Id));
			Assert.That(combinedChange.Type, Is.EqualTo(addChange.Type));

			Assert.That(PayloadUtils.TryGetPayloadFromChange(addChange, out var addPayload), Is.True);
			Assert.That(PayloadUtils.TryGetPayloadFromChange(combinedChange, out var combinedPayload), Is.True);

			Assert.That(addPayload.Data, Is.EqualTo(combinedPayload.Data));
		}

		[Test]
		public async Task GetUsers_Tests()
		{
			context.Users.RemoveRange(_users);
			await context.Users.AddRangeAsync(_users);
			await context.SaveChangesAsync();

			Assert.That(context.Users.Count(), Is.EqualTo(3));

			var users = context.GetUsers().ToArray();

			Assert.Multiple(() =>
			{
				Assert.That(users, Does.Contain("Lukas"));
				Assert.That(users, Does.Contain("Morteza"));
				Assert.That(users, Does.Contain("Curtis"));
			});
		}

		[Test]
		public async Task DoneAsync_Tests()
		{
			context.RemoveRange(context.GetChanges());
			var doneUser = _users.First();

			for (var i = 0; i < 30; i++)
			{
				var change = new ImmutableChange
				{
					Id = Guid.NewGuid(),
					UniqueId = Guid.NewGuid(),
					Action = ChangeAction.Add | ChangeAction.Temporary,
					Owner = _users[i % 3].Name,
					Stamp = DateTime.UtcNow,
					Type = CrashHub.CrashGeometryChange,
					Payload = "Payload"
				};
				await context.AddChangeAsync(change);
			}

			await context.DoneAsync(doneUser.Name);

			var changes = context.GetChanges();
			var temporary = changes.Where(c => c.Action.HasFlag(ChangeAction.Temporary));
			var released = changes.Where(c => !c.Action.HasFlag(ChangeAction.Temporary));

			Assert.That(temporary.Count(), Is.EqualTo(20));
			Assert.That(released.Count(), Is.EqualTo(10));

			Assert.That(temporary.Select(t => t.Owner != doneUser.Name).Count(), Is.EqualTo(20));
			Assert.That(released.Select(t => t.Owner == doneUser.Name).Count(), Is.EqualTo(10));
		}

	}
}
