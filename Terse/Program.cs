using Terse;

const string Usage = "usage: terse <path>...  |  terse --stdin <name>";

var counts = new Dictionary<string, int>();
var total = 0;

switch (args)
{
	case ["--stdin", var name]:
		if (!Rules.IsExcluded(name))
		{
			Lint(name, ReadStandardInput());
		}

		break;
	case ["-h" or "--help"]:
		Console.WriteLine(Usage);
		return 0;
	case var _ when args.Any(arg => arg.StartsWith('-')):
		Console.Error.WriteLine(Usage);
		return 2;
	default:
		var files = new Dictionary<string, string>();
		foreach (var root in args.Length > 0 ? args : ["."])
		{
			if (File.Exists(root))
			{
				files.TryAdd(Path.GetFullPath(root), root);
			}
			else if (Directory.Exists(root))
			{
				foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
				{
					files.TryAdd(Path.GetFullPath(file), file);
				}
			}
			else
			{
				Console.Error.WriteLine($"{root}: no such file or directory");
				return 2;
			}
		}

		foreach (var (fullPath, file) in files)
		{
			if (!Rules.IsExcluded(file))
			{
				Lint(file, File.ReadAllBytes(fullPath));
			}
		}

		if (total > 0 && files.Count > 1)
		{
			Console.WriteLine();
			Console.WriteLine($"total: {total}");
			foreach (var (rule, count) in counts.OrderByDescending(pair => pair.Value))
			{
				Console.WriteLine($"  {count,5}  {rule}");
			}
		}

		break;
}

return total > 0 ? 1 : 0;

// Console.In would decode with the console code page and report characters the file does not hold.
static byte[] ReadStandardInput()
{
	using var input = Console.OpenStandardInput();
	using var buffer = new MemoryStream();
	input.CopyTo(buffer);

	return buffer.ToArray();
}

void Lint(string file, ReadOnlySpan<byte> source)
{
	foreach (var violation in Rules.Check(source))
	{
		Console.WriteLine($"{file}:{violation.Line}: {violation.Rule}  {violation.Message}");
		counts[violation.Rule] = counts.GetValueOrDefault(violation.Rule) + 1;
		total++;
	}
}
