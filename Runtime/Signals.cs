using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime
{
    internal class BreakSignal : Exception;
    internal class ContinueSignal : Exception;
    internal class LeaveSignal : Exception;
    internal class ExportSignal : Exception
    {
        public ExportSignal(Value value)
        {
            Value = value;
        }

        public Value Value { get; }
    }
}
