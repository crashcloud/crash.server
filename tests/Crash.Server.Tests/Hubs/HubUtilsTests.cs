// ReSharper disable HeapView.BoxingAllocation

using Crash.Server.Hubs;

namespace Crash.Server.Tests.Hubs
{
	[Parallelizable]
	public class HubUtilsTests
	{
		[TestCaseSource(typeof(TestData), nameof(TestData.ValidUserNames))]
		public void IsUserValid_ValidUsers(string userName)
		{
			Assert.That(ChangeValidation.IsUserValid(userName), Is.True);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidUserNames))]
		public void IsUserValid_InvalidUsers(string? userName)
		{
			Assert.That(ChangeValidation.IsUserValid(userName), Is.False);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.ValidFirstChanges))]
		public void IsChangeValid_ValidChanges(IChange change)
		{
			Assert.That(ChangeValidation.IsChangeValid(change), Is.True);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidChanges))]
		public void IsChangeValid_InvalidChanges(IChange change)
		{
			Assert.That(ChangeValidation.IsChangeValid(change), Is.False);
		}

		[Test]
		public void IsGuidValid()
		{
			Assert.That(ChangeValidation.IsGuidValid(Guid.NewGuid()), Is.True);
			Assert.That(ChangeValidation.IsGuidValid(Guid.Empty), Is.False);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.ValidPayloads))]
		public void IsPayloadValid_ValidPayloads(string payload)
		{
			Change change = new() { Payload = payload };
			Assert.That(ChangeValidation.IsPayloadValid(change), Is.True);
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidPayloads))]
		public void IsPayloadValid_InvalidPayloads(string? payload)
		{
			Change change = new() { Payload = payload };
			Assert.That(ChangeValidation.IsPayloadValid(change), Is.False);
		}
	}
}
