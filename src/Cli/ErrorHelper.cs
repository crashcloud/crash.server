
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Crash.Server.Cli;

internal class ErrorHelper
{

	internal record struct ErrorHandler(Func<Exception, bool> Predicate, string Message, string HighlightPattern);

	private string ArgsMessage { get; }

	private List<ErrorHandler> ErrorHandlers { get; } = new();

	public ErrorHelper(string[] args)
	{
		ErrorHandlers = GetErrorHandlers();
		ArgsMessage = $"        >> {string.Join(", ", args)} <<";
	}

	public bool TryCaptureException(Exception ex, out string assistanceMessage)
	{
		assistanceMessage = string.Empty;
		if (ex is null) return false;

		var assistanceMessages = new List<string>
		{
			ArgsMessage,
			GetPaddedNewLine(ArgsMessage.Length)
		};

		var helpFound = false;

		foreach (var handler in ErrorHandlers)
		{
			if (!handler.Predicate(ex)) continue;
			// TODO : Perform sorting!
			if (!TryGetStringSection(handler.HighlightPattern, out int startIndex, out int endIndex)) continue;
			var newLine = GetPaddedNewLine(ArgsMessage.Length)!;

			// Handle Arrows
			var arrowLine = new Span<char>(assistanceMessages[1].ToCharArray());

			arrowLine[startIndex] = '^';
			// arrowLine[endIndex] = '^';

			assistanceMessages[1] = arrowLine.ToString();

			var newHelpMessage = GetPaddedNewLine((ArgsMessage.Length / 2) + 2)! + handler.Message;

			var helpLine = new Span<char>(newHelpMessage.ToCharArray());

			assistanceMessages.Add(helpLine.ToString());

			helpFound = true;
		}

		if (assistanceMessages.Count < 3) return false;
		var arrowMessage = assistanceMessages[1];

		for (int i = 2; i < assistanceMessages.Count; i++)
		{
			var helpMessage = new Span<char>(assistanceMessages[i].ToCharArray());

			for (int j = 0 + 1; j < arrowMessage.Length; j++)
			{
				if (arrowMessage[j] == '^')
				{
					helpMessage[j] = ':';

					helpMessage.Slice(j + 1, ArgsMessage.Length - j).Fill('_');
				}
			}

			assistanceMessages[i] = helpMessage.ToString();
		}

		assistanceMessage = string.Join("\n", assistanceMessages);

		return helpFound;
	}

	private static string GetPaddedNewLine(int length, char padding = ' ')
	{
		var spaces = new char[length];
		Array.Fill(spaces, padding);
		return string.Join(" ", spaces);
	}

	private bool TryGetStringSection(string pattern, out int startIndex, out int endIndex)
	{
		startIndex = -1;
		endIndex = -1;
		try
		{
			var match = Regex.Match(ArgsMessage, pattern);
			if (!match.Success) return false;

			var capture = match.Captures[0];
			startIndex = capture.Index;
			endIndex = startIndex + capture.Length;

			return startIndex < endIndex;
		}
		catch { }
		return false;
	}

	private static List<ErrorHandler> GetErrorHandlers() =>
	[
		new ErrorHandler(ex => ex is SocketException, "It is possible your port is unavailable. Is the port correct?", @"[\d.]:"),
		new ErrorHandler(ex => ex is SocketException, "It is possible your address is unreachable. Is the address correct? Can you ping it successfully?", @"://[\d.]+:"),
		// new ErrorHandler(ex => ex is Exception, "It is possible your address is unreachable. Is the address correct? Can you ping it successfully?", @"://[\d.]:"),
	];

}
