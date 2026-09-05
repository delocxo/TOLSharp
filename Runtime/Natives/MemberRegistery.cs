using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime.Natives
{
    internal static class MemberRegistery
    {
        public static Dictionary<int, INativeMembers> Registery { get; } = new Dictionary<int, INativeMembers>()
        {
            {
                ValueKind.List,
                new ListNatives()
            }
        };
    }
}
