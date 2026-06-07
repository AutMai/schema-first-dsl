namespace SchemaFirst.Generator.Lexer;

public enum TokenType
{
    // Literals
    Identifier,

    // Keywords
    Entity,

    // Symbols
    OpenBrace,
    CloseBrace,
    Colon,
    At,
    OpenParen,
    CloseParen,

    // Values (inside annotations)
    StringValue,
    NumberValue,
    BoolValue,
    Dot,

    // Meta
    EndOfFile
}

public record Token(TokenType Type, string Value, int Line);