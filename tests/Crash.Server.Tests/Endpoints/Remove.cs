using System.IO;

using Crash.Server.Hubs;

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Remove : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Successful(Change change)
		{
			var currCount = CrashHub.Database.Changes.Count();

			await CrashHub.PushChange(change);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			var deleteChange = new Change(change)
			{
				Action = ChangeAction.Remove,
				Id = change.Id,
				Type = CrashHub.CrashGeometryChange
			};
			await CrashHub.PushChange(deleteChange);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount + 2));
			Assert.That(CrashHub.Database.TryGetChange(change.Id, out var latestChange), Is.True);

			Assert.That(latestChange, Is.Not.Null);
			Assert.That(latestChange.Id, Is.EqualTo(latestChange.Id));
			Assert.That(latestChange.Owner, Is.EqualTo(latestChange.Owner));
			Assert.That(latestChange.Type, Is.EqualTo(latestChange.Type));
		}

		[Test]
		public async Task Delete_Failure_MissingId()
		{
			var currCount = CrashHub.Database.Changes.Count();
			var invalidIdDeleteChange = new ImmutableChange
			{
				Id = Guid.Empty,
				Action = ChangeAction.Remove,
				Type = CrashHub.CrashGeometryChange
			};
			await CrashHub.Database.AddChangeAsync(invalidIdDeleteChange);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[Test]
		public async Task Delete_Failure_MissingType()
		{
			var currCount = CrashHub.Database.Changes.Count();
			var invalidIdDeleteChange = new Change { Id = Guid.NewGuid(), Action = ChangeAction.Remove, Type = null };

			await CrashHub.PushChange(invalidIdDeleteChange);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Failure(Change change)
		{
			var currCount = CrashHub.Database.Changes.Count();

			await CrashHub.PushChange(new Change
			{
				Id = Guid.Empty,
				Action = ChangeAction.Remove,
				Type = CrashHub.CrashGeometryChange
			});

			for (var i = 0; i < 5; i++)
			{
				var user = Path.GetRandomFileName().Replace(".", "");
				var guid = Guid.NewGuid();
				await CrashHub.PushChange(new Change
				{
					Id = guid,
					Action = ChangeAction.Remove,
					Type = CrashHub.CrashGeometryChange
				});
				Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
			}

			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Failure_NotInDatabase(Change change)
		{
			var currCount = CrashHub.Database.Changes.Count();

			await CrashHub.PushChange(new Change
			{
				Action = ChangeAction.Remove,
				Id = change.Id,
				Type = CrashHub.CrashGeometryChange
			});

			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}
	}
}
