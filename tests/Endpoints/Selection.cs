namespace Crash.Server.Tests.Endpoints
{
	public sealed class Selection : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidChanges))]
		public async Task Select_Success(Change change)
		{
			var currCount = _crashHub.Count;

			await _crashHub.Add(change.Owner, change);
			Assert.That(_crashHub.Count, Is.EqualTo(currCount + 1));

			await _crashHub.Select(change.Owner, change.Id);
			Assert.That(_crashHub.TryGet(change.Id, out var changeOut), Is.True);

			Assert.That(changeOut.Action.HasFlag(ChangeAction.Lock), Is.True);
		}

		[TestCaseSource(nameof(ValidChanges))]
		public async Task UnSelect_Success(Change change)
		{
			var currCount = _crashHub.Count;

			await _crashHub.Add(change.Owner, change);
			Assert.That(_crashHub.Count, Is.EqualTo(currCount + 1));

			await _crashHub.Unselect(change.Owner, change.Id);
			Assert.That(_crashHub.TryGet(change.Id, out var changeOut), Is.True);

			Assert.That(changeOut.Action.HasFlag(ChangeAction.Lock), Is.False);
		}
	}
}
