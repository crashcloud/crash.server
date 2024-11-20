using Crash.Server.Hubs;

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Done : CrashHubEndpoints
	{
		private static IEnumerable BadUserNames
		{
			get
			{
				yield return null;
				yield return string.Empty;
			}
		}

		[TestCaseSource(nameof(BadUserNames))]
		public async Task Done_WithUser_InvalidUser(string userName)
		{
			var doneChange = new Change
			{
				Type = CrashHub.CrashDoneChange,
				Action = ChangeAction.Release,
				Owner = userName
			};

			var currentChangesCount = CrashHub.Database.Changes.Count();
			var latestChangesCount = CrashHub.Database.Changes.Count();

			await CrashHub.PushChange(doneChange);

			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currentChangesCount));
			Assert.That(CrashHub.Database.LatestChanges.Count(), Is.EqualTo(latestChangesCount));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Failures(IEnumerable<Change> changes)
		{
			var currCount = CrashHub.Database.Changes.Count();

			foreach (var change in changes)
			{
				await CrashHub.PushChange(change);
			}

			Assert.That(CrashHub.Database.Changes.Count(), Is.Not.EqualTo(0));

			var tempCount = CrashHub.Database.GetChanges().Select(c => c.HasFlag(ChangeAction.Temporary)).Count();
			Assert.That(tempCount, Is.GreaterThan(0));

			await CrashHub.PushChange(new Change
			{
				Owner = string.Empty,
				Action = ChangeAction.Release,
				Type = CrashHub.CrashGeometryChange
			});

			Assert.That(tempCount, Is.EqualTo(tempCount));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Success(IEnumerable<Change> changes)
		{
			var currCount = CrashHub.Database.Changes.Count();
			HashSet<string> owners = changes.Select(c => c.Owner).ToHashSet();

			await CrashHub.PushChangesThroughStream(changes.ToAsyncEnumerable());

			var changesCount = CrashHub.Database.Changes.Count();
			var inputChangesCount = changes.Count();

			Assert.That(changesCount, Is.Not.EqualTo(0));
			Assert.That(CrashHub.Database.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.True);

			foreach (var owner in owners)
			{
				var doneChange = new Change
				{
					Owner = owner,
					Type = CrashHub.CrashDoneChange,
					Action = ChangeAction.Release
				};
				await CrashHub.PushChange(doneChange);

				foreach (var change in CrashHub.Database.GetChanges())
				{
					if (change.Owner.Equals(owner))
					{
						Assert.That(change.Action.HasFlag(ChangeAction.Temporary), Is.False);
					}
				}
			}

			var latestChanges = CrashHub.Database.GetChanges();
			Assert.That(latestChanges.Any(c => c.HasFlag(ChangeAction.Temporary)), Is.False);
		}
	}
}
