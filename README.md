# terse

[![C#](https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white)](#)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=fff)](#)
[![NuGet](https://img.shields.io/nuget/v/terse?logo=nuget&logoColor=fff&label=NuGet)](https://www.nuget.org/packages/terse)
[![release](https://github.com/mrcsin/terse/actions/workflows/release.yml/badge.svg)](https://github.com/mrcsin/terse/actions/workflows/release.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comment linter for C#. It reads the comments of every `.cs` file and reports the ones
that carry nothing the code could carry itself: essays, multi-paragraph XML docs,
decorative banners, `#region`. A separate rule rejects any character outside ASCII. The
tool is built for code that AI agents write and humans review, and it runs as a dotnet
tool, on demand or from a git hook.

## Install

```sh
dotnet tool install -g terse
```

## Use

```sh
terse                                    # scan the current directory
terse Terse Terse.Tests/Pass.cs          # directories, single files, or both
git show :Foo.cs | terse --stdin Foo.cs  # lint the piped text, report it as Foo.cs
```

With `--stdin` the tool lints what the pipe gives it and never opens `Foo.cs` on disk. The
name decides two things: what the report prints in front of the line number, and whether
the input is skipped as generated code or build output.

Every violation prints as `file:line: rule  message`. When the run covers more than one
file, a per-rule count follows:

```
Terse.Tests/Fail/comment-essay.cs:5: comment-essay  4 consecutive // lines; move the knowledge into code or docs
Terse.Tests/Fail/region.cs:5: region  #region hides structure; split the type instead

total: 3
      2  comment-essay
      1  region
```

Exit code 0 when clean, 1 when a violation was found, 2 on a bad argument or a missing
path. Skipped without a word: files that are not `.cs`, generated code (`*.g.cs`,
`*.Designer.cs`) and build output (`bin/`, `obj/`, `Artifacts/`, `.git/`).

## Rules

| Rule | What it reports |
| --- | --- |
| `comment-essay` | 4 or more `//` lines in a row, or a `/* */` block of 4 or more lines |
| `doc-long` | a `<summary>` or `<remarks>` longer than 3 lines |
| `doc-para` | a `<para>` tag or a blank `///` line inside an XML doc |
| `banner` | a separator comment drawn out of `= - * _ ~ #`, or a bare run of slashes |
| `region` | `#region` |
| `comment-style` | a `//` with no space after it, or a `/** */` doc comment |
| `non-ascii` | any code point above U+007F after UTF-8 decoding, anywhere in the file |

The ASCII rule covers the whole file, string literals included, so a bidi override in a URL
or a Cyrillic letter in an identifier cannot slip through. Text that users read belongs in
resources.

A leading UTF-8 byte order mark is skipped, not reported: file encoding, line endings and the
final newline belong to `.editorconfig` and `dotnet format --verify-no-changes`, the same way
indentation does. A file that is not valid UTF-8, which includes UTF-16 saved with its byte
order mark, reports one `non-ascii` line naming the byte and is not parsed.

This passes:

```csharp
// The vendor SDK returns 0 for "unknown", not -1 as documented.
var level = sdk.ReadLevel();

/// <summary>Adds <paramref name="value"/> to the running total.</summary>
public int Add(int value) { ... }

var timeout = 30; // seconds

// TODO: drop once the firmware reports the flag.
```

This fails:

```csharp
// First we open the socket.
// Then we wait for the handshake.
// If the handshake fails we retry twice.
// After that we give up and log the error.
Connect();

/// <summary>Opens the file.</summary>
/// <remarks><para>First paragraph.</para><para>Second paragraph.</para></remarks>
public void Open() { ... }

// ==================== Helpers ====================
#region Fields
```

The full catalogue is the test suite: `Terse.Tests/Pass.cs` is everything that must stay
clean, and `Terse.Tests/Fail/` holds one file per rule with the tool's output beside it.

## Git hook

`examples/pre-commit` lints the staged content of every staged `.cs` file through `--stdin`,
so the check runs on what gets committed and not on the working-tree copy. It fails the
commit when `terse` is missing from `PATH` instead of skipping silently.

```sh
mkdir -p .githooks && cp examples/pre-commit .githooks/pre-commit
chmod +x .githooks/pre-commit
git config core.hooksPath .githooks
```

## No baseline, no suppression

The gate holds at zero. A comment either passes as written, or the knowledge in it moves
into the code, the docs, or the test suite. There is no baseline file and no suppression
marker, so a legacy repository starts at whatever number it starts at and works down.

A comment that cannot pass without losing a fact is a stop, not a rewrite: report it to a
human instead of shortening it. The reason for the whole gate is that a comment is a fixed
cost every reader pays, so it has to carry what the code cannot: a third-party quirk, an
ordering constraint, a reason no name can hold.
