using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal delegate Value Native(List<Value> args, Value? target, Position pos);
}
