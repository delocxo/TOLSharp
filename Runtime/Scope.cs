using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime
{
    internal class Scope
    {
        public Dictionary<string, Value> Locals { get; } = new Dictionary<string, Value>();
        Scope? _parent;

        bool AtTopLevel => _parent == null;

        public Scope(Scope? parent = null)
        {
            _parent = parent;
        }

        public void Define(string name, Value value, Position position)
        {
            if (TryGetLocal(name, out Value value1))
                VerfiyName(name, value1, position);
            Locals[name] = value;
        }

        public void DefineNoPosition(string name, Value value)
        {
            if (TryGetLocal(name, out Value value1))
                VerfiyName(name, value1, null);
            Locals[name] = value;
        }

        public Value Get(string name, Position position)
        {
            if (Locals.TryGetValue(name, out Value value))
                return value;

            if (_parent != null)
                return _parent.Get(name, position);

            throw new Error($"'{name}' is not defined", position);
        }

        public bool TryGetLocal(string name, out Value value) => Locals.TryGetValue(name, out value);

        void VerfiyName(string name, Value value, Position? position)
        {
            if (value.IsKind(ValueKind.Action))
                throw position != null 
                    ? new Error($"Cannot redefine action '{name}'", position!.Value) 
                    : new InvalidOperationException($"Cannot redefine action '{name}'");
        }
    }
}
