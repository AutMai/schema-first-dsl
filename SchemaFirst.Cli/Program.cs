using SchemaFirst.Generator;

// Argument parsing

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    PrintUsage();
    return 0;
}

// Last argument is output dir if it doesn't end in .schema, otherwise default
var lastArg = args[^1];
var hasOutDir =
    !lastArg.EndsWith(".schema", StringComparison.OrdinalIgnoreCase)
    && Directory.Exists(lastArg)
    || (!lastArg.EndsWith(".schema") && !File.Exists(lastArg));

string outputDir;
string[] inputArgs;

if (!lastArg.EndsWith(".schema", StringComparison.OrdinalIgnoreCase) &&
    args.Length > 1)
{
    outputDir = lastArg;
    inputArgs = args[..^1];
}
else
{
    outputDir = ".";
    inputArgs = args;
}

// Collect all .schema files from the given paths
var schemaFiles = new List<string>();
foreach (var input in inputArgs)
{
    if (Directory.Exists(input))
    {
        var found = Directory.GetFiles(input, "*.schema",
            SearchOption.TopDirectoryOnly);
        if (found.Length == 0)
            Console.WriteLine($"  ⚠  No .schema files found in '{input}'");
        schemaFiles.AddRange(found);
    }
    else if (File.Exists(input))
    {
        schemaFiles.Add(input);
    }
    else
    {
        Console.Error.WriteLine($"Error: not found: '{input}'");
        return 1;
    }
}

if (schemaFiles.Count == 0)
{
    Console.Error.WriteLine("Error: no .schema files to process.");
    return 1;
}

// Generate

var extensions = new Dictionary<string, string>
{
    ["SQL DDL"] = "sql",
    ["C#"] = "cs",
    ["TypeScript"] = "ts",
};

Directory.CreateDirectory(outputDir);
var generator = new SchemaGenerator();
var errors = 0;

foreach (var schemaFile in schemaFiles)
{
    var baseName = Path.GetFileNameWithoutExtension(schemaFile);
    Console.WriteLine($"Processing {Path.GetFileName(schemaFile)}...");
    try
    {
        var source = File.ReadAllText(schemaFile);
        var results = generator.Generate(source);

        foreach (var (target, code) in results)
        {
            var ext = extensions[target];
            var outPath =
                Path.Combine(outputDir, $"{baseName}.generated.{ext}");
            File.WriteAllText(outPath, code);
            Console.WriteLine($"  ✓  {target,-15} → {outPath}");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"  ✗  Error: {ex.Message}");
        errors++;
    }
}

Console.WriteLine();
Console.WriteLine(errors == 0
    ? $"Done. {schemaFiles.Count} file(s) processed."
    : $"Done with {errors} error(s). {schemaFiles.Count - errors}/{schemaFiles.Count} file(s) succeeded.");

return errors > 0 ? 1 : 0;

// Help 

static void PrintUsage()
{
    Console.WriteLine("""
                      SchemaFirst Generator
                      Usage:  schemafirst <input...> [output-dir]

                      Arguments:
                        input        One or more .schema files, or a folder containing .schema files
                        output-dir   Directory for generated files (default: current directory)

                      Examples:
                        schemafirst schemas/User.schema generated/
                        schemafirst schemas/User.schema schemas/Product.schema generated/
                        schemafirst schemas/ generated/
                      """);
}