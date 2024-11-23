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

			Assert.That(await CrashHub.PushChange(change), Is.True);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			var deleteChange = new Change(change)
			{
				Action = ChangeAction.Remove,
				Id = change.Id,
				Type = CrashHub.CrashGeometryChange,
				Owner = Path.GetRandomFileName().Replace(".", ""),
			};
			Assert.That(await CrashHub.PushChange(deleteChange), Is.True);
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
				Type = CrashHub.CrashGeometryChange,
				Owner = Path.GetRandomFileName().Replace(".", ""),
			};
			Assert.That(await CrashHub.Database.AddChangeAsync(invalidIdDeleteChange), Is.False);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[Test]
		public async Task Delete_Failure_MissingType()
		{
			var currCount = CrashHub.Database.Changes.Count();
			var invalidIdDeleteChange = new Change { Id = Guid.NewGuid(), Action = ChangeAction.Remove, Type = null };

			Assert.That(await CrashHub.PushChange(invalidIdDeleteChange), Is.False);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Failure(Change change)
		{
			var currCount = CrashHub.Database.Changes.Count();

			Assert.That(await CrashHub.PushChange(new Change
			{
				Id = Guid.Empty,
				Action = ChangeAction.Remove,
				Type = CrashHub.CrashGeometryChange,
				Owner = Path.GetRandomFileName().Replace(".", ""),
			}), Is.False);

			for (var i = 0; i < 5; i++)
			{
				var user = Path.GetRandomFileName().Replace(".", "");
				var guid = Guid.NewGuid();
				Assert.That(await CrashHub.PushChange(new Change
				{
					Id = guid,
					Action = ChangeAction.Remove,
					Type = CrashHub.CrashGeometryChange
				}), Is.False);
				Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
			}

			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Failure_NotInDatabase(Change change)
		{
			var currCount = CrashHub.Database.Changes.Count();

			Assert.That(await CrashHub.PushChange(new Change
			{
				Action = ChangeAction.Remove,
				Id = change.Id,
				Type = CrashHub.CrashGeometryChange
			}), Is.False);

			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}
	}
}
