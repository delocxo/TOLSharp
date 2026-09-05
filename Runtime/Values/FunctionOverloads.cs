using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Compiler;
using TOLSharp.Runtime;
using TOLSharp.Runtime.Natives;

namespace TOLSharp.Runtime.Values
{
    internal class ActionOverload
    {
        public ActionOverload(string[] parameters, List<Stmt> body, Scope closure)
        {
            Parameters = parameters;
            Body = body;
            Closure = closure;
        }

        public string[] Parameters { get; }
        public List<Stmt> Body { get; }
        public Scope Closure { get; }

        public int Arity => Parameters.Length;
    }

    internal class NativeOverload
    {
        public NativeOverload(int arity, Native native)
        {
            Arity = arity;
            Native = native;
        }

        public int Arity { get; }
        public Native Native { get; }
    }
}
