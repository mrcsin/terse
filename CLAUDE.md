# terse

Comment linter for C#: rejects what no build-time analyzer catches, non-ASCII source,
multi-paragraph XML docs, comment essays, banners, `#region`. A .NET 10 console app
packaged as the dotnet tool `terse` (package id `terse`), rules on Roslyn syntax-tree
trivia. Solution `Terse.slnx`, entry point `Terse/Program.cs`. README.md is the user-facing
description of the rules. All commands run from the repository root.

## Build

```sh
dotnet build
dotnet pack                          # Artifacts/package/release/terse.<version>.nupkg
dotnet tool install -g terse         # released versions, from nuget.org
dotnet tool install -g terse --add-source Artifacts/package/release   # the local build
```

A `v*` tag runs `.github/workflows/release.yml`: test, pack at the tag version, push to
nuget.org and to GitHub Packages (`nuget.pkg.github.com/mrcsin`), then create the GitHub
release from the tag annotation.

## Test

```sh
dotnet test          # xunit v3 on Microsoft.Testing.Platform; global.json selects the runner
```

`Rules.Check` takes the file's bytes and owns the decode: it skips a leading UTF-8 byte order
mark, decodes strictly, and reports an invalid byte as `non-ascii` without parsing. Both the file
path and `--stdin` go through it, so the two modes cannot disagree.

`Terse.Tests/Pass.cs` must lint clean. `Terse.Tests/Fail/<rule>.cs` holds the shapes one
rule rejects; `<rule>.expected` beside it is the tool's own output for that file, compared
verbatim. After changing a rule or a fixture, regenerate the snapshots and review the diff:

```sh
cd Terse.Tests/Fail && for f in *.cs; do dotnet run --project ../../Terse -- "$f" > "${f%.cs}.expected"; done
```

## Format

```sh
dotnet format --verify-no-changes
dotnet run --project Terse -- Terse Terse.Tests/RulesTests.cs Terse.Tests/Pass.cs   # dogfood, must exit 0
```

## Layout

```
Terse/               Program.cs the CLI, Rules.cs the rules, csproj carries tool packaging
Terse.Tests/         Pass.cs, Fail/ fixtures with .expected snapshots, RulesTests.cs
examples/pre-commit  staged-content hook for consumers
docs/plans/          dated work plans
```
