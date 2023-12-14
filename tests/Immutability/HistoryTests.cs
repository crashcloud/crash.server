// ReSharper disable HeapView.BoxingAllocation

using Crash.Changes.Utils;
using Crash.Server.Tests.Endpoints;
using Crash.Server.Tests.Utils;

namespace Crash.Server.Tests.Immutability
{
	public sealed class HistoryTests : CrashHubEndpoints
	{
		private const string ExamplePayload = "{ \"example\": \"payload\" }";

		private static IEnumerable NonConflictingChangeStacks
		{
			get
			{
				yield return initialOnly();
				yield return AddRemoveStack();
				yield return GenerateLifetimeStack();
			}
		}

		[Test]
		public void NullChange()
		{
			// _crashHub.Add
		}

		[TestCaseSource(nameof(NonConflictingChangeStacks))]
		public async Task TestContext(ValueTuple<Stack<Change>, Change> changeTuple)
		{
			var changeHistory = changeTuple.Item1;
			var latestChange = changeTuple.Item2;

			var context = MockCrashHub.GetContext(MockCrashHub.GetLogger());
			
			await Task.WhenAll(changeHistory.Select(
				async c => await context.AddChangeAsync(new ImmutableChange
					{
						UniqueId = Guid.NewGuid(),
						Id = c.Id,
						Owner = c.Owner,
						Action = c.Action,
						Payload = c.Payload,
						Stamp = c.Stamp,
						Type = c.Type
					}
				)));

			var changes = context.GetChanges();

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

				PayloadUtils.TryGetPayloadFromChange(latestChange, out var changePacket);
				PayloadUtils.TryGetPayloadFromChange(latestFound, out var foundPacket);

				// Payload Can be null or not null?
				// It'd be more efficient if null
				Assert.That(string.Equals(changePacket.Data, foundPacket.Data,
							StringComparison.OrdinalIgnoreCase));

				// Type must never be null
				Assert.That(string.IsNullOrEmpty(latestFound.Type), Is.False);
				Assert.That(latestFound.Type, Is.EqualTo(latestChange.Type));

				// Action must never be None
				Assert.That(latestFound.Action, Is.Not.EqualTo(ChangeAction.None));
				Assert.That(latestFound.Action.HasFlag(latestChange.Action));
			});
		}

		private static (Stack<Change>, Change) initialOnly()
		{
			Stack<Change> firstHistory = new();
			var initialChange = getNewChange(Guid.NewGuid(), ChangeAction.Add, ExamplePayload);
			firstHistory.Push(initialChange);

			return new ValueTuple<Stack<Change>, Change>(firstHistory, initialChange);
		}

		private static (Stack<Change>, Change) AddRemoveStack()
		{
			Stack<Change> firstHistory = new();
			var stackId = Guid.NewGuid();

			var add = getNewChange(stackId, ChangeAction.Add, ExamplePayload);
			var remove = getNewChange(stackId, ChangeAction.Remove, ExamplePayload);

			firstHistory.Push(add);
			firstHistory.Push(remove);

			return new ValueTuple<Stack<Change>, Change>(firstHistory, add);
		}

		private static (Stack<Change>, Change) GenerateLifetimeStack()
		{
			Stack<Change> firstHistory = new();
			var stackId = Guid.NewGuid();
			var first = getNewChange(stackId, ChangeAction.Add, ExamplePayload);
			firstHistory.Push(first);
			firstHistory.Push(getNewChange(stackId, ChangeAction.Locked, null));

			var transform = new CTransform(1, 2, 3, 4);
			var json = JsonSerializer.Serialize(transform);

			firstHistory.Push(getNewChange(stackId, ChangeAction.Transform, json));
			firstHistory.Push(getNewChange(stackId, ChangeAction.Temporary, null));
			firstHistory.Push(getNewChange(stackId, ChangeAction.Unlocked, null));
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
			return new Change
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
