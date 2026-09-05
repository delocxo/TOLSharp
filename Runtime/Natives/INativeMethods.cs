using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Runtime;

namespace TOLSharp.Runtime.Natives
{
    internal interface INativeMethods
    {
        public void Register(Scope scope, Position? position);
    }
}
