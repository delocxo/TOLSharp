using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime.Natives
{
    internal delegate Value Native(List<Value> args, Position pos);
}
