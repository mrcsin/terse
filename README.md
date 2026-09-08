# terse

A comment linter for C#, built for code that AI agents write and humans review. It runs
as a dotnet tool, on demand or from a git hook. The linter reads the comment trivia of
every `.cs` file and flags any character outside ASCII, XML docs that grew
into paragraphs, runs of `//` lines that explain what the code should have said itself,
decorative banners, `#region` blocks. The gate holds at zero. There is no baseline file
and no suppression marker: a comment either passes as written or the knowledge in it
moves into the code, the docs, or the test suite. A comment that cannot pass without
losing a fact is a stop, not a rewrite: report it to a human instead of shortening it.

The idea behind the rules is that a comment is a fixed cost every reader pays, so it has
to carry something the code cannot: a third-party quirk, an ordering constraint, a reason
no name can hold. One line of `//` above the code, a `<summary>` of up to three lines,
a trailing note after a statement, a `TODO`: all fine. Four `//` lines in a row, a
`/* */` block of four lines, a `<summary>` or `<remarks>` past three lines of text, a
`<para>` tag, a blank `///` line, a banner made of dashes or equals signs, `#region`:
the linter reports them. A separate rule rejects any character outside ASCII anywhere in
the source, comments and string literals alike, so a bidi override in a URL or a Cyrillic
letter in an identifier cannot slip through; user-facing text belongs in resources.

```csharp
// good: one fact the code cannot say
// The vendor SDK returns 0 for "unknown", not -1 as documented.
var level = sdk.ReadLevel();

/// <summary>Adds <paramref name="value"/> to the running total.</summary>
public int Add(int value) { ... }

var timeout = 30; // seconds

// bad: an essay
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

## Install and run

```sh
dotnet tool install -g Terse

terse <path> [<path>...]                 # directories to scan, or single files
git show :Foo.cs | terse --stdin Foo.cs  # text from stdin, named for the report
```

Output is `file:line: rule  message`, plus a per-rule summary when more than one file was
linted. Exit code 1 when violations exist, 0 when clean, 2 on a bad argument or a missing
path. Paths that are not `.cs`, generated files and build output are skipped. The full
catalogue of what passes and what fails lives in `Terse.Tests/Pass.cs` and `Terse.Tests/Fail/`,
one file per rule with the tool's output next to it.

## Git hook

`examples/pre-commit` lints the staged content of every staged `.cs` file through
`--stdin`, so what gets checked is what gets committed, not the working-tree copy. It
fails the commit when the tool is missing from `PATH` instead of skipping silently.

```sh
mkdir -p .githooks && cp examples/pre-commit .githooks/pre-commit
chmod +x .githooks/pre-commit
git config core.hooksPath .githooks
```
