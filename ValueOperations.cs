using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal static class ValueOperations
    {
        public static Value Add(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() + right.AsFloat());

                try
                {
                    return new Value(checked(left.Int + right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(left.AsFloat() + right.AsFloat());
                }
            }

            if (left.IsKind(ValueKind.String) || right.IsKind(ValueKind.String))
                return new Value(left.ToString() + right.ToString());

            throw BinaryError(left, right, "+", position);
        }

        public static Value Sub(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() - right.AsFloat());

                try
                {
                    return new Value(checked(left.Int - right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(left.AsFloat() - right.AsFloat());
                }
            }

            throw BinaryError(left, right, "-", position);
        }

        public static Value Mul(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() * right.AsFloat());

                try
                {
                    return new Value(checked(left.Int * right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(left.AsFloat() * right.AsFloat());
                }
            }

            throw BinaryError(left, right, "*", position);
        }

        public static Value Div(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (right.AsFloat() == 0)
                    throw new Error("Division by zero", position);

                return new Value(left.AsFloat() / right.AsFloat());
            }

            throw BinaryError(left, right, "/", position);
        }

        public static Value Mod(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (right.AsFloat() == 0)
                    throw new Error("Modulo by zero", position);

                return new Value(left.AsFloat() % right.AsFloat());
            }

            throw BinaryError(left, right, "%", position);
        }

        public static Value Less(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() < right.AsFloat());

                return new Value(left.Int < right.Int);
            }

            throw BinaryError(left, right, "<", position);
        }

        public static Value Greater(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() > right.AsFloat());

                return new Value(left.Int > right.Int);
            }

            throw BinaryError(left, right, ">", position);
        }

        public static Value LessEqual(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() <= right.AsFloat());

                return new Value(left.Int <= right.Int);
            }

            throw BinaryError(left, right, "<=", position);
        }

        public static Value GreaterEqual(Value left, Value right, Position position)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsKind(ValueKind.Float) || right.IsKind(ValueKind.Float))
                    return new Value(left.AsFloat() >= right.AsFloat());

                return new Value(left.Int >= right.Int);
            }

            throw BinaryError(left, right, ">=", position);
        }

        public static Value Equals(Value left, Value right, Position position)
        {
            return new Value(ValueFunctions.Compare(left, right));
        }

        public static Value NotEqual(Value left, Value right, Position position)
        {
            return new Value(!ValueFunctions.Compare(left, right));
        }

        public static Value Negate(Value right, Position position)
        {
            if (right.IsKind(ValueKind.Float))
                return new Value(-right.Float);

            if (right.IsKind(ValueKind.Int))
            {
                try
                {
                    return new Value(checked(-right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(-right.AsFloat());
                }
            }

            throw UnaryError(right, "-", position);
        }

        public static Value Flip(Value right, Position position)
        {
            return new Value(!ValueFunctions.IsTruthy(right));
        }

        public static Value LogicalAnd(Expr leftExpr, Expr rightExpr, Scope scope)
        {
            Value left = Evaluator.Evaluate(leftExpr, scope);

            if (!ValueFunctions.IsTruthy(left))
                return left;

            return Evaluator.Evaluate(rightExpr, scope);
        }

        public static Value LogicalOr(Expr leftExpr, Expr rightExpr, Scope scope)
        {
            Value left = Evaluator.Evaluate(leftExpr, scope);

            if (ValueFunctions.IsTruthy(left))
                return left;

            return Evaluator.Evaluate(rightExpr, scope);
        }

        static Error UnaryError(Value value, string @operator, Position position)
        {
            string kindName = ValueKind.GetName(value.Kind);
            return new Error($"Cannot apply '{@operator}' to {kindName}", position);
        }

        static Error BinaryError(Value left, Value right, string @operator, Position position)
        {
            string leftName = ValueKind.GetName(left.Kind);
            string rightName = ValueKind.GetName(right.Kind);
            return new Error($"Cannot apply '{@operator}' to {leftName} and {rightName}", position);
        }
    }
}
