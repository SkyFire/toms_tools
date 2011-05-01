using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WoWPacketViewer.Parsers;
using WowTools.Core;

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

        public static void ReInit()
        {
            Parsers.Clear();
            Init();
        }

        private static void Init()
        {
            LoadAssembly(Assembly.GetCallingAssembly());

            if (Directory.Exists("parsers"))
            {
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

                var extensions = new string[] { "*.cs", "*.vb", "*.js" };

                foreach (var ext in extensions)
                {
                    foreach (string file in Directory.GetFiles("parsers", ext, SearchOption.AllDirectories))
                    {
                        try
                        {
                            Assembly assembly = CompileParser(file);
                            LoadAssembly(assembly);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private static string GetLanguageFromExtension(string file)
        {
            return CodeDomProvider.GetLanguageFromExtension(Path.GetExtension(file));
        }

        private static Assembly CompileParser(string file)
        {
            using (CodeDomProvider provider = CodeDomProvider.CreateProvider(GetLanguageFromExtension(file)))
            {
                CompilerParameters cp = new CompilerParameters();

                cp.GenerateInMemory = true;
                cp.TreatWarningsAsErrors = false;
                cp.GenerateExecutable = false;
                cp.ReferencedAssemblies.Add("WowTools.Core.dll");

                CompilerResults cr = provider.CompileAssemblyFromFile(cp, file);

                if (cr.Errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Errors building {0}", file).AppendLine();
                    foreach (CompilerError ce in cr.Errors)
                    {
                        sb.AppendFormat("  {0}", ce.ToString()).AppendLine();
                    }
                    MessageBox.Show(sb.ToString());
                }

                return cr.CompiledAssembly;
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
