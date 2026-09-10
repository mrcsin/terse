// Comments the linter lets through. Every shape here is a real comment in real code;
// the test asserts the file reports nothing.

namespace Terse.Tests;

public static class Pass
{
	/// <summary>Adds <paramref name="value"/> to <see cref="Total"/>.</summary>
	/// <param name="value">The <c>int</c> to add.</param>
	/// <returns>The new total.</returns>
	public static int Add(int value)
	{
		Total += value;
		return Total;
	}

	/// <summary>Running total.</summary>
	public static int Total { get; private set; }

	/// <summary>
	/// A three-line summary, the maximum.
	/// Line two.
	/// Line three.
	/// </summary>
	public static void ThreeLineSummary() { }

	/// <summary>Short.</summary>
	/// <remarks>
	/// Remarks get the same limit as the summary.
	/// Line two.
	/// Line three.
	/// </remarks>
	public static void ThreeLineRemarks() { }

	/// <summary>Opens the file.</summary>
	/// <exception cref="IOException">The file is locked.</exception>
	public static void Throws() { }

	/// <inheritdoc/>
	public static void Inherits() { }

	// one line above code
	public static int Socket => 1;

	// three consecutive lines
	// is the maximum
	// before the run counts as an essay
	public static int Run => 3;

	//
	public static int Empty => 0;

	// var old = Compute();
	public static int CommentedOut => 1;

	// TODO: refactor later
	public static int Todo => 1;

	// see https://example.com/spec#section-4
	public static int Url => 1;

	// dashes inside text --- are not a banner
	public static int Dashes => 1;

	// --- three characters alone are not a rule
	public static int Three => 1;

	//    
	public static int TrailingSpaces => 1;

	public static int Timeout => 30; // seconds
	public static int Retries => 3;  // per host
	public static int Backoff => 2;  // multiplier
	public static int Jitter => 1;   // trailing comments never form an essay

	public static int Inclusive => /* inclusive */ 1;

	/* a block comment
	   over three lines
	   is under the essay limit */
	public static int Block => 1;

	// run a, line one
	// run a, line two
	// run a, line three

	// run b, line one
	// run b, line two
	// run b, line three
	public static int Split => 1;

#if DEBUG
	public static int Debug => 1;
#else
	public static int Debug => 0;
#endif
}
