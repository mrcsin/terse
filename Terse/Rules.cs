using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Terse;

public sealed record Violation(int Line, string Rule, string Message);

public static class Rules
{
	private const int MaxDocLines = 3;
	private const int EssayLines = 4;
	private const string BannerChars = "=-*_~#";

	private static readonly UTF8Encoding _utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	public static bool IsExcluded(string file)
	{
		var path = "/" + file.Replace('\\', '/');

		return !path.EndsWith(".cs", StringComparison.Ordinal)
			|| path.EndsWith(".g.cs", StringComparison.Ordinal)
			|| path.EndsWith(".Designer.cs", StringComparison.Ordinal)
			|| path.Contains("/bin/") || path.Contains("/obj/") || path.Contains("/Artifacts/") || path.Contains("/.git/");
	}

	public static List<Violation> Check(ReadOnlySpan<byte> source)
	{
		// A byte order mark records the file's encoding, not its text; the C# compiler skips it too.
		if (source.StartsWith(Encoding.UTF8.Preamble))
		{
			source = source[Encoding.UTF8.Preamble.Length..];
		}

		try
		{
			return CheckText(_utf8.GetString(source));
		}
		catch (DecoderFallbackException exception)
		{
			var line = source[..exception.Index].Count((byte)'\n') + 1;
			var value = exception.BytesUnknown is [var first, ..] ? first : (byte)0;

			return [new Violation(line, "non-ascii", $"invalid UTF-8 byte 0x{value:X2}; source is ASCII only")];
		}
	}

	private static List<Violation> CheckText(string source)
	{
		var tree = CSharpSyntaxTree.ParseText(source);
		var text = tree.GetText();
		var violations = new List<Violation>();
		var commentLines = new List<int>();

		foreach (var trivia in tree.GetRoot().DescendantTrivia(descendIntoTrivia: true))
		{
			var line = text.Lines.GetLinePosition(trivia.SpanStart).Line + 1;

			switch (trivia.Kind())
			{
				case SyntaxKind.SingleLineCommentTrivia:
					var comment = trivia.ToString();
					if (IsBanner(comment))
					{
						violations.Add(new Violation(line, "banner", "decorative separator comment"));
					}
					else if (comment.Length > 2 && comment[2] != ' ')
					{
						violations.Add(new Violation(line, "comment-style", "one space after //"));
					}

					if (StartsItsLine(text, trivia))
					{
						commentLines.Add(line);
					}

					break;
				case SyntaxKind.MultiLineCommentTrivia:
					var blockLines = LineCount(trivia);
					if (blockLines >= EssayLines)
					{
						violations.Add(new Violation(line, "comment-essay", $"{blockLines}-line /* */ block; move the knowledge into code or docs"));
					}

					break;
				case SyntaxKind.SingleLineDocumentationCommentTrivia:
					violations.AddRange(CheckDoc(text, trivia));
					break;
				case SyntaxKind.MultiLineDocumentationCommentTrivia:
					violations.Add(new Violation(line, "comment-style", "use /// doc comments, not /** */"));
					break;
				case SyntaxKind.RegionDirectiveTrivia:
					violations.Add(new Violation(line, "region", "#region hides structure; split the type instead"));
					break;
			}
		}

		violations.AddRange(CheckEssays(commentLines));
		violations.AddRange(CheckAscii(source, text));

		return [.. violations.OrderBy(violation => violation.Line).ThenBy(violation => violation.Rule, StringComparer.Ordinal)];
	}

	private static bool IsBanner(string comment)
	{
		var body = comment.TrimStart('/').Trim();
		if (body.Length == 0)
		{
			return comment.TrimEnd().Length >= 4;
		}

		var leading = body.TakeWhile(BannerChars.Contains).Count();
		var trailing = body.Reverse().TakeWhile(BannerChars.Contains).Count();
		var framed = leading >= 3 && trailing >= 3 && body.Length > leading + trailing;
		var ruled = body.Count(BannerChars.Contains) >= 4 && body.All(c => c == ' ' || BannerChars.Contains(c));

		return framed || ruled;
	}

	private static bool StartsItsLine(SourceText text, SyntaxTrivia trivia)
	{
		var lineStart = text.Lines.GetLineFromPosition(trivia.SpanStart).Start;

		return string.IsNullOrWhiteSpace(text.ToString(TextSpan.FromBounds(lineStart, trivia.SpanStart)));
	}

	private static int LineCount(SyntaxTrivia trivia)
	{
		var span = trivia.GetLocation().GetLineSpan();

		return span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
	}

	private static IEnumerable<Violation> CheckDoc(SourceText text, SyntaxTrivia trivia)
	{
		if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax doc)
		{
			yield break;
		}

		var paraLines = doc.DescendantNodes().OfType<XmlNameSyntax>()
			.Where(name => name.LocalName.ValueText == "para")
			.Select(name => LineOf(text, name.SpanStart))
			.Distinct();
		foreach (var line in paraLines)
		{
			yield return new Violation(line, "doc-para", "XML doc must be one paragraph (no <para>)");
		}

		var firstLine = LineOf(text, trivia.FullSpan.Start);
		var docLines = trivia.ToFullString().Split('\n');
		for (var i = 0; i < docLines.Length; i++)
		{
			if (docLines[i].Trim() == "///")
			{
				yield return new Violation(firstLine + i, "doc-para", "blank /// line splits the doc into paragraphs");
			}
		}

		foreach (var element in doc.DescendantNodes().OfType<XmlElementSyntax>())
		{
			var tag = element.StartTag.Name.LocalName.ValueText;
			if (tag is not ("summary" or "remarks"))
			{
				continue;
			}

			var contentLines = string.Concat(element.Content.Select(node => node.ToFullString()))
				.Split('\n')
				.Count(docLine => docLine.TrimStart().TrimStart('/').Trim().Length > 0);
			if (contentLines > MaxDocLines)
			{
				yield return new Violation(LineOf(text, element.SpanStart), "doc-long",
					$"<{tag}> is {contentLines} lines; one tight fact, max {MaxDocLines} lines");
			}
		}
	}

	private static IEnumerable<Violation> CheckEssays(List<int> commentLines)
	{
		var start = 0;
		while (start < commentLines.Count)
		{
			var end = start;
			while (end + 1 < commentLines.Count && commentLines[end + 1] == commentLines[end] + 1)
			{
				end++;
			}

			var length = end - start + 1;
			if (length >= EssayLines)
			{
				yield return new Violation(commentLines[start], "comment-essay", $"{length} consecutive // lines; move the knowledge into code or docs");
			}

			start = end + 1;
		}
	}

	private static IEnumerable<Violation> CheckAscii(string source, SourceText text)
	{
		var offset = 0;
		while (true)
		{
			var hit = source.AsSpan(offset).IndexOfAnyExceptInRange('\0', '\x7F');
			if (hit < 0)
			{
				yield break;
			}

			var position = offset + hit;
			Rune.DecodeFromUtf16(source.AsSpan(position), out var rune, out _);
			var line = text.Lines.GetLineFromPosition(position);

			yield return new Violation(line.LineNumber + 1, "non-ascii", $"non-ASCII character U+{rune.Value:X4}; source is ASCII only");

			offset = line.EndIncludingLineBreak;
		}
	}

	private static int LineOf(SourceText text, int position)
	{
		return text.Lines.GetLinePosition(position).Line + 1;
	}
}
