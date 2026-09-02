using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal abstract class Expr
    {
        protected Expr(Position position)
        {
            Position = position;
        }

        public Position Position { get; }
    }

    internal class IntExpr : Expr
    {
        public IntExpr(long value, Position position) : base(position)
        {
            Value = value;
        }

        public long Value { get; }
    }

    internal class FloatExpr : Expr
    {
        public FloatExpr(double value, Position position) : base(position)
        {
            Value = value;
        }

        public double Value { get; }
    }

    internal class StringExpr : Expr
    {
        public StringExpr(string value, Position position) : base(position)
        {
            Value = value;
        }

        public string Value { get; }
    }

    internal class BoolExpr : Expr
    {
        public BoolExpr(bool value, Position position) : base(position)
        {
            Value = value;
        }

        public bool Value { get; }
    }

    internal class NullExpr : Expr
    {
        public NullExpr(Position position) : base(position)
        {
        }
    }

    internal class NameExpr : Expr
    {
        public NameExpr(string name, Position position) : base(position)
        {
            Name = name;
        }

        public string Name { get; }
    }

    internal class UnaryExpr : Expr
    {
        public UnaryExpr(Expr right, TokenType op, Position position) : base(position)
        {
            Right = right;
            Op = op;
        }

        public Expr Right { get; }
        public TokenType Op { get; }
    }

    internal class BinaryExpr : Expr
    {
        public BinaryExpr(Expr left, Expr right, TokenType op, Position position) : base(position)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        public Expr Left { get; }
        public Expr Right { get; }
        public TokenType Op { get; }
    }

    internal abstract class Stmt
    {
        public Stmt(Position position, bool allowedInImport)
        {
            Position = position;
            AllowedInImport = allowedInImport;
        }

        public Position Position { get; }
        public bool AllowedInImport { get; }
    }

    internal class VarStmt : Stmt
    {
        public VarStmt(string name, Expr expr, Position position) : base(position, false) 
        {
            Name = name;
            Expr = expr;
        }

        public string Name { get; }
        public Expr Expr { get; }
    }
}
