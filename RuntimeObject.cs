using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal abstract class RuntimeObject;
    internal class StringObject : RuntimeObject
    {
        public StringObject(string @string)
        {
            String = @string;
        }

        public string String { get; }
    }
}
