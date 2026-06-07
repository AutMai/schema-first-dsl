using SchemaFirst.Generator;

// Run the generator programmatically against the User.schema
var schemaPath = Path.Combine(AppContext.BaseDirectory,
    "..", "..", "..", "schemas", "Relations.schema");

var source = File.ReadAllText(schemaPath);
var generator = new SchemaGenerator();
var results = generator.Generate(source);

foreach (var (target, code) in results)
{
    Console.WriteLine($"── {target} ────────────────────────────────");
    Console.WriteLine(code);
}

// Optionally write to the generated/ folder
var outputDir = Path.Combine(AppContext.BaseDirectory,
    "..", "..", "..", "generated");
Directory.CreateDirectory(outputDir);

var extensions = new Dictionary<string, string>
{
    ["SQL DDL"] = "sql",
    ["C#"] = "cs",
    ["TypeScript"] = "ts",
};

foreach (var (target, code) in results)
{
    var ext = extensions[target];
    var outPath = Path.Combine(outputDir, $"Relations.generated.{ext}");
    File.WriteAllText(outPath, code);
}

Console.WriteLine("Files written to generated/");