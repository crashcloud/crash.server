using Crash.Server.Hubs;

namespace Crash.Server.Tests.Hubs
{
	[Parallelizable(ParallelScope.None)]
	public sealed class CrashHubTests
	{
		public CrashHub Hub { get; private set; }

		[SetUp]
		public void SetUp()
		{
			Hub = MockCrashHub.GenerateHub();
		}


		[TearDown]
		public void TearDown()
		{
			Hub?.Dispose();
			Hub = null;
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.ValidFirstChanges))]
		public async Task PushSingleValidChange(Change change)
		{
			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});

			await Hub.PushChange(change);

			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes.Count(), Is.GreaterThanOrEqualTo(1));
				Assert.That(Hub.Database.LatestChanges.Count(), Is.EqualTo(1));
				Assert.That(Hub.Database.Users.Count(), Is.EqualTo(1));
			});
		}

		[TestCaseSource(typeof(TestData), nameof(TestData.InvalidChanges))]
		public async Task PushSingleInvalidChange(Change invalidChange)
		{
			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});

			try
			{
				await Hub.PushChange(invalidChange);
				Assert.Multiple(() =>
				{
					Assert.That(Hub.Database.Changes, Is.Empty);
					Assert.That(Hub.Database.LatestChanges, Is.Empty);
					Assert.That(Hub.Database.Users, Is.Empty);
				});
			}
			catch (Exception e)
			{
				Assert.That(e.Message, Is.Not.Null.Or.Empty);
			}
		}

		[Test]
		public async Task PushChanges_NullOrEmptyData_ThrowsNoExceptions()
		{
			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});

			await Hub.PushChange(null);

			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});
		}

		[Test]
		public async Task PushChange_NullData_ThrowsNoExceptions()
		{
			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});

			await Hub.PushChange(null);

			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});
		}

		[Test]
		public async Task PushChange_InvalidData_ThrowsNoExceptions()
		{
			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});

			await Hub.PushChange(null);

			Assert.Multiple(() =>
			{
				Assert.That(Hub.Database.Changes, Is.Empty);
				Assert.That(Hub.Database.LatestChanges, Is.Empty);
				Assert.That(Hub.Database.Users, Is.Empty);
			});
		}
	}
}
