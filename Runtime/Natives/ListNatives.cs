using System;
using System.Collections.Generic;
using System.Text;
using TOLSharp.Common;
using TOLSharp.Runtime.Values;

namespace TOLSharp.Runtime.Natives
{
    internal class ListNatives : INativeMembers
    {
        public Value GetMember(Value target, string memberName, Position position)
        {
            List<Value> list = target.ListObject.Values;

            switch (memberName)
            {
                case "length":
                    return new Value(list.Count);

                case "isEmpty":
                    return new Value(list.Count == 0);

                case "add":
                    {
                        NativeObject nativeObject = new NativeObject("add");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                list.Add(args[0]);
                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "remove":
                    {
                        NativeObject nativeObject = new NativeObject("remove");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                for (int i = 0; i < list.Count; i++)
                                    if (ValueFunctions.Compare(args[0], list[i]))
                                    {
                                        list.RemoveAt(i);
                                        break;
                                    }

                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "indexOf":
                    {
                        NativeObject nativeObject = new NativeObject("indexOf");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                for (int i = 0; i < list.Count; i++)
                                    if (ValueFunctions.Compare(args[0], list[i]))
                                    {
                                        return new Value(i);
                                    }

                                return new Value(-1);
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "has":
                    {
                        NativeObject nativeObject = new NativeObject("has");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                for (int i = 0; i < list.Count; i++)
                                    if (ValueFunctions.Compare(args[0], list[i]))
                                    {
                                        return new Value(true);
                                    }

                                return new Value(false);
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "join":
                    {
                        NativeObject nativeObject = new NativeObject("join");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                List<Value> other = args[0].Expect(ValueKind.List, "Expected a list for joining", position).ListObject.Values;

                                List<Value> copy = [.. list];

                                copy.AddRange(other);

                                return new Value(copy);
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "extend":
                    {
                        NativeObject nativeObject = new NativeObject("extend");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                List<Value> other = args[0].Expect(ValueKind.List, "Expected a list for extending the other", position).ListObject.Values;

                                list.AddRange(other);

                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "clear":
                    {
                        NativeObject nativeObject = new NativeObject("clear");

                        nativeObject.AddOverload(
                            new NativeOverload(0, (args, pos) =>
                            {
                                list.Clear();
                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "reverse":
                    {
                        NativeObject nativeObject = new NativeObject("reverse");

                        nativeObject.AddOverload(
                            new NativeOverload(0, (args, pos) =>
                            {
                                list.Reverse();
                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "reversed":
                    {
                        NativeObject nativeObject = new NativeObject("reversed");

                        nativeObject.AddOverload(
                            new NativeOverload(0, (args, pos) =>
                            {
                                List<Value> copy = [.. list];

                                copy.Reverse();

                                return new Value(copy);
                            })
                        );

                        return new Value(nativeObject);
                    }


                case "slice":
                    {
                        NativeObject nativeObject = new NativeObject("slice");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                long start = args[0].Expect(ValueKind.Int, "Start index must be an integer", pos).Int;

                                if (start < 0)
                                    throw new Error("Start index cannot be less than zero", pos);

                                if (start >= list.Count)
                                    throw new Error("Start index cannot be more than the list count", pos);

                                return new Value(list[(int)start..]);
                            })
                        );

                        nativeObject.AddOverload(
                            new NativeOverload(2, (args, pos) =>
                            {
                                long start = args[0].Expect(ValueKind.Int, "Start index must be an integer", pos).Int;

                                if (start < 0)
                                    throw new Error("Start index cannot be less than zero", pos);

                                if (start >= list.Count)
                                    throw new Error("Start index cannot be more than the list count", pos);

                                long length = args[1].Expect(ValueKind.Int, "Length must be an integer", pos).Int;

                                if (length < 0)
                                    throw new Error("Length cannot be less than zero", pos);

                                if (start + length > list.Count)
                                    throw new Error("Range exceeds list length", pos);

                                return new Value(list[(int)start..(int)(start + length)]);
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "removeAt":
                    {
                        NativeObject nativeObject = new NativeObject("removeAt");

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                long index = args[0].Expect(ValueKind.Int, "Start index must be an integer", pos).Int;

                                if (index < 0)
                                    throw new Error("Remove index cannot be less than zero", pos);

                                if (index >= list.Count)
                                    throw new Error("Remove index exceeds list length", pos);

                                list.RemoveAt((int)index);

                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "insert":
                    {
                        NativeObject nativeObject = new NativeObject("insert");

                        nativeObject.AddOverload(
                            new NativeOverload(2, (args, pos) =>
                            {
                                long index = args[0].Expect(ValueKind.Int, "Insert index must be an integer", pos).Int;

                                if (index < 0)
                                    throw new Error("Insert index cannot be less than zero", pos);

                                if (index > list.Count)
                                    throw new Error("Insert index exceeds list length", pos);

                                Value value = args[1];

                                list.Insert((int)index, value);

                                return target;
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "first":
                    {
                        NativeObject nativeObject = new NativeObject("first");

                        nativeObject.AddOverload(
                            new NativeOverload(0, (args, pos) =>
                            {
                                if (list.Count == 0)
                                    throw new Error("Cannot get the first item from an empty list", pos);

                                return list[0];
                            })
                        );

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                if (list.Count == 0)
                                    return args[0];

                                return list[0];
                            })
                        );

                        return new Value(nativeObject);
                    }

                case "last":
                    {
                        NativeObject nativeObject = new NativeObject("last");

                        nativeObject.AddOverload(
                            new NativeOverload(0, (args, pos) =>
                            {
                                if (list.Count == 0)
                                    throw new Error("Cannot get the last item from an empty list", pos);

                                return list[^1];
                            })
                        );

                        nativeObject.AddOverload(
                            new NativeOverload(1, (args, pos) =>
                            {
                                if (list.Count == 0)
                                    return args[0];

                                return list[^1];
                            })
                        );

                        return new Value(nativeObject);
                    }
            }


            throw new Error($"list does not contain member '{memberName}'", position);
        }
    }
}
