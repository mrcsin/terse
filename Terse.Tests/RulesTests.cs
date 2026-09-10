using System.Text;

using Xunit;

namespace Terse.Tests;

public sealed class RulesTests
{
	private const string EssaySource = "// one\n// two\n// three\n// four\nclass A { }\n";

	public static TheoryData<string> FailFixtures => [.. Directory.GetFiles(FailDirectory, "*.cs").Select(file => Path.GetFileName(file))];

	private static string FailDirectory => Path.Combine(AppContext.BaseDirectory, "Fail");

	[Fact]
	public void PassReportsNothing()
	{
		Assert.Empty(Rules.Check(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Pass.cs"))));
	}

	[Theory]
	[MemberData(nameof(FailFixtures))]
	public void FailFixtureMatchesItsSnapshot(string fixture)
	{
		var source = File.ReadAllBytes(Path.Combine(FailDirectory, fixture));
		var report = Rules.Check(source).Select(violation => $"{fixture}:{violation.Line}: {violation.Rule}  {violation.Message}");
		var snapshot = File.ReadAllText(Path.Combine(FailDirectory, Path.ChangeExtension(fixture, ".expected")));

		Assert.Equal(snapshot.ReplaceLineEndings("\n").TrimEnd(), string.Join('\n', report));
	}

	[Fact]
	public void LeadingByteOrderMarkChangesNothing()
	{
		var withMark = Rules.Check(Encoding.UTF8.GetBytes("\uFEFF" + EssaySource));
		var without = Rules.Check(Encoding.UTF8.GetBytes(EssaySource));

		Assert.Equal(without, withMark);
	}

	[Fact]
	public void LeadingByteOrderMarkDoesNotMaskTheFirstLine()
	{
		var report = Rules.Check(Encoding.UTF8.GetBytes("\uFEFF" + EssaySource));

		Assert.Equal([new Violation(1, "comment-essay", "4 consecutive // lines; move the knowledge into code or docs")], report);
	}

	[Fact]
	public void Utf16SourceReportsOneInvalidByte()
	{
		var report = Rules.Check([.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes("// x\n")]);

		Assert.Equal([new Violation(1, "non-ascii", "invalid UTF-8 byte 0xFF; source is ASCII only")], report);
	}

	[Fact]
	public void InvalidByteReportsItsOwnLine()
	{
		var report = Rules.Check([.. "// one\n// two\n"u8, 0xFF, .. "\nclass A { }\n"u8]);

		Assert.Equal([new Violation(3, "non-ascii", "invalid UTF-8 byte 0xFF; source is ASCII only")], report);
	}

	[Theory]
	[InlineData("src/App/Program.cs", false)]
	[InlineData("Program.cs", false)]
	[InlineData("bin/Program.cs", true)]
	[InlineData("src/App/obj/Program.cs", true)]
	[InlineData("Artifacts/package/release/Program.cs", true)]
	[InlineData("src/App/Form1.Designer.cs", true)]
	[InlineData("src/App/Resources.g.cs", true)]
	[InlineData("README.md", true)]
	[InlineData("src/App.csproj", true)]
	[InlineData(@"src\App\obj\Program.cs", true)]
	public void ExcludesNonSourceAndBuildOutput(string path, bool excluded)
	{
		Assert.Equal(excluded, Rules.IsExcluded(path));
	}
}
