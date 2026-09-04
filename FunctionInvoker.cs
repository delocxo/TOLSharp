using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal static class FunctionInvoker
    {
        public static Value Invoke(List<Expr> arguments, Expr callee, Scope scope, Position position)
        {
            List<Value> values = arguments
                .Select(x => Evaluator.Evaluate(x, scope))
                .ToList();

            Value target = Evaluator.Evaluate(callee, scope);

            if (target.IsKind(ValueKind.Action))
            {
                ActionObject actionObject = target.ActionObject;

                if (!actionObject.Overloads.TryGetValue(values.Count, out var actionOverload))
                    throw new Error($"No overload of '{actionObject.Name}' takes {values.Count} argument(s)", position);

                Scope scope1 = new Scope(actionOverload.Closure);

                for (int i = 0; i < values.Count; i++)
                    scope1.Define(actionOverload.Parameters[i], values[i], position);
                
                try
                {
                    Intepreter.Execute(actionOverload.Body, scope1);
                }
                catch (ExportSignal export)
                {
                    return export.Value;
                }

                return Value.Null;
            }
            else if (target.IsKind(ValueKind.Native))
            {
                NativeObject nativeObject = target.NativeObject;

                if (!nativeObject.Overloads.TryGetValue(values.Count, out var actionOverload))
                    throw new Error($"No overload of '{nativeObject.Name}' takes {values.Count} argument(s)", position);

                return actionOverload.Native(values, null, position);
            }

            throw new Error($"{target.KindName} not callable", position);
        }
    }
}
