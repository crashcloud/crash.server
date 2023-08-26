// ReSharper disable HeapView.BoxingAllocation

using Crash.Server.Tests.Endpoints;

namespace Crash.Server.Tests.Immutability
{
	public sealed class HistoryTests : CrashHubEndpoints
	{
		private static IEnumerable NonConflictingChangeStacks
		{
			get
			{
				yield return initialOnly();
				yield return addRemoveStack();
				yield return generateLifetimeStack();
			}
		}

		[Test]
		public void nullChage()
		{
			// _crashHub.Add
		}

		[TestCaseSource(nameof(NonConflictingChangeStacks))]
		public async Task TestContext(ValueTuple<Stack<Change>, Change> changeTuple)
		{
			var changeHistory = changeTuple.Item1;
			var latestChange = changeTuple.Item2;

			var options = GetMockOptions();
			CrashContext context = new(options);

			Task.WaitAll(changeHistory.Select(
				c => context.AddChangeAsync(new ImmutableChange
					{
						UniqueId = Guid.NewGuid(),
						Id = c.Id,
						Owner = c.Owner,
						Action = c.Action,
						Payload = c.Payload,
						Stamp = c.Stamp,
						Type = c.Type
					}
				)).ToArray());

			Assert.Multiple(() =>
			{
				// Assert court?

				Assert.That(context.TryGetChange(latestChange.Id, out var latestFound), Is.True);

				Assert.That(latestFound.Stamp, Is.GreaterThan(DateTime.MinValue));
				Assert.That(latestFound.Stamp, Is.LessThan(DateTime.MaxValue));
				Assert.That(latestFound.Stamp,
					Is.GreaterThanOrEqualTo(latestChange.Stamp)); // May be unecessary to verify?

				// Guid must not be null
				Assert.That(Guid.Empty, Is.Not.EqualTo(latestFound.Id));
				Assert.That(latestFound.Id, Is.EqualTo(latestChange.Id));
				Assert.That(latestFound.Owner, Is.EqualTo(latestChange.Owner));

				// Payload Can be null or not null?
				// It'd be more efficient if null
				Assert.That(latestFound.Payload,
					Is.EqualTo(latestChange.Payload)); // This should be a combination of geom and transform

				// Type must never be null
				Assert.That(string.IsNullOrEmpty(latestFound.Type), Is.False);
				Assert.That(latestFound.Type, Is.EqualTo(latestChange.Type));

				// Action must never be None
				Assert.That(latestFound.Action, Is.Not.EqualTo(ChangeAction.None));
				Assert.That(latestFound.Action, Is.EqualTo(latestChange.Action));
			});
		}

		private static (Stack<Change>, Change) initialOnly()
		{
			Stack<Change> firstHistory = new();
			var initialChange = getNewChange(Guid.NewGuid(), ChangeAction.Add, "{ \"example\": \"payload\" }");
			firstHistory.Push(initialChange);

			return new ValueTuple<Stack<Change>, Change>(firstHistory, initialChange);
		}

		private static (Stack<Change>, Change) addRemoveStack()
		{
			Stack<Change> firstHistory = new();
			var stackId = Guid.NewGuid();

			var payload = "{ \"example\": \"payload\" }";
			var add = getNewChange(stackId, ChangeAction.Add, payload);
			var remove = getNewChange(stackId, ChangeAction.Remove, payload);

			firstHistory.Push(add);
			firstHistory.Push(remove);

			return new ValueTuple<Stack<Change>, Change>(firstHistory, remove);
		}

		private static (Stack<Change>, Change) generateLifetimeStack()
		{
			Stack<Change> firstHistory = new();
			var stackId = Guid.NewGuid();
			var first = getNewChange(stackId, ChangeAction.Add, "{ \"example\": \"payload\" }");
			firstHistory.Push(first);
			firstHistory.Push(getNewChange(stackId, ChangeAction.Lock, null));

			var transform = new CTransform(1, 2, 3, 4);
			var json = JsonSerializer.Serialize(transform);

			firstHistory.Push(getNewChange(stackId, ChangeAction.Transform, json));
			firstHistory.Push(getNewChange(stackId, ChangeAction.Temporary, null));
			firstHistory.Push(getNewChange(stackId, ChangeAction.Unlock, null));
			firstHistory.Push(getNewChange(stackId, ChangeAction.Remove, null));
			var last = getNewChange(stackId, ChangeAction.Add, null);
			firstHistory.Push(last);
			// firstHistory.Push(getNewChange(stackId, ChangeAction.Update, null)); // Undefined

			var liveChange = new Change
			{
				Id = stackId,
				Action = ChangeAction.Add | ChangeAction.Transform,
				Owner = first.Owner,
				Payload = first.Payload,
				Stamp = last.Stamp,
				Type = first.Type
			};

			return new ValueTuple<Stack<Change>, Change>(firstHistory, liveChange);
		}

		private static Change getNewChange(Guid id, ChangeAction action, string? payload)
		{
			return new Change()
			{
				Id = id,
				Action = action,
				Owner = nameof(HistoryTests),
				Payload = payload,
				Stamp = DateTime.UtcNow,
				Type = nameof(HistoryTests)
			};
		}
	}
}
