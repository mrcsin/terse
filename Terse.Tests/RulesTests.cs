using Xunit;

namespace Terse.Tests;

public sealed class RulesTests
{
	public static TheoryData<string> FailFixtures => [.. Directory.GetFiles(FailDirectory, "*.cs").Select(file => Path.GetFileName(file))];

	private static string FailDirectory => Path.Combine(AppContext.BaseDirectory, "Fail");

	[Fact]
	public void PassReportsNothing()
	{
		Assert.Empty(Rules.Check(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Pass.cs"))));
	}

	[Theory]
	[MemberData(nameof(FailFixtures))]
	public void FailFixtureMatchesItsSnapshot(string fixture)
	{
		var source = File.ReadAllText(Path.Combine(FailDirectory, fixture));
		var report = Rules.Check(source).Select(violation => $"{fixture}:{violation.Line}: {violation.Rule}  {violation.Message}");
		var snapshot = File.ReadAllText(Path.Combine(FailDirectory, Path.ChangeExtension(fixture, ".expected")));

		Assert.Equal(snapshot.ReplaceLineEndings("\n").TrimEnd(), string.Join('\n', report));
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
