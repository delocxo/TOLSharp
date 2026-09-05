using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Compiler;
using TOLSharp.Runtime;
using TOLSharp.Runtime.Natives;

namespace TOLSharp.Runtime.Values
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

        public static Value IndexGet(Value target, Value index, Position position)
        {
            if (!target.IsKind(ValueKind.List) && !target.IsKind(ValueKind.String))
                throw new Error($"{target.KindName} cannot be index read", position);

            if (!index.IsKind(ValueKind.Int))
                throw new Error($"{index.KindName} cannot be used an indexer", position);

            long rawIndex = index.Int;

            if (rawIndex < 0)
                throw new Error("Indexer cannot be less than zero", position);

            if (target.IsKind(ValueKind.List))
            {
                ListObject list = target.ListObject;

                if (rawIndex >= list.Count)
                    throw new Error("Indexer is more than or equal to the list count", position);

                return list.Values[(int)rawIndex];
            }
            else
            {
                string str = target.String;

                if (rawIndex >= str.Length)
                    throw new Error("Indexer is more than or equal to the string length", position);

                return new Value(str[(int)rawIndex].ToString());
            }
        }

        public static void IndexSet(Value target, Value index, Value value, Position position)
        {
            if (!target.IsKind(ValueKind.List))
                throw new Error($"{target.KindName} cannot be index set", position);

            if (!index.IsKind(ValueKind.Int))
                throw new Error($"{index.KindName} cannot be used an indexer", position);

            long rawIndex = index.Int;

            if (rawIndex < 0)
                throw new Error("Indexer cannot be less than zero", position);

            if (target.IsKind(ValueKind.List))
            {
                ListObject list = target.ListObject;

                if (rawIndex >= list.Count)
                    throw new Error("Indexer is more than or equal to the list count", position);

                list.Values[(int)rawIndex] = value;
            }
        }

        public static Value GetMember(Value target, string memberName, Position position)
        {
            if (MemberRegistery.Registery.TryGetValue(target.Kind, out var nativeMembers))
                return nativeMembers.GetMember(target, memberName, position);

            throw new Error($"{target.KindName} has no members to accessed", position);
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
