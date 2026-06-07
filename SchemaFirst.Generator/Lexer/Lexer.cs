namespace SchemaFirst.Generator.Lexer;

public class Lexer(string source)
{
    private int _pos;
    private int _line = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["entity"] = TokenType.Entity,
        ["true"] = TokenType.BoolValue,
        ["false"] = TokenType.BoolValue,
    };

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_pos < source.Length)
        {
            SkipWhitespaceAndComments();
            if (_pos >= source.Length) break;

            var ch = source[_pos];
            tokens.Add(ch switch
            {
                '{' => Consume(TokenType.OpenBrace),
                '}' => Consume(TokenType.CloseBrace),
                ':' => Consume(TokenType.Colon),
                '@' => Consume(TokenType.At),
                '(' => Consume(TokenType.OpenParen),
                ')' => Consume(TokenType.CloseParen),
                '.' => Consume(TokenType.Dot),
                '"' => ReadString(),
                _ => char.IsLetter(ch) || ch == '_'
                    ? ReadIdentifierOrKeyword()
                    : char.IsDigit(ch) || ch == '-'
                        ? ReadNumber()
                        : throw new Exception(
                            $"Unexpected character '{ch}' at line {_line}")
            });
        }

        tokens.Add(new Token(TokenType.EndOfFile, "", _line));
        return tokens;
    }

    private Token Consume(TokenType type)
    {
        var t = new Token(type, source[_pos].ToString(), _line);
        _pos++;
        return t;
    }

    private Token ReadIdentifierOrKeyword()
    {
        var start = _pos;
        while (_pos < source.Length &&
               (char.IsLetterOrDigit(source[_pos]) || source[_pos] == '_'))
            _pos++;
        var value = source[start.._pos];
        var type = Keywords.GetValueOrDefault(value, TokenType.Identifier);
        return new Token(type, value, _line);
    }

    private Token ReadNumber()
    {
        var start = _pos;
        if (source[_pos] == '-') _pos++;
        while (_pos < source.Length &&
               (char.IsDigit(source[_pos]) || source[_pos] == '.'))
            _pos++;
        return new Token(TokenType.NumberValue, source[start.._pos],
            _line);
    }

    private Token ReadString()
    {
        _pos++; // skip opening "
        var start = _pos;
        while (_pos < source.Length && source[_pos] != '"')
            _pos++;
        var value = source[start.._pos];
        _pos++; // skip closing "
        return new Token(TokenType.StringValue, value, _line);
    }

    private void SkipWhitespaceAndComments()
    {
        while (_pos < source.Length)
        {
            if (source[_pos] == '\n')
            {
                _line++;
                _pos++;
            }
            else if (char.IsWhiteSpace(source[_pos])) _pos++;
            else if (_pos + 1 < source.Length && source[_pos] == '/' &&
                     source[_pos + 1] == '/')
            {
                while (_pos < source.Length && source[_pos] != '\n')
                    _pos++;
            }
            else break;
        }
    }
}