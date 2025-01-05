// ReSharper disable HeapView.BoxingAllocation

using Crash.Server.Hubs;

namespace Crash.Server.Tests.Hubs
{
	[Parallelizable]
	public class HubUtilsTests
	{

		private CrashHub Hub { get; } = MockCrashHub.GenerateHub();

		[TestCaseSource(typeof(TestData), nameof(TestData.ValidUserNames))]
		public void IsUserValid_ValidUsers(string userName)
		{
			Assert.That(Hub.IsUserValid(new Change() { Owner = userName }), Is.True);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidUserNames))]
		public void IsUserValid_InvalidUsers(string userName)
		{
			Assert.That(Hub.IsUserValid(new Change() { Owner = userName }), Is.False);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.ValidFirstChanges))]
		public void IsChangeValid_ValidChanges(IChange change)
		{
			Assert.That(Hub.IsChangeValid(change), Is.True);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidChanges))]
		public void IsChangeValid_InvalidChanges(IChange change)
		{
			Assert.That(Hub.IsChangeValid(change), Is.False);
		}

		[Test]
		public void IsGuidValid()
		{
			Assert.That(Hub.IsGuidValid(Guid.NewGuid()), Is.True);
			Assert.That(Hub.IsGuidValid(Guid.Empty), Is.False);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.ValidPayloads))]
		public void IsPayloadValid_ValidPayloads(string payload)
		{
			Change change = new() { Payload = payload };
			Assert.That(Hub.IsPayloadEmpty(change), Is.False);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidPayloads))]
		public void IsPayloadValid_InvalidPayloads(string payload)
		{
			Change change = new() { Payload = payload };
			Assert.That(Hub.IsPayloadEmpty(change), Is.True);
		}
	}
}
