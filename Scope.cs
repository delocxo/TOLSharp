using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal class Scope
    {
        Dictionary<string, Value> _locals = new Dictionary<string, Value>();
        Scope? _parent;

        bool AtTopLevel => _parent == null;

        public Scope(Scope? parent = null)
        {
            _parent = parent;
        }

        public void Define(string name, Value value)
        {
            _locals[name] = value;
        }

        public Value Get(string name, Position position)
        {
            if (_locals.TryGetValue(name, out Value value))
                return value;

            if (_parent != null)
                return _parent.Get(name, position);

            throw new Error($"'{name}' is not defined", position);
        }
    }
}
