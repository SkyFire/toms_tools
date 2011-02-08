using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WoWPacketViewer.Parsers;

namespace WoWPacketViewer
{
    public class ParserFactory
    {
        private static readonly Dictionary<int, Type> Parsers = new Dictionary<int, Type>();
        private static readonly Parser UnknownParser = new UnknownPacketParser();

        static ParserFactory()
        {
            Init();
        }

        private static void Init()
        {
            LoadAssembly(Assembly.GetCallingAssembly());
            if (!Directory.Exists("parsers")) return;
            foreach (string file in Directory.GetFiles("parsers", "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(Path.GetFullPath(file));
                    LoadAssembly(assembly);
                }
                catch
                {
                }
            }
        }

        private static void LoadAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Parser)))
                {
                    var attributes = (ParserAttribute[])type.GetCustomAttributes(typeof(ParserAttribute), true);
                    foreach (ParserAttribute attribute in attributes)
                    {
                        Parsers[(int)attribute.Code] = type;
                    }
                }
            }
        }

        public static Parser CreateParser(Packet packet)
        {
            Type type;
            if (!Parsers.TryGetValue((int)packet.Code, out type))
            {
                return UnknownParser;
            }
            return (Parser)Activator.CreateInstance(type, packet);
        }
    }
}
