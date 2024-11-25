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
			var currCount = CrashHub.Database.Changes.Count();

			await CrashHub.PushChange(change);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));
			Assert.That(CrashHub.Database.TryGetChange(change.Id, out var changeOut), Is.True);
			Assert.That(EqualityUtils.CompareChanges(change, changeOut), Is.True);
		}
	}
}
