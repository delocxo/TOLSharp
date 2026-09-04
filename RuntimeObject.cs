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
    internal class ActionObject : RuntimeObject
    {
        public ActionObject(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Dictionary<int, ActionOverload> Overloads { get; } = [];
    }

    internal class TaskObject : RuntimeObject
    {
        public TaskObject(Task<Value> task)
        {
            Task = task;
        }

        public Task<Value> Task { get; }
    }

    internal class NativeObject : RuntimeObject
    {
        public NativeObject(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Dictionary<int, NativeOverload> Overloads { get; } = [];
    }

    internal class ListObject : RuntimeObject
    {
        public ListObject(List<Value> values)
        {
            Values = values;
        }

        public List<Value> Values { get; }

        public int Count => Values.Count;
    }
}
