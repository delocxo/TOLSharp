using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal static class ListExtensions
    {
        public static void CheckForDuplicates(this IList<string> list, Func<string, string> callback, Position position)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (string name in list)
                if (!names.Add(name))
                    throw new Error(callback(name), position);
        }
    }
}
