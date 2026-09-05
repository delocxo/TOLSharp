using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Compiler;
using TOLSharp.Runtime.Natives;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime
{
    internal static class RuntimeDeclarations
    {
        public static void DefineAction(ActionStmt actionStmt, Scope scope)
        {
            ActionOverload overload = new ActionOverload(actionStmt.Parameters.ToArray(), actionStmt.Body, scope);

            if (scope.TryGetLocal(actionStmt.Name, out Value existing))
            {
                if (!existing.IsKind(ValueKind.Action))
                    throw new Error($"Cannot define action '{actionStmt.Name}': name is already used by {existing.KindName}", actionStmt.Position);
                ActionObject action = existing.ActionObject;
                action.Overloads[actionStmt.Arity] = overload;
            }
            else
            {
                ActionObject action = new ActionObject(actionStmt.Name);

                action.Overloads[actionStmt.Arity] = overload;

                scope.Define(actionStmt.Name, new Value(action), actionStmt.Position);
            }
        }

        public static void DefineNative(string name, int arity, Native native, Scope scope, Position? position = null)
        {
            NativeOverload overload = new NativeOverload(arity, native);

            if (scope.TryGetLocal(name, out Value existing))
            {
                if (!existing.IsKind(ValueKind.Native))
                    throw position != null 
                        ? new Error($"Cannot define native '{name}': name is already used by {existing.KindName}", position.Value) 
                        : new InvalidOperationException($"Cannot define native '{name}': name is already used by {existing.KindName}");

                NativeObject nativeObject = existing.NativeObject;

                nativeObject.AddOverload(overload);
                return;
            }

            NativeObject nativeObject1 = new NativeObject(name);

            nativeObject1.AddOverload(overload);

            if (position != null)
                scope.Define(name, new Value(nativeObject1), position.Value);
            else
                scope.DefineNoPosition(name, new Value(nativeObject1));
        }
    }
}
