using System.Collections;

using Crash.Changes.Extensions;

namespace Crash.Server.Tests.Endpoints
{

	public sealed class Done : CrashHubEndpoints
	{

		[Test]
		public async Task Done_Failures()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () => await _crashHub.Done(null));
			Assert.ThrowsAsync<ArgumentNullException>(async () => await _crashHub.Done(string.Empty));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Success(IEnumerable<Change> changes)
		{
			int currCount = _crashHub.Count;
			HashSet<string> owners = changes.Select(c => c.Owner).ToHashSet();

			foreach(var change in changes)
			{
				await _crashHub.Add(change.Owner, change);
			}

			Assert.That(_crashHub.Count, Is.EqualTo(currCount + changes.Count()));
			Assert.That(_crashHub.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.True);

			foreach (string owner in owners)
			{
				await _crashHub.Done(owner);
				
				foreach(Change change in _crashHub.GetChanges())
				{
					if (change.Owner.Equals(owner))
					{
						Assert.That(change.Action.HasFlag(ChangeAction.Temporary), Is.False);
					}
				}
			}

			Assert.That(_crashHub.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.False);
		}

	}
}
