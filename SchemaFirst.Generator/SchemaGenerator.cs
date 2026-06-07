using SchemaFirst.Generator.Ast;
using SchemaFirst.Generator.Lexer;
using SchemaFirst.Generator.Parser;
using SchemaFirst.Generator.Visitors;

namespace SchemaFirst.Generator;

public class SchemaGenerator
{
    private readonly List<IVisitor> _visitors;

    /// <summary>
    /// Create a generator with the default set of visitors
    /// (SQL, C#, TypeScript). Pass a custom list to restrict
    /// or extend the targets.
    /// </summary>
    public SchemaGenerator(IEnumerable<IVisitor>? visitors = null)
    {
        _visitors = visitors?.ToList() ?? new List<IVisitor>
        {
            new SqlVisitor(),
            new CSharpVisitor(),
            new TypeScriptVisitor(),
        };
    }

    /// <summary>
    /// Parse a DSL source string and return generated code
    /// for each registered visitor, keyed by target name.
    /// </summary>
    public Dictionary<string, string> Generate(string dslSource)
    {
        var tokens = new Lexer.Lexer(dslSource).Tokenize();
        var schema = new Parser.Parser(tokens).Parse();
        Validate(schema);

        return _visitors.ToDictionary(
            v => v.TargetName,
            v => v.Generate(schema));
    }

    //  Semantic validation 

    private static void Validate(SchemaNode schema)
    {
        var entityNames = schema.Entities.Select(e => e.Name).ToHashSet();

        foreach (var entity in schema.Entities)
        {
            var pkCount = entity.ScalarAttributes.Count(a => a.IsPrimaryKey);
            if (pkCount == 0)
                throw new Exception(
                    $"Entity '{entity.Name}' has no @primaryKey attribute.");
            if (pkCount > 1)
                throw new Exception(
                    $"Entity '{entity.Name}' has more than one @primaryKey attribute.");

            foreach (var rel in entity.RelationAttributes)
            {
                if (!entityNames.Contains(rel.Type))
                    throw new Exception(
                        $"Entity '{entity.Name}': relation '{rel.Name}' references " +
                        $"unknown entity '{rel.Type}'.");

                if (rel.Relation!.ForeignKeyField is { } fk && entity.ScalarAttributes.All(a => a.Name != fk))
                    throw new Exception(
                        $"Entity '{entity.Name}': @manyToOne('{fk}') references " +
                        $"unknown field '{fk}'.");
            }
        }
    }
}