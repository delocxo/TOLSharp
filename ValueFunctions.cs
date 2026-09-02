using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal static class ValueFunctions
    {
        public static bool Compare(Value left, Value right)
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.Kind == ValueKind.Float || right.Kind == ValueKind.Float)
                    return left.AsFloat() == right.AsFloat();

                return left.Int == right.Int;
            }

            if (left.Kind == ValueKind.Bool && right.Kind == ValueKind.Bool)
                return left.Bool == right.Bool;

            if (left.Kind == ValueKind.String && right.Kind == ValueKind.String)
                return left.String == right.String;

            if (left.Kind == ValueKind.Null && right.Kind == ValueKind.Null)
                return true;

            return false;
        }

        public static bool IsTruthy(Value value)
        {
            if (value.Kind == ValueKind.Bool)
                return value.Bool;

            if (value.Kind == ValueKind.Null)
                return false;

            return true;
        }
    }
}
