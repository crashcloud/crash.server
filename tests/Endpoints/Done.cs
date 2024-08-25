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

			var currentChangesCount = _crashHub.Database.Changes.Count();
			var latestChangesCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(doneChange);

			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currentChangesCount));
			Assert.That(_crashHub.Database.LatestChanges.Count(), Is.EqualTo(latestChangesCount));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Failures(IEnumerable<Change> changes)
		{
			var currCount = _crashHub.Database.Changes.Count();

			foreach (var change in changes)
			{
				await _crashHub.PushChange(change);
			}

			Assert.That(_crashHub.Database.Changes.Count(), Is.Not.EqualTo(0));

			var tempCount = _crashHub.Database.GetChanges().Select(c => c.HasFlag(ChangeAction.Temporary)).Count();
			Assert.That(tempCount, Is.GreaterThan(0));

			await _crashHub.PushChange(new Change
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
			var currCount = _crashHub.Database.Changes.Count();
			HashSet<string> owners = changes.Select(c => c.Owner).ToHashSet();

			await _crashHub.PushChangesThroughStream(changes.ToAsyncEnumerable());

			var changesCount = _crashHub.Database.Changes.Count();
			var inputChangesCount = changes.Count();

			Assert.That(changesCount, Is.Not.EqualTo(0));
			Assert.That(_crashHub.Database.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.True);

			foreach (var owner in owners)
			{
				var doneChange = new Change
				{
					Owner = owner,
					Type = CrashHub.CrashDoneChange,
					Action = ChangeAction.Release
				};
				await _crashHub.PushChange(doneChange);

				foreach (var change in _crashHub.Database.GetChanges())
				{
					if (change.Owner.Equals(owner))
					{
						Assert.That(change.Action.HasFlag(ChangeAction.Temporary), Is.False);
					}
				}
			}

			var latestChanges = _crashHub.Database.GetChanges();
			Assert.That(latestChanges.Any(c => c.HasFlag(ChangeAction.Temporary)), Is.False);
		}
	}
}
