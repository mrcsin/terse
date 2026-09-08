namespace Terse.Tests.Fail;

public static class CommentStyle
{
	/** <summary>Short.</summary> */
	public static void StarDoc() { }

	//no space after the slashes
	public static int NoSpace => 1;

	//	tab after the slashes
	public static int Tab => 1;

	//var old = Compute();
	public static int CommentedOut => 1;

	//!important
	public static int Bang => 1;

	//// old code behind four slashes
	public static int FourSlashes => 1;
}
