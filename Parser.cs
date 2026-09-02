using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;

namespace TOLSharp
{
    internal class Parser
    {
        List<Token> _tokens;
        int _i = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> stmts = new List<Stmt>();

            while (true)
            {
                SkipNewlines();

                if (!NotAtEnd())
                    break;

                stmts.Add(ParseStmt());
            }

            return stmts;
        }

        Stmt ParseStmt()
        {
            if (Check(TokenType.Identifier))
                return ParseIdentifier();
            throw ThrowUnexpected();
        }

        Stmt ParseIdentifier()
        {
            Position position = Current().Position;

            Expr assignee = ParsePostfix();

            Expect(TokenType.Equal);

            Expr expr = ParseExpr();

            Expect(TokenType.NewLine);

            if (assignee is NameExpr nameExpr)
                return new VarStmt(nameExpr.Name, expr, position);

            throw new Error("Invalid assign target", position);
        }

        Error ThrowUnexpected()
        {
            Token token = Current();
            string? keyword = Lexer.GetKeywordFromType(token.TokenType);
            string? symbol = Lexer.GetSymbolFromType(token.TokenType);
            if (keyword != null)
                throw new Error($"Unexpected keyword '{keyword}'", token.Position);
            else if (symbol != null)
                throw new Error($"Unexpected symbol '{symbol}'", token.Position);
            else
                throw new Error($"Unexpected token '{token.TokenType}'", token.Position);
        }

        string ParseName()
        {
            string name = Current().Lexeme;
            Eat("Expected name", TokenType.Identifier);
            return name;
        }

        //List<Stmt> ParseBody()
        //{
        //    List<Stmt> stmts = new List<Stmt>();

        //    if (usesDo)
        //        Expect(TokenType.Do);

        //    while (NotAtEnd() && !Check(TokenType.End))
        //        stmts.Add(ParseStmt());

        //    Expect(TokenType.End);

        //    return stmts;
        //}

        List<string> ParseNames(TokenType end)
        {
            if (Match(end))
                return new List<string>();

            List<string> names = new List<string>()
            {
                ParseName()
            };

            while (Match(TokenType.Comma))
                names.Add(ParseName());

            Expect(end);

            return names;
        }

        List<string> ParseNames(TokenType start, TokenType end)
        {
            Expect(start);
            return ParseNames(end);
        }

        List<Expr> ParseArgs(TokenType start, TokenType end)
        {
            Expect(start);

            if (Match(end))
                return new List<Expr>();

            List<Expr> args = new List<Expr>()
            {
                ParseExpr()
            };

            while (Match(TokenType.Comma))
                args.Add(ParseExpr());

            Expect(end);

            return args;
        }

        bool Check(params TokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (Current().TokenType == types[i])
                    return true;
            return false;
        }

        Token Current() => _tokens[_i];
        Token Peek() => _tokens[_i + 1];
        bool NotAtEnd() => !Check(TokenType.Eof);
        bool PeekNotAtEnd() => _i + 1 < _tokens.Count;
        bool AtEnd() => Check(TokenType.Eof);
        void Next() => _i++;

        void Eat(string message, params TokenType[] types)
        {
            if (Check(types))
            {
                Next();
                return;
            }
            throw new Error(message, Current().Position);
        }

        bool Match(params TokenType[] types)
        {
            if (Check(types))
            {
                Next();
                return true;
            }
            return false;
        }

        void Expect(TokenType type)
        {
            if (Check(type))
            {
                Next();
                return;
            }
            string? keyword = Lexer.GetKeywordFromType(type);
            string? symbol = Lexer.GetSymbolFromType(type);
            if (keyword != null)
                throw new Error($"Expected keyword '{keyword}'", Current().Position);
            else if (symbol != null)
                throw new Error($"Expected symbol '{symbol}'", Current().Position);
            else
                throw new Error($"Expected token '{type}'", Current().Position);
        }

        Expr ParsePrimary()
        {
            Token token = Current();

            if (Match(TokenType.Int))
            {
                if (long.TryParse(token.Lexeme, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longResult))
                    return new IntExpr(longResult, token.Position);

                if (double.TryParse(token.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
                    return new FloatExpr(doubleResult, token.Position);

                throw new Error("Failed to cast lexeme into an integer", token.Position);
            }
            else if (Match(TokenType.Float))
            {
                if (double.TryParse(token.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
                    return new FloatExpr(doubleResult, token.Position);

                throw new Error("Failed to cast lexeme into a float", token.Position);
            }
            else if (Match(TokenType.String))
                return new StringExpr(token.Lexeme, token.Position);
            else if (Match(TokenType.True))
                return new BoolExpr(true, token.Position);
            else if (Match(TokenType.False))
                return new BoolExpr(false, token.Position);
            else if (Match(TokenType.Null))
                return new NullExpr(token.Position);
            else if (Match(TokenType.Identifier))
            {
                return new NameExpr(token.Lexeme, token.Position);
            }
            else if (Match(TokenType.LeftParen))
            {
                SkipNewlines();

                Expr expr = ParseExpr();

                SkipNewlines();

                Expect(TokenType.RightParen);
                return expr;
            }

            throw new Error("Invalid expression", token.Position);
        }

        Expr ParsePostfix()
        {
            Expr left = ParsePrimary();
            //while (Check(TokenType.LeftParen, TokenType.LeftBracket, TokenType.Period))
            //{
            //    break;
            //}
            return left;
        }

        Expr ParseUnary()
        {
            if (Check(TokenType.Sub, TokenType.Bang))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseUnary();

                return new UnaryExpr(right, op.TokenType, op.Position);
            }
            return ParsePostfix();
        }

        Expr ParseTerm()
        {
            Expr left = ParseUnary();

            while (Check(TokenType.Mul, TokenType.Div, TokenType.Mod))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseUnary();

                left = new BinaryExpr(left, right, op.TokenType, op.Position);
            }

            return left;
        }

        Expr ParseFactor()
        {
            Expr left = ParseTerm();

            while (Check(TokenType.Add, TokenType.Sub))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseTerm();

                left = new BinaryExpr(left, right, op.TokenType, op.Position);
            }

            return left;
        }

        Expr ParseComparison()
        {
            Expr left = ParseFactor();

            while (Check(TokenType.Less, TokenType.Greater, TokenType.LessEq, TokenType.GreaterEq))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseFactor();

                left = new BinaryExpr(left, right, op.TokenType, op.Position);
            }

            return left;
        }

        Expr ParseEquality()
        {
            Expr left = ParseComparison();

            while (Check(TokenType.NotEqual, TokenType.IsEqual))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseComparison();

                left = new BinaryExpr(left, right, op.TokenType, op.Position);
            }

            return left;
        }

        Expr ParseAnd()
        {
            Expr left = ParseEquality();

            while (Check(TokenType.And))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseEquality();

                left = new BinaryExpr(left, right, op.TokenType, op.Position);
            }

            return left;
        }

        Expr ParseOr()
        {
            Expr left = ParseAnd();

            while (Check(TokenType.Or))
            {
                Token op = Current();

                Next();
                SkipNewlines();

                Expr right = ParseAnd();

                left = new BinaryExpr(left, right, op.TokenType, op.Position);
            }

            return left;
        }

        Expr ParseExpr() => ParseOr();

        void SkipNewlines()
        {
            while (Check(TokenType.NewLine))
                Next();
        }
    }
}
