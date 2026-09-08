namespace Terse.Tests.Fail;

public static class DocLong
{
	/// <summary>
	/// Line one.
	/// Line two.
	/// Line three.
	/// Line four.
	/// </summary>
	public static void FourLineSummary() { }

	/// <summary>Five lines with the tags on the text lines
	/// are still five lines of text,
	/// line three,
	/// line four,
	/// line five.</summary>
	public static void FiveLineSummaryWithInlineTags() { }

	/// <summary>Short.</summary>
	/// <remarks>
	/// Line one.
	/// Line two.
	/// Line three.
	/// Line four.
	/// </remarks>
	public static void FourLineRemarks() { }
}
