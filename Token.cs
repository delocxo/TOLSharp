using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal enum TokenType
    {
        String, Int, Float, Identifier,

        True, False, Null, If, Else, ElseIf,
        End, While, Continue, Break, Leave, Exit,
        Export, Action, Await, Spawn,

        Add, Sub, Mul, Div, Mod,
        IsEqual, NotEqual, Less, Greater,
        LessEq, GreaterEq, And, Or, Bang,

        Equal, LeftBracket, RightBracket,
        LeftBrace, RightBrace, LeftParen, RightParen,
        Comma, Period, Hash, Colon, Arrow,

        Eof, Tab, NewLine
    }

    internal record Token(TokenType TokenType, string Lexeme, Position Position);
}
