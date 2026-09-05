using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;

namespace TOLSharp.Compiler
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

    internal class CallExpr : Expr
    {
        public CallExpr(Expr callee, List<Expr> arguments, Position position) : base(position)
        {
            Callee = callee;
            Arguments = arguments;
        }

        public Expr Callee { get; }
        public List<Expr> Arguments { get; }
    }

    internal class SpawnExpr : Expr
    {
        public SpawnExpr(Expr expr, Position position) : base(position)
        {
            Expr = expr;
        }

        public Expr Expr { get; }
    }

    internal class AwaitExpr : Expr
    {
        public AwaitExpr(Expr expr, Position position) : base(position)
        {
            Expr = expr;
        }

        public Expr Expr { get; }
    }

    internal class ConditionalExpr : Expr
    {
        public ConditionalExpr(Expr expr, Expr condition, Expr? @else, Position position) : base(position)
        {
            Expr = expr;
            Condition = condition;
            Else = @else;
        }

        public Expr Expr { get; }
        public Expr Condition { get; }
        public Expr? Else { get; }
    }

    internal class ListExpr : Expr
    {
        public ListExpr(List<Expr> exprs, Position position) : base(position)
        {
            Exprs = exprs;
        }

        public List<Expr> Exprs { get; }
    }

    internal class IndexGetExpr : Expr
    {
        public IndexGetExpr(Expr indexee, Expr index, Position position) : base(position)
        {
            Indexee = indexee;
            Index = index;
        }

        public Expr Indexee { get; }
        public Expr Index { get; }
    }

    internal class MemberGetExpr : Expr
    {
        public MemberGetExpr(Expr target, string memberName, Position position) : base(position)
        {
            Target = target;
            MemberName = memberName;
        }

        public Expr Target { get; }
        public string MemberName { get; }
    }

    internal class IfBranch
    {
        public IfBranch(Expr expr, List<Stmt> body)
        {
            Expr = expr;
            Body = body;
        }

        public Expr Expr { get; }
        public List<Stmt> Body { get; }
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

    internal class IfStmt : Stmt
    {
        public IfStmt(List<IfBranch> branches, List<Stmt>? elseBody, Position position) : base(position, false)
        {
            Branches = branches;
            ElseBody = elseBody;
        }

        public List<IfBranch> Branches { get; }
        public List<Stmt>? ElseBody { get; }
    }

    internal class WhileStmt : Stmt
    {
        public WhileStmt(IfBranch branch, List<Stmt>? elseBody, Position position) : base(position, false)
        {
            Branch = branch;
            ElseBody = elseBody;
        }

        public IfBranch Branch { get; }
        public List<Stmt>? ElseBody { get; }
    }

    internal class BreakStmt : Stmt
    {
        public BreakStmt(Expr? condition, Position position) : base(position, false)
        {
            Condition = condition;
        }

        public Expr? Condition { get; }
    }

    internal class ContinueStmt : Stmt
    {
        public ContinueStmt(Expr? condition, Position position) : base(position, false)
        {
            Condition = condition;
        }

        public Expr? Condition { get; }
    }

    internal class LeaveStmt : Stmt
    {
        public LeaveStmt(Expr? condition, Position position) : base(position, false)
        {
            Condition = condition;
        }

        public Expr? Condition { get; }
    } 

    internal class ExportStmt : Stmt
    {
        public ExportStmt(Expr? condition, Expr? expr, Position position) : base(position, false)
        {
            Condition = condition;
            Expr = expr;
        }

        public Expr? Condition { get; }
        public Expr? Expr { get; }
    }

    internal class ActionStmt : Stmt
    {
        public ActionStmt(string name, List<string> parameters, List<Stmt> body, Position position) : base(position, true)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public string Name { get;}
        public List<string> Parameters { get; }
        public List<Stmt> Body { get; }

        public int Arity => Parameters.Count;
    }

    internal class ExprStmt : Stmt
    {
        public ExprStmt(Expr expr) : base(expr.Position, false)
        {
            Expr = expr;
        }

        public Expr Expr { get; }
    }

    internal class IndexSetStmt : Stmt
    {
        public IndexSetStmt(IndexGetExpr indexGetExpr, Expr expr) : base(indexGetExpr.Position, false)
        {
            IndexGetExpr = indexGetExpr;
            Expr = expr;
        }

        public IndexGetExpr IndexGetExpr { get; }
        public Expr Expr { get; }
    }
}
