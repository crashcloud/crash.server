// ReSharper disable HeapView.BoxingAllocation

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Add : CrashHubEndpoints
	{
		private static bool EqualChanges(IChange left, IChange right)
		{
			if (left.Id != right.Id)
			{
				return false;
			}

			if (left.Owner != right.Owner)
			{
				return false;
			}

			if (left.Action != right.Action)
			{
				return false;
			}

			if (left.Payload != right.Payload)
			{
				return false;
			}

			if (left.Stamp != right.Stamp)
			{
				return false;
			}

			if (left.Type != right.Type)
			{
				return false;
			}

			return true;
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Add_Successful(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));
			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var changeOut), Is.True);
			Assert.That(EqualChanges(change, changeOut), Is.True);
		}

		[TestCaseSource(nameof(InvalidAddChanges))]
		public async Task Add_Failure(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount));
			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var changeOut), Is.False);
			Assert.That(changeOut, Is.Null);
		}
	}
}
