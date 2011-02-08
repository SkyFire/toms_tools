using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace go_types
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
                new Extractor(args[0]);
            else
                new Extractor();
        }
    }

    class Extractor
    {
        string WOW_EXE_NAME = String.Empty;
        string WOW_VERSION = String.Empty;
        string STREAM = String.Empty;
        //long GO_TYPE_INFO_START = 0x4F0A70;
        //long GO_TYPE_INFO_START = 0x4A9B60;
        //long GO_TYPE_INFO_START = 0x5BAB18; // 3.0.3.9095
        //long GO_TYPE_INFO_START = 0x56EF18; // 3.0.3.9183
        long GO_TYPE_INFO_START = 0x5DF1F0; // 3.1.2.9855
        //long OFFSET = 0x401400;
        //long OFFSET = 0x401200;
        //long OFFSET = 0x401000; // 3.0.3.9095
        //long OFFSET = 0x401A00; // 3.0.3.9183 string offset
        long OFFSET = 0x401400; // 3.1.2.9855 string offset
        //long OFFSET2 = 0x401800;
        //long OFFSET2 = 0x402000; // 3.0.3.9183 data offset
        long OFFSET2 = 0x401400;
        //long OFFSET2 = 0x401800; // 3.0.3.9095
        //long GO_DATA_INFO_START = 0x4EF9C8;
        //long GO_DATA_INFO_START = 0x4A8AB8; // 2.4.2.8125
        //long GO_DATA_INFO_START = 0x5B9748; // 3.0.3.9095
        //long GO_DATA_INFO_START = 0x56DB48; // 3.0.3.9183
        long GO_DATA_INFO_START = 0x5DDE60; // 3.1.2.9855

        GenericReader gr;
        List<GameObjectTypeInfo> m_GoTypes = new List<GameObjectTypeInfo>();
        List<List<int>> m_GoData = new List<List<int>>();
        List<GameObjectDataNameInfo> m_GoDataNames = new List<GameObjectDataNameInfo>();

        public Extractor(string file)
        {
            WOW_EXE_NAME = file;

            Init();
        }

        public Extractor()
        {
            WOW_EXE_NAME = "WoW.exe";

            Init();
        }

        void Init()
        {
            if (!File.Exists(WOW_EXE_NAME))
            {
                Console.WriteLine("{0} not found!", WOW_EXE_NAME);
                Console.ReadKey();
                return;
            }

            STREAM = new StreamReader(WOW_EXE_NAME, Encoding.ASCII).ReadToEnd();

            gr = new GenericReader(WOW_EXE_NAME, Encoding.ASCII);

            Console.WriteLine("{0} kb readed to filestream", gr.BaseStream.Length / 1024);

            gr.BaseStream.Position = GO_DATA_INFO_START;

            for (int i = 0; i < 128; ++i)
            {
                GameObjectDataNameInfo info = gr.ReadStruct<GameObjectDataNameInfo>();
                m_GoDataNames.Add(info);
            }

            gr.BaseStream.Position = GO_TYPE_INFO_START;

            for (int i = 0; i < 36; ++i)
            {
                GameObjectTypeInfo info = gr.ReadStruct<GameObjectTypeInfo>();
                m_GoTypes.Add(info);

                m_GoData.Add(new List<int>());

                if (info.DataCount > 0)
                {
                    long pos = gr.BaseStream.Position;
                    gr.BaseStream.Position = info.DataListOffset - OFFSET2;

                    for (int j = 0; j < info.DataCount; ++j)
                    {
                        int dataid = gr.ReadInt32();
                        m_GoData[i].Add(dataid);
                    }

                    gr.BaseStream.Position = pos;
                }
            }

            DumpToFile();

            gr.Close();

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        void DumpToFile()
        {
            StreamWriter sw = new StreamWriter("test.txt");
            sw.WriteLine("enum GameObjectTypes");
            sw.WriteLine("{");
            for (int i = 0; i < m_GoTypes.Count; ++i)
            {
                gr.BaseStream.Position = m_GoTypes[i].NameOffset - OFFSET;
                string name = "    GAMEOBJECT_TYPE_" + gr.ReadStringNull().ToUpper().Replace(" ", "_") + " = 0x" + i.ToString("X2") + ",";
                sw.WriteLine(name);

                for (int j = 0; j < m_GoTypes[i].DataCount; ++j)
                {
                    int idx = m_GoData[i][j];
                    gr.BaseStream.Position = m_GoDataNames[idx].NameOffset - OFFSET;
                    string dataname = gr.ReadStringNull();
                    sw.WriteLine("    //" + dataname);
                }
            }
            sw.WriteLine("};");
            sw.Flush();
            sw.Close();
        }

        void WriteFormat(StreamWriter sw, string name, string delta, string index, string descr, bool end)
        {
            string temp = String.Empty;
            if (end)
            {
                temp = name.Substring(0, name.IndexOf("_")) + "_END";
                if(String.IsNullOrEmpty(delta))
                    sw.WriteLine("    " + zsp(temp, 42, true) + " = " + delta + "0x{0},", index);
                else
                    sw.WriteLine("    " + zsp(temp, 42, true) + " = " + delta + " + 0x{0},", index);
            }
            else
            {
                temp = name;
                if (String.IsNullOrEmpty(delta))
                    sw.WriteLine("    " + zsp(temp, 42, true) + " = " + delta + "0x{0}, // {1}", index, descr);
                else
                    sw.WriteLine("    " + zsp(temp, 42, true) + " = " + delta + " + 0x{0}, // {1}", index, descr);
            }
        }

        private static string zsp(string s, int sp_max, bool size_left)
        {
            string t = String.Empty, result;

            result = s;
            if (s.Length > sp_max)
            {
                return s; // not change
            }

            int length = s.Length;
            for (int i = 1; i < sp_max - length; i++) s += " ";

            if (size_left)
                result = t + s;
            else
                result = s + t;

            return result;
        }
    }
}
