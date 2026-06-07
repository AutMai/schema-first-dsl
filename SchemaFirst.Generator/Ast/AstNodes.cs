namespace SchemaFirst.Generator.Ast;

// Top-level schema

public record SchemaNode(List<EntityNode> Entities);

// Entity

public record EntityNode(string Name, List<AttributeNode> Attributes)
{
    public IEnumerable<AttributeNode> ScalarAttributes =>
        Attributes.Where(a => a.Relation is null);

    public IEnumerable<AttributeNode> RelationAttributes =>
        Attributes.Where(a => a.Relation is not null);
}

// Attribute

public record AttributeNode(
    string Name,
    string Type, // primitive type OR entity name for relations
    bool IsPrimaryKey,
    bool IsRequired,
    bool IsExposed,
    string? DefaultValue,
    RelationInfo? Relation // null for scalar fields
);

// Relation

public enum RelationType
{
    ManyToOne,
    OneToMany,
    ManyToMany
}

/// <param name="Kind">Type of relation.</param>
/// <param name="ForeignKeyField">
///   For ManyToOne: the scalar field on this entity that holds the FK value.
///   For OneToMany/ManyToMany: null (FK lives on the other side).
/// </param>
public record RelationInfo(RelationType Kind, string? ForeignKeyField);