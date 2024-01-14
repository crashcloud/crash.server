// ReSharper disable HeapView.BoxingAllocation

using Crash.Server.Tests.Utils;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Crash.Server.Tests.Endpoints
{
	public sealed class ReleaseTests : CrashHubEndpoints
	{

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Release_Successful(Change change)
		{
			Assert.Ignore("Strings are encoded wrongly");
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));
			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var changeOut), Is.True);
			Assert.That(EqualityUtils.CompareChanges(change, changeOut), Is.True);
		}
	}
}
