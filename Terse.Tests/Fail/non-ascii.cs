namespace Terse.Tests.Fail;

public static class NonAscii
{
	/// <summary>Открывает файл.</summary>
	public static void CyrillicDoc() { }

	// сокет переиспользуется
	public static int Cyrillic => 1;

	/* inclusive,
	   включительно */
	public static int CyrillicBlock => 1;

	// ==== Вспомогательные ====
	public static int FramedCyrillic => 1;

	// ────────────────────
	public static int Box => 1;

	// 日本語のコメント
	public static int Japanese => 1;

	// ships in v2 🚀
	public static int Emoji => 1;

	// pens — one per channel
	public static int EmDash => 1;

	// stale after 30 s
	public static int Nbsp => 1;

	// see https://example.com/‮txt.exe
	public static int Bidi => 1;

	public static string Greeting => "привет";

	public static string Interpolated => $"привет, {Greeting}";

	public static string Flag => "🚀";

	public static int Привет => 1;
}
