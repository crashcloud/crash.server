namespace Crash.Server.Tests
{
	public static class ChangeUtils
	{
		public static bool CompareChanges(IChange left, IChange right)
		{
			if (left.Id != right.Id)
			{
				return false;
			}

			if (left.Owner != right.Owner)
			{
				return false;
			}

			if (left.Action != right.Action)
			{
				return false;
			}

			if (left.Payload != right.Payload)
			{
				return false;
			}

			if (left.Type != right.Type)
			{
				return false;
			}

			if (left.Stamp != right.Stamp)
			{
				return false;
			}

			return true;
		}
	}
}
