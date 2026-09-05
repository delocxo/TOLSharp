using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using TOLSharp.Common;

namespace TOLSharp.Compiler
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
            if (Check(TokenType.Identifier) && Peek(TokenType.Equal))
                return ParseIdentifier();

            else if (Check(TokenType.If))
                return ParseIf();

            else if (Check(TokenType.While))
                return ParseWhile();

            else if (Check(TokenType.Break))
                return ParseBreak();

            else if (Check(TokenType.Continue))
                return ParseContinue();

            else if (Check(TokenType.Leave))
                return ParseLeave();

            else if (Check(TokenType.Export))
                return ParseExport();

            else if (Check(TokenType.Action))
                return ParseAction();

            return ParseExprStmt();
        }

        VarStmt ParseIdentifier()
        {
            Position position = Current().Position;

            string name = ParseName();

            Expect(TokenType.Equal);

            Expr expr = ParseExpr();

            Expect(TokenType.NewLine);

            return new VarStmt(name, expr, position);
        }

        Stmt ParseExprStmt()
        {
            Expr expr = ParseExpr();

            if (Match(TokenType.Equal))
            {
                Expr value = ParseExpr();

                Expect(TokenType.NewLine);

                if (expr is IndexGetExpr indexGetExpr)
                    return new IndexSetStmt(indexGetExpr, value);

                throw new Error("Invalid assign target", expr.Position);
            }

            Expect(TokenType.NewLine);
            return new ExprStmt(expr);
        }

        IfStmt ParseIf()
        {
            Position position = Current().Position;

            Next();

            SkipNewlines();

            List<IfBranch> branches = new List<IfBranch>();

            Expr condition = ParseExpr();

            Expect(TokenType.NewLine);

            List<Stmt> body = ParseBodyUtil(TokenType.Else, TokenType.ElseIf, TokenType.End);

            branches.Add(new IfBranch(condition, body));

            while (Match(TokenType.ElseIf))
            {
                Expr elseIfCondition = ParseExpr();

                Expect(TokenType.NewLine);

                List<Stmt> elseIfbody = ParseBodyUtil(TokenType.Else, TokenType.ElseIf, TokenType.End);

                branches.Add(new IfBranch(elseIfCondition, elseIfbody));
            }

            List<Stmt>? elseBody = null;

            if (Match(TokenType.Else))
            {
                Expect(TokenType.NewLine);
                elseBody = ParseBodyUtil(TokenType.Else, TokenType.ElseIf, TokenType.End);
            }

            Expect(TokenType.End);

            return new IfStmt(branches, elseBody, position);
        }

        WhileStmt ParseWhile()
        {
            Position position = Current().Position;

            Next();

            SkipNewlines();

            Expr condition = ParseExpr();

            Expect(TokenType.NewLine);

            List<Stmt> body = ParseBodyUtil(TokenType.Else, TokenType.ElseIf, TokenType.End);

            IfBranch branch = new IfBranch(condition, body);

            List<Stmt>? elseBody = null;

            if (Match(TokenType.Else))
            {
                Expect(TokenType.NewLine);
                elseBody = ParseBodyUtil(TokenType.Else, TokenType.ElseIf, TokenType.End);
            }

            Expect(TokenType.End);

            return new WhileStmt(branch, elseBody, position);
        }

        BreakStmt ParseBreak()
        {
            Position position = Current().Position;

            Next();

            Expr? conditon = TryParsePostfixIf();

            return new BreakStmt(conditon, position);
        }

        ContinueStmt ParseContinue()
        {
            Position position = Current().Position;

            Next();

            Expr? conditon = TryParsePostfixIf();

            return new ContinueStmt(conditon, position);
        }


        LeaveStmt ParseLeave()
        {
            Position position = Current().Position;

            Next();

            Expr? conditon = TryParsePostfixIf();

            return new LeaveStmt(conditon, position);
        }

        ExportStmt ParseExport()
        {
            Position position = Current().Position;

            Next();

            if (Match(TokenType.NewLine))
                return new ExportStmt(null, null, position);

            Expr? condition = TryParsePostfixIf();

            if (condition != null)
            {
                Expect(TokenType.NewLine);
                return new ExportStmt(condition, null, position);
            }

            SkipNewlines();

            Expr expr = ParseExpr();

            condition = TryParsePostfixIf();

            Expect(TokenType.NewLine);

            return new ExportStmt(condition, expr, position);
        }

        ActionStmt ParseAction()
        {
            Position position = Current().Position;

            Next();

            // action

            SkipNewlines();

            // action name

            string name = ParseName();

            if (Match(TokenType.NewLine))
            {
                /* action name
                 * 
                 *  end
                */

                List<Stmt> body = ParseBody();
                return new ActionStmt(name, [], body, position);
            }

            SkipNewlines();

            if (Match(TokenType.Arrow))
            {
                SkipNewlines();

                // action name => expr
                
                Expr expr = ParseExpr();

                ExportStmt exportStmt = new ExportStmt(null, expr, position);

                Expect(TokenType.NewLine);

                return new ActionStmt(name, [], [exportStmt], position);
            }

            // action name(...)

            List<string> names = ParseNames(TokenType.LeftParen, TokenType.RightParen);

            if (Match(TokenType.NewLine))
            {
                /* action name(a, b)
                * 
                *  end
               */

                List<Stmt> body = ParseBody();
                return new ActionStmt(name, names, body, position);
            }

            SkipNewlines();

            // action name(a, b) => expr

            Expect(TokenType.Arrow);

            SkipNewlines();

            Expr expr1 = ParseExpr();

            ExportStmt exportStmt1 = new ExportStmt(null, expr1, position);

            Expect(TokenType.NewLine);

            return new ActionStmt(name, names, [exportStmt1], position);
        }

        Expr? TryParsePostfixIf()
        {
            if (Match(TokenType.If))
                return ParseExpr();
            return null;
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

        List<Stmt> ParseBodyUtil(params TokenType[] types)
        {
            List<Stmt> stmts = new List<Stmt>();

            while (true)
            {
                SkipNewlines();

                if (!NotAtEnd())
                    break;

                if (Check(types))
                    break;

                stmts.Add(ParseStmt());
            }

            return stmts;
        }

        List<Stmt> ParseBody()
        {
            List<Stmt> body = ParseBodyUtil(TokenType.End);
            Expect(TokenType.End);
            return body;
        }

        List<string> ParseNames(TokenType end)
        {
            SkipNewlines();

            if (Match(end))
                return new List<string>();

            List<string> names = new List<string>()
            {
                ParseName()
            };

            while (Match(TokenType.Comma))
            {
                names.Add(ParseName());
                SkipNewlines();
            }

            SkipNewlines();
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

            SkipNewlines();

            if (Match(end))
                return new List<Expr>();

            List<Expr> args = new List<Expr>()
            {
                ParseExpr()
            };

            while (Match(TokenType.Comma))
            { 
                args.Add(ParseExpr()); 
                SkipNewlines();
            }

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

        bool Peek(params TokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (PeekNotAtEnd() && _tokens[_i + 1].TokenType == types[i])
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
            else if (Match(TokenType.Spawn))
            {
                SkipNewlines();

                Expr expr = ParseExpr();

                return new SpawnExpr(expr, token.Position);
            }
            else if (Match(TokenType.Await))
            {
                SkipNewlines();

                Expr expr = ParseExpr();

                return new AwaitExpr(expr, token.Position);
            }
            else if (Check(TokenType.LeftBracket))
            {
                SkipNewlines();

                List<Expr> exprs = ParseArgs(TokenType.LeftBracket, TokenType.RightBracket);

                return new ListExpr(exprs, token.Position);
            }

            throw ThrowUnexpected();
        }

        Expr ParsePostfix()
        {
            Expr left = ParsePrimary();

            while (Check(TokenType.LeftParen, TokenType.LeftBracket, TokenType.Period))
            {
                if (Check(TokenType.LeftParen))
                {
                    Position position = left.Position;

                    SkipNewlines();

                    List<Expr> arguments = ParseArgs(TokenType.LeftParen, TokenType.RightParen);

                    left = new CallExpr(left, arguments, position);

                    continue;
                }

                if (Check(TokenType.LeftBracket))
                {
                    Position position = left.Position;

                    Next();

                    SkipNewlines();

                    Expr index = ParseExpr();

                    SkipNewlines();

                    Expect(TokenType.RightBracket);

                    left = new IndexGetExpr(left, index, position);

                    continue;
                }

                if (Check(TokenType.Period))
                {
                    Position position = left.Position;

                    Next();

                    SkipNewlines();

                    string memberName = ParseName();

                    left = new MemberGetExpr(left, memberName, position);

                    continue;
                }

                break;
            }
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

        Expr ParseExpr()
        {
            Expr expr = ParseOr();

            if (Match(TokenType.If))
            {
                SkipNewlines();

                Expr condition = ParseOr();

                Expr? elseExpr = null;

                if (Match(TokenType.Else))
                {
                    SkipNewlines();

                    elseExpr = ParseExpr();
                }

                return new ConditionalExpr(expr, condition, elseExpr, expr.Position);
            }

            return expr;
        }

        void SkipNewlines()
        {
            while (Check(TokenType.NewLine))
                Next();
        }
    }
}
