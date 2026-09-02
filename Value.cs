using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace TOLSharp
{
    internal struct Value
    {
        public int Kind { get; private set; }
        public long Int { get; }
        public double Float { get; }
        public bool Bool { get; set; }
        public RuntimeObject? RuntimeObject { get; private set; }
        public string String => (RuntimeObject as StringObject)!.String;

        public Value(long @int)
        {
            Kind = ValueKind.Int;
            Int = @int;
        }

        public Value(double @float)
        {
            Kind = ValueKind.Float;
            Float = @float;
        }

        public Value(bool @bool)
        {
            Kind = ValueKind.Bool;
            Bool = @bool;
        }

        public Value(string @string)
        {
            Kind = ValueKind.String;
            RuntimeObject = new StringObject(@string);
        }

        public static Value Null => new Value
        {
            Kind = ValueKind.Null
        };

        public bool IsNumber() => Kind == ValueKind.Int || Kind == ValueKind.Float;

        public double AsFloat() => Kind == ValueKind.Int ? Int : Float;

        public bool IsKind(int kind) => Kind == kind;

        public override string ToString()
        {
            if (IsKind(ValueKind.Int))
                return Int.ToString(CultureInfo.InvariantCulture);

            else if (IsKind(ValueKind.Float))
                return Float.ToString(CultureInfo.InvariantCulture);

            else if (IsKind(ValueKind.Bool))
                return Bool ? "true" : "false";

            else if (IsKind(ValueKind.String))
                return String;

            else if (IsKind(ValueKind.Null))
                return "null";

            return "INVALID TYPE";
        }

        public string ToStringWithQuotes()
        {
            if (IsKind(ValueKind.String))
                return $"'{String}'";
            return ToString();
        }
    }
}
