using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal class KindInfo
    {
        public KindInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
    }

    internal static class ValueKind
    {
        static List<KindInfo> s_types = new List<KindInfo>();
        public static Dictionary<string, int> NameToId = new Dictionary<string, int>();

        // Core
        public static int Int = Register("int");
        public static int Float = Register("float");
        public static int String = Register("string");
        public static int Bool = Register("bool");
        public static int Null = Register("null");
        public static int Action = Register("action");
        public static int Task = Register("task");
        public static int Native = Register("native");
        public static int BoundNative = Register("bound native");
        public static int List = Register("list");

        public static int Register(string name)
        {
            if (NameToId.TryGetValue(name, out int existing))
                return existing;

            int id = s_types.Count;

            s_types.Add(new KindInfo(id, name));
            NameToId[name] = id;

            return id;
        }

        public static KindInfo Get(int id) => s_types[id];
        public static int GetId(string name) => NameToId[name];
        public static string GetName(int id) => Get(id).Name;
    }
}
