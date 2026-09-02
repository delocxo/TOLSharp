using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TOLSharp
{
    internal static class Evaluator
    {
        public static Value Evaluate(Expr expr, Scope scope)
        {
            if (expr is IntExpr intExpr)
                return new Value(intExpr.Value);

            else if (expr is FloatExpr floatExpr)
                return new Value(floatExpr.Value);

            else if (expr is StringExpr stringExpr)
                return new Value(stringExpr.Value);

            else if (expr is BoolExpr boolExpr)
                return new Value(boolExpr.Value);

            else if (expr is NullExpr)
                return Value.Null;

            else if (expr is NameExpr nameExpr)
                return scope.Get(nameExpr.Name, nameExpr.Position);

            else if (expr is UnaryExpr unaryExpr)
            {
                if (unaryExpr.Op == TokenType.Sub)
                    return ValueOperations.Negate(Evaluate(unaryExpr.Right, scope), unaryExpr.Position);

                else
                    return ValueOperations.Flip(Evaluate(unaryExpr.Right, scope), unaryExpr.Position);
            }

            else if (expr is BinaryExpr binaryExpr)
            {
                if (binaryExpr.Op == TokenType.And)
                    return ValueOperations.LogicalAnd(binaryExpr.Left, binaryExpr.Right, scope);

                else if (binaryExpr.Op == TokenType.Or)
                    return ValueOperations.LogicalOr(binaryExpr.Left, binaryExpr.Right, scope);

                Value left = Evaluate(binaryExpr.Left, scope);
                Value right = Evaluate(binaryExpr.Right, scope);

                Func<Value, Value, Position, Value> method = binaryExpr.Op switch
                {
                    TokenType.Add => ValueOperations.Add,
                    TokenType.Sub => ValueOperations.Sub,
                    TokenType.Mul => ValueOperations.Mul,
                    TokenType.Div => ValueOperations.Div,
                    TokenType.Mod => ValueOperations.Mod,
                    TokenType.Less => ValueOperations.Less,
                    TokenType.Greater => ValueOperations.Greater,
                    TokenType.LessEq => ValueOperations.LessEqual,
                    TokenType.GreaterEq => ValueOperations.GreaterEqual,
                    TokenType.IsEqual => ValueOperations.Equals,
                    TokenType.NotEqual => ValueOperations.NotEqual,
                    _ => throw new UnreachableException(),
                };

                return method(left, right, binaryExpr.Position);
            }

            throw new UnreachableException();
        }

        public static bool ExprIsTruthy(Expr expr, Scope scope)
        {
            Value value = Evaluate(expr, scope);
            return ValueFunctions.IsTruthy(value);
        }
    }
}
