# terse

Comment linter for C#: rejects what no build-time analyzer catches, non-ASCII source,
multi-paragraph XML docs, comment essays, banners, `#region`. A .NET 10 console app
packaged as the dotnet tool `terse` (package id `Terse`), rules on Roslyn syntax-tree
trivia. Solution `Terse.slnx`, entry point `Terse/Program.cs`. README.md is the user-facing
description of the rules. All commands run from the repository root.

## Build

```sh
dotnet build
dotnet pack                                                  # Artifacts/package/release/Terse.<version>.nupkg
dotnet tool install -g Terse --add-source Artifacts/package/release   # no public feed yet
```

## Test

```sh
dotnet test          # xunit v3 on Microsoft.Testing.Platform; global.json selects the runner
```

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
