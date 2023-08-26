// ReSharper disable HeapView.BoxingAllocation

namespace Crash.Server.Tests.Model
{
	public class ChangeFactoryTests
	{
		[Test]
		public void Create_Delete_Success()
		{
			// Act
			var change = ChangeFactory.CreateDeleteRecord(Guid.NewGuid());

			// Assert
			ValidateIds(change);
			ValidateActions(change);

			Assert.That(change.HasFlag(ChangeAction.Add), Is.False);
			Assert.That(change.HasFlag(ChangeAction.Remove), Is.True);
			Assert.That(change.Action, Is.Not.EqualTo(ChangeAction.None));
		}

		[Test]
		public void Create_Lock_Success()
		{
			// Act
			var change = ChangeFactory.CreateLockRecord(nameof(ChangeFactoryTests), Guid.NewGuid());

			// Assert
			ValidateIds(change);
			ValidateActions(change);

			Assert.That(change.HasFlag(ChangeAction.Unlock), Is.False);
			Assert.That(change.HasFlag(ChangeAction.Lock), Is.True);
			Assert.That(change.Action, Is.Not.EqualTo(ChangeAction.None));
		}

		[Test]
		public void Create_Unlock_Success()
		{
			// Act
			var change = ChangeFactory.CreateUnlockRecord(nameof(ChangeFactoryTests), Guid.NewGuid());

			// Assert
			ValidateIds(change);
			ValidateActions(change);

			Assert.That(change.HasFlag(ChangeAction.Unlock), Is.True);
			Assert.That(change.HasFlag(ChangeAction.Lock), Is.False);
			Assert.That(change.Action, Is.Not.EqualTo(ChangeAction.None));
		}

		[Test]
		public void Create_DoneRecord_Success()
		{
			// Act
			var change = ChangeFactory.CreateDoneRecord(nameof(ChangeFactoryTests), "Done", Guid.NewGuid());

			// Assert
			ValidateIds(change);
			ValidateActions(change);
		}

		[Test]
		public void Create_MergedDoneRecord_Success()
		{
			var id = Guid.NewGuid();
			ImmutableChange latestChange = new()
			{
				Action = ChangeAction.Add |
				         ChangeAction.Temporary |
				         ChangeAction.Update |
				         ChangeAction.Transform |
				         ChangeAction.Lock,
				Payload = "{}",
				Id = id,
				Type = nameof(ChangeAction.Camera)
			};
			// Arrange

			// Act
			var change = ChangeFactory.CreateDoneRecord(nameof(ChangeFactoryTests), "Done", id);

			// Assert
			ValidateIds(change);
			ValidateActions(change);
		}

		private void ValidateIds(ImmutableChange change)
		{
			// Id Validity
			Assert.That(change.UniqueId, Is.Not.EqualTo(Guid.Empty));
			Assert.That(change.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(change.Id, Is.Not.EqualTo(change.UniqueId));
		}

		private void ValidateActions(ImmutableChange change)
		{
			// DateTime
			Assert.That(change.Stamp, Is.Not.EqualTo(DateTime.MinValue));
			Assert.That(change.Stamp, Is.Not.EqualTo(DateTime.MaxValue));

			var hasAddAndDelete = change.Action.HasFlag(ChangeAction.Add | ChangeAction.Remove);
			var hasLockAndUnlock = change.Action.HasFlag(ChangeAction.Lock | ChangeAction.Unlock);

			Assert.False(hasAddAndDelete);
			Assert.False(hasLockAndUnlock);
		}
	}
}
