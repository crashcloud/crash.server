// ReSharper disable HeapView.BoxingAllocation

using Crash.Changes.Utils;
using Crash.Server.Hubs;

namespace Crash.Server.Tests.Model
{
	[Parallelizable(ParallelScope.All)]
	public class ChangeFactoryTests
	{
		private ImmutableChange GetAddChange()
		{
			return new ImmutableChange
			{
				Id = Guid.NewGuid(),
				Action = ChangeAction.Add | ChangeAction.Temporary,
				Payload = "Example Payload",
				Type = CrashHub.CrashGeometryChange
			};
		}

		[Test]
		public void CombineRecords_AddOnly()
		{
			var addChange = GetAddChange();
			var releaseChange = new ImmutableChange
			{
				Id = addChange.Id, Action = ChangeAction.Release, Type = CrashHub.CrashGeometryChange
			};

			// Combining but NOT adding Actions!
			var combinedChange = ChangeFactory.CombineRecords(addChange, releaseChange);

			AssertCombinedChangeIsValid(addChange, releaseChange, combinedChange);
		}

		private void AssertCombinedChangeIsValid(ImmutableChange addChange, ImmutableChange releaseChange,
			MutableChange combinedChange)
		{
			Assert.Multiple(() =>
			{
				Assert.That(combinedChange.Id, Is.EqualTo(addChange.Id));
				Assert.That(combinedChange.Type, Is.EqualTo(addChange.Type));
				Assert.That(combinedChange.Owner, Is.EqualTo(addChange.Owner));

				Assert.That(PayloadUtils.TryGetPayloadFromChange(addChange, out var addPayload), Is.True);
				Assert.That(PayloadUtils.TryGetPayloadFromChange(combinedChange, out var combinedPayload), Is.True);

				Assert.That(addPayload.Data, Is.EqualTo(combinedPayload.Data));
			});
		}

		[Test]
		public void CombineRecords_BadInputs()
		{
			var duplicateChange = new Change { Id = Guid.NewGuid() };
			var badChange = new Change { Id = Guid.Empty };

			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(null, badChange));
			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(badChange, null));
			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(null, null));
			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(duplicateChange, duplicateChange));
			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(badChange, badChange));
			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(duplicateChange, badChange));
			Assert.Throws<ArgumentException>(() => ChangeFactory.CombineRecords(badChange, duplicateChange));
		}

		[Test]
		public void CombineRecords_TransformOnly()
		{
			var transformChange = new ImmutableChange
			{
				Id = Guid.NewGuid(),
				Action = ChangeAction.Transform | ChangeAction.Temporary,
				Payload = JsonSerializer.Serialize(new CTransform(100, 200, 300)),
				Type = CrashHub.CrashGeometryChange
			};
			var releaseChange = new ImmutableChange
			{
				Id = transformChange.Id, Action = ChangeAction.Release, Type = CrashHub.CrashGeometryChange
			};

			var combinedChange = ChangeFactory.CombineRecords(transformChange, releaseChange);

			AssertCombinedChangeIsValid(transformChange, releaseChange, combinedChange);
		}

		[Test]
		public void CombineRecords_UpdateOnly()
		{
			var updateChange = new ImmutableChange
			{
				Id = Guid.NewGuid(),
				Action = ChangeAction.Update | ChangeAction.Temporary,
				Payload = JsonSerializer.Serialize(new Dictionary<string, string> { { "Key", "Value" } }),
				Type = CrashHub.CrashGeometryChange
			};
			var releaseChange = new ImmutableChange
			{
				Id = updateChange.Id, Action = ChangeAction.Release, Type = CrashHub.CrashGeometryChange
			};

			var combinedChange = ChangeFactory.CombineRecords(updateChange, releaseChange);

			AssertCombinedChangeIsValid(updateChange, releaseChange, combinedChange);
		}

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

			Assert.That(change.HasFlag(ChangeAction.Unlocked), Is.False);
			Assert.That(change.HasFlag(ChangeAction.Locked), Is.True);
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

			Assert.That(change.HasFlag(ChangeAction.Unlocked), Is.True);
			Assert.That(change.HasFlag(ChangeAction.Locked), Is.False);
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
				         ChangeAction.Locked,
				Payload = "{}",
				Id = id,
				Type = "Camera"
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
			var hasLockAndUnlock = change.Action.HasFlag(ChangeAction.Locked | ChangeAction.Unlocked);

			Assert.False(hasAddAndDelete);
			Assert.False(hasLockAndUnlock);
		}
	}
}
