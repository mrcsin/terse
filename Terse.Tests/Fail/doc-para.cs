namespace Terse.Tests.Fail;

public static class DocPara
{
	/// <summary>Short.</summary>
	/// <remarks><para>One.</para><para>Two.</para></remarks>
	public static void Para() { }

	/// <summary>One.<para/>Two.</summary>
	public static void SelfClosingPara() { }

	/// <summary>Short.</summary>
	///
	/// <remarks>More.</remarks>
	public static void BlankDocLine() { }
}
