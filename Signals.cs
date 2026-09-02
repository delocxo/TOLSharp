using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal class BreakSignal : Exception;
    internal class ContinueSignal : Exception;
    internal class LeaveSignal : Exception;
    internal class ReturnSignal : Exception
    {
        public ReturnSignal(Value? value)
        {
            Value = value;
        }

        public Value? Value { get; }
    }
}
