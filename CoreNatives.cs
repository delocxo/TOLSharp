using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal class CoreNatives : INativeMethods
    {
        public void Register(Scope scope, Position? position)
        {
            RuntimeDeclarations.DefineNative("print", 1, (args, target, pos) =>
            {
                Console.WriteLine(args[0]);
                return Value.Null;
            }, scope, position);

            RuntimeDeclarations.DefineNative("print", 2, (args, target, pos) =>
            {
                Console.Write($"{args[0]}{args[1]}");
                return Value.Null;
            }, scope, position);

            RuntimeDeclarations.DefineNative("typeof", 1, (args, target, pos) =>
            {
                return new Value(args[0].KindName);
            }, scope, position);

            RuntimeDeclarations.DefineNative("input", 0, (args, target, pos) =>
            {
                return new Value(Console.ReadLine() ?? "");
            }, scope, position);

            RuntimeDeclarations.DefineNative("input", 1, (args, target, pos) =>
            {
                Console.Write(args[0]);
                return new Value(Console.ReadLine() ?? "");
            }, scope, position);
        }
    }
}
