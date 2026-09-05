using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using TOLSharp.Common;

namespace TOLSharp.Runtime.Values
{
    internal struct Value
    {
        public int Kind { get; private set; }
        public long Int { get; }
        public double Float { get; }
        public bool Bool { get; set; }
        public RuntimeObject? RuntimeObject { get; private set; }
        public string String => (RuntimeObject as StringObject)!.String;
        public ActionObject ActionObject => (RuntimeObject as ActionObject)!;
        public Task<Value> Task => (RuntimeObject as TaskObject)!.Task;
        public NativeObject NativeObject => (RuntimeObject as NativeObject)!;
        public ListObject ListObject => (RuntimeObject as ListObject)!;

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

        public Value(ActionObject actionObject)
        {
            Kind = ValueKind.Action;
            RuntimeObject = actionObject;
        }

        public Value(Task<Value> task)
        {
            Kind = ValueKind.Task;
            RuntimeObject = new TaskObject(task);
        }

        public Value(NativeObject nativeObject)
        {
            Kind = ValueKind.Native;
            RuntimeObject = nativeObject;
        }

        public Value(List<Value> values)
        {
            Kind = ValueKind.List;
            RuntimeObject = new ListObject(values);
        }

        public static Value Null => new Value
        {
            Kind = ValueKind.Null
        };

        public string KindName => ValueKind.GetName(Kind);

        public bool IsNumber() => Kind == ValueKind.Int || Kind == ValueKind.Float;

        public double AsFloat() => Kind == ValueKind.Int ? Int : Float;

        public bool IsKind(int kind) => Kind == kind;

        public Value Expect(int kind, string message, Position position)
        {
            if (!IsKind(kind))
                throw new Error(message, position);

            return this;
        }

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

            else if (IsKind(ValueKind.Action))
                return $"<action {ActionObject.Name}>";

            else if (IsKind(ValueKind.Task))
                return $"<task {Task.Status.ToString().ToLower()}>";

            else if (IsKind(ValueKind.Native))
                return $"<native {NativeObject.Name}>";

            else if (IsKind(ValueKind.List))
                return $"[{string.Join(", ", ListObject.Values.Select(x => x.ToStringWithQuotes()))}]";

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
