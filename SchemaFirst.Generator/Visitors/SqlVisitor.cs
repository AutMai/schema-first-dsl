using SchemaFirst.Generator.Ast;
using System.Text;

namespace SchemaFirst.Generator.Visitors;

public class SqlVisitor : IVisitor
{
    public string TargetName => "SQL DDL";

    public string Generate(SchemaNode schema)
    {
        var sb = new StringBuilder();
        foreach (var entity in schema.Entities)
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine(GenerateTable(entity, schema));
        }

        // ManyToMany: generate junction tables (deduplicated)
        var junctions = new HashSet<string>();
        foreach (var entity in schema.Entities)
        {
            foreach (var attr in entity.RelationAttributes
                         .Where(a => a.Relation!.Kind == RelationType.ManyToMany))
            {
                // Normalize: always sort the two names so A_B == B_A
                var pair = string.Compare(entity.Name, attr.Type, StringComparison.Ordinal) <= 0
                    ? $"{entity.Name}_{attr.Type}"
                    : $"{attr.Type}_{entity.Name}";

                if (junctions.Add(pair))
                {
                    var parts = pair.Split('_');
                    sb.AppendLine();
                    sb.AppendLine(GenerateJunctionTable(parts[0], parts[1]));
                }
            }
        }

        return sb.ToString();
    }

    private static string GenerateTable(EntityNode entity, SchemaNode schema)
    {
        var sb = new StringBuilder();
        var lines = new List<string>();

        sb.AppendLine($"CREATE TABLE {entity.Name} (");

        // Scalar columns
        foreach (var attr in entity.ScalarAttributes)
        {
            var sqlType = TypeMap.ToSql(attr.Type);
            var notNull = attr.IsRequired ? " NOT NULL" : "";
            var def = attr.DefaultValue is not null ? $" DEFAULT {attr.DefaultValue}" : "";
            lines.Add($"  {ToSnakeCase(attr.Name),-22}{sqlType}{notNull}{def}");
        }

        // ManyToOne: add FK column (e.g. user_id INTEGER NOT NULL)
        foreach (var attr in entity.RelationAttributes
                     .Where(a => a.Relation!.Kind == RelationType.ManyToOne))
        {
            // Only generate the FK column if it's not already a declared scalar field
            var fkField = attr.Relation!.ForeignKeyField;
            if (fkField is not null &&
                entity.ScalarAttributes.Any(a => a.Name == fkField))
                continue; // already declared explicitly

            var colName = fkField is not null
                ? ToSnakeCase(fkField)
                : ToSnakeCase(attr.Name) + "_id";
            var notNull = attr.IsRequired ? " NOT NULL" : "";
            lines.Add($"  {colName,-22}INTEGER{notNull}");
        }

        // PRIMARY KEY
        var pk = entity.ScalarAttributes.FirstOrDefault(a => a.IsPrimaryKey);
        if (pk is not null)
            lines.Add($"  PRIMARY KEY ({ToSnakeCase(pk.Name)})");

        // FOREIGN KEY constraints
        foreach (var attr in entity.RelationAttributes
                     .Where(a => a.Relation!.Kind == RelationType.ManyToOne))
        {
            var fkField = attr.Relation!.ForeignKeyField;
            var colName = fkField is not null
                ? ToSnakeCase(fkField)
                : ToSnakeCase(attr.Name) + "_id";

            // Find PK of the referenced entity
            var refEntity = schema.Entities.FirstOrDefault(e => e.Name == attr.Type);
            var refPk = refEntity?.ScalarAttributes.FirstOrDefault(a => a.IsPrimaryKey);
            var refCol = refPk is not null ? ToSnakeCase(refPk.Name) : "id";

            lines.Add($"  FOREIGN KEY ({colName}) REFERENCES {attr.Type}({refCol})");
        }

        sb.Append(string.Join(",\n", lines));
        sb.AppendLine();
        sb.Append(");");
        return sb.ToString();
    }

    private static string GenerateJunctionTable(string entityA, string entityB)
    {
        var tableA = ToSnakeCase(entityA);
        var tableB = ToSnakeCase(entityB);
        return $"""
                CREATE TABLE {entityA}_{entityB} (
                  {tableA}_id  INTEGER NOT NULL,
                  {tableB}_id  INTEGER NOT NULL,
                  PRIMARY KEY ({tableA}_id, {tableB}_id),
                  FOREIGN KEY ({tableA}_id) REFERENCES {entityA}(id),
                  FOREIGN KEY ({tableB}_id) REFERENCES {entityB}(id)
                );
                """;
    }

    private static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder();
        foreach (var ch in name)
        {
            if (char.IsUpper(ch) && sb.Length > 0) sb.Append('_');
            sb.Append(char.ToLower(ch));
        }

        return sb.ToString();
    }
}