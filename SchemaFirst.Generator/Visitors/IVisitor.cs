using SchemaFirst.Generator.Ast;

namespace SchemaFirst.Generator.Visitors;

//  Visitor interface 

/// <summary>
/// Each code-generation target implements this interface.
/// Add a new target by creating a new IVisitor implementation —
/// no changes to the parser or AST are required.
/// </summary>
public interface IVisitor
{
    /// <summary>Human-readable name shown in CLI output.</summary>
    string TargetName { get; }

    /// <summary>Generate source code for a full schema.</summary>
    string Generate(SchemaNode schema);
}

//  Shared type mapping 

/// <summary>
/// Central registry of DSL type → target-language type mappings.
/// Extend this class when adding a new target language.
/// </summary>
public static class TypeMap
{
    public static string ToSql(string dslType) => dslType switch
    {
        "Int" => "INTEGER",
        "Text" => "VARCHAR(255)",
        "Boolean" => "BOOLEAN",
        "Date" => "DATE",
        "Decimal" => "DECIMAL(10,2)",
        _ => throw new Exception($"Unknown DSL type: {dslType}")
    };

    public static string ToCSharp(string dslType, bool nullable) => dslType switch
    {
        "Int" => nullable ? "int?" : "int",
        "Text" => nullable ? "string?" : "string",
        "Boolean" => nullable ? "bool?" : "bool",
        "Date" => nullable ? "DateOnly?" : "DateOnly",
        "Decimal" => nullable ? "decimal?" : "decimal",
        _ => throw new Exception($"Unknown DSL type: {dslType}")
    };

    public static string ToTypeScript(string dslType) => dslType switch
    {
        "Int" => "number",
        "Text" => "string",
        "Boolean" => "boolean",
        "Date" => "Date",
        "Decimal" => "number",
        _ => throw new Exception($"Unknown DSL type: {dslType}")
    };
}