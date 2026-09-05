using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Runtime;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime.Natives
{
    internal interface INativeMembers
    {
        public Value GetMember(Value target, string memberName, Position position);
    }
}
