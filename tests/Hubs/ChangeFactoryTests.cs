using Crash.Changes.Extensions;
using Crash.Server.Model;

using NUnit.Framework;

namespace Crash.Server.Tests.Hubs
{
	public class ChangeFactoryTests
	{

		[NUnit.Framework.Test]
		public void CreateDelete()
		{
			// Act & Assert
			var change = ChangeFactory.CreateDeleteRecord(Guid.NewGuid());
			
			// General Validity
			ValidateIds(change);
			ValidateActions(change);

			Assert.That(change.HasFlag(ChangeAction.Add), Is.False);
			Assert.That(change.HasFlag(ChangeAction.Remove), Is.True);
			Assert.That(change.Action, Is.Not.EqualTo(ChangeAction.None));
		}

		[NUnit.Framework.Test]
		public void CreateLock()
		{
			// Act & Assert
			var change = ChangeFactory.CreateLockRecord(nameof(ChangeFactoryTests), Guid.NewGuid());
			
			// General Validity
			ValidateIds(change);
			ValidateActions(change);

			Assert.That(change.HasFlag(ChangeAction.Unlock), Is.False);
			Assert.That(change.HasFlag(ChangeAction.Lock), Is.True);
			Assert.That(change.Action, Is.Not.EqualTo(ChangeAction.None));
		}

		[NUnit.Framework.Test]
		public void CreateUnlock()
		{
			// Act & Assert
			var change = ChangeFactory.CreateUnlockRecord(nameof(ChangeFactoryTests), Guid.NewGuid());
			
			// General Validity
			ValidateIds(change);
			ValidateActions(change);

			Assert.That(change.HasFlag(ChangeAction.Unlock), Is.True);
			Assert.That(change.HasFlag(ChangeAction.Lock), Is.False);
			Assert.That(change.Action, Is.Not.EqualTo(ChangeAction.None));
		}

		[NUnit.Framework.Test]
		public void CreateDoneRecord()
		{
			// Act & Assert
			var change = ChangeFactory.CreateDoneRecord(nameof(ChangeFactoryTests), "Done", Guid.NewGuid());
			
			// General Validity
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
