﻿using System.Collections;

using Crash.Server.Settings;

namespace Crash.Server.Tests
{

	public sealed class ConfigHandlerTests
	{

		[TestCaseSource(nameof(InvalidFilePaths))]
		public void InvalidConfig(string filePath)
		{
			var testConfig = new ConfigHandler(filePath);
			Assert.NotNull(testConfig);
		}

		public static IEnumerable InvalidFilePaths
		{
			get
			{
				yield return "TestFiles//emptyfile.json";
				yield return "missingfile.nosj";
				yield return "TestFiles//invalid_file_1.json";
			}
		}
	}
}
