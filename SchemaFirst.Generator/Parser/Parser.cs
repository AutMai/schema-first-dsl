using SchemaFirst.Generator.Ast;
using SchemaFirst.Generator.Lexer;

namespace SchemaFirst.Generator.Parser;

public class Parser(List<Token> tokens)
{
    private int _pos;

    public SchemaNode Parse()
    {
        var entities = new List<EntityNode>();
        while (Current.Type != TokenType.EndOfFile)
            entities.Add(ParseEntity());
        return new SchemaNode(entities);
    }

    private EntityNode ParseEntity()
    {
        Expect(TokenType.Entity);
        var name = Expect(TokenType.Identifier).Value;
        Expect(TokenType.OpenBrace);

        var attributes = new List<AttributeNode>();
        while (Current.Type != TokenType.CloseBrace)
            attributes.Add(ParseAttribute());

        Expect(TokenType.CloseBrace);
        return new EntityNode(name, attributes);
    }

    private AttributeNode ParseAttribute()
    {
        var attrName = Expect(TokenType.Identifier).Value;
        Expect(TokenType.Colon);
        var typeName = Expect(TokenType.Identifier).Value;

        bool isPrimaryKey = false, isRequired = false, isExposed = false;
        string? defaultValue = null;
        RelationInfo? relation = null;

        while (Current.Type == TokenType.At)
        {
            Advance(); // consume @
            var annotation = Expect(TokenType.Identifier).Value;
            switch (annotation)
            {
                case "primaryKey": isPrimaryKey = true; break;
                case "required": isRequired = true; break;
                case "exposed": isExposed = true; break;
                case "default":
                    Expect(TokenType.OpenParen);
                    defaultValue = Current.Value;
                    Advance();
                    Expect(TokenType.CloseParen);
                    break;
                case "manyToOne":
                    relation = new RelationInfo(RelationType.ManyToOne,
                        ParseOptionalFkArg());
                    break;
                case "oneToMany":
                    relation = new RelationInfo(RelationType.OneToMany,
                        ParseOptionalFkArg());
                    break;
                case "manyToMany":
                    relation =
                        new RelationInfo(RelationType.ManyToMany, null);
                    // consume optional () if present
                    if (Current.Type == TokenType.OpenParen)
                    {
                        Advance();
                        Expect(TokenType.CloseParen);
                    }

                    break;
                default:
                    throw new Exception(
                        $"Unknown annotation '@{annotation}' at line {Current.Line}");
            }
        }

        if (isPrimaryKey) isRequired = true;

        return new AttributeNode(attrName, typeName,
            isPrimaryKey, isRequired, isExposed, defaultValue, relation);
    }

    // Parse optional (fieldName) argument after a relation annotation
    private string? ParseOptionalFkArg()
    {
        if (Current.Type != TokenType.OpenParen) return null;
        Advance();
        var fk = Expect(TokenType.Identifier).Value;
        Expect(TokenType.CloseParen);
        return fk;
    }

    // Helpers 
    private Token Current => tokens[_pos];

    private Token Advance() => tokens[_pos++];

    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
            throw new Exception(
                $"Expected {type} but got {Current.Type} ('{Current.Value}') at line {Current.Line}");
        return Advance();
    }
}