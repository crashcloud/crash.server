using Crash.Server.Hubs;

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Remove : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Successful(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			var deleteChange = new Change(change)
			{
				Action = ChangeAction.Remove, Id = change.Id, Type = CrashHub.CrashGeometryChange
			};
			await _crashHub.PushChange(deleteChange);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 2));
			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var latestChange), Is.True);

			Assert.That(latestChange, Is.Not.Null);
			Assert.That(latestChange.Id, Is.EqualTo(latestChange.Id));
			Assert.That(latestChange.Owner, Is.EqualTo(latestChange.Owner));
			Assert.That(latestChange.Type, Is.EqualTo(latestChange.Type));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Failure(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(new Change { Id = Guid.Empty });

			for (var i = 0; i < 10; i++)
			{
				var user = Path.GetRandomFileName().Replace(".", "");
				var guid = Guid.NewGuid();
				await _crashHub.PushChange(new Change { Id = guid });
				Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount));
			}

			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}
	}
}
