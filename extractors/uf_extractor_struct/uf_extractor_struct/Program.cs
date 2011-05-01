using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace uf_extractor_struct
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
        long FIELDS_INFO_START = 0;
        long FIELDS_NAMES_START = 0;
        long FIELDS_NAMES_OFFSET = 0;

        GenericReader gr;
        List<UpdateField> list = new List<UpdateField>();

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

            if (!GetWoWVersion())
                return;

            if (!GetFieldsNamesStartPos())
                return;

            if (!GetFieldsInfoStartPos())
                return;

            GetStringsOffset();
            FillList();

            gr.Close();

            DumpToFile();

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        bool GetWoWVersion()
        {
            string dir = Directory.GetCurrentDirectory();
            string file = dir + "\\" + WOW_EXE_NAME;

            if (!File.Exists(file))
                return false;

            WOW_VERSION = FileVersionInfo.GetVersionInfo(dir + "\\" + WOW_EXE_NAME).FileVersion;

            Console.WriteLine("Client version {0}", WOW_VERSION);

            return true;
        }

        bool GetFieldsNamesStartPos()
        {
            int i = STREAM.IndexOf("OBJECT_FIELD_GUID" + (char)0);
            if (i > 0)
            {
                FIELDS_NAMES_START = i;

                Console.WriteLine("Fields names start position found at 0x{0}", FIELDS_NAMES_START.ToString("X8"));

                return true;
            }
            Console.WriteLine("Fields names start position not found");
            return false;
        }

        bool GetFieldsInfoStartPos()
        {
            string sstr = String.Empty;
            byte[] temp = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };

            for (int n = 0; n < temp.Length; ++n)
                sstr += (char)temp[n];

            int i = STREAM.IndexOf(sstr);
            if (i > 0)
            {
                FIELDS_INFO_START = i - 4;

                Console.WriteLine("Found fields info start position at 0x{0}", FIELDS_INFO_START.ToString("X8"));

                return true;
            }
            Console.WriteLine("Fields info start position not found");
            return false;
        }

        void GetStringsOffset()
        {
            long oldpos = gr.BaseStream.Position;
            gr.BaseStream.Position = FIELDS_INFO_START;
            long temp = gr.ReadInt64();
            gr.BaseStream.Position = oldpos;
            FIELDS_NAMES_OFFSET = temp - FIELDS_NAMES_START;
            Console.WriteLine("str_ofs_delta = temp - start_str_ofs = 0x{0} - 0x{1} = 0x{2}", temp.ToString("X8"), FIELDS_NAMES_START.ToString("X8"), FIELDS_NAMES_OFFSET.ToString("X8"));
        }

        void FillList()
        {
            uint val1, pos, total;
            UpdateFieldType type;
            UpdatafieldFlags flags;
            string cur = String.Empty, old = String.Empty;

            gr.BaseStream.Position = FIELDS_INFO_START;

            do
            {
                val1 = gr.ReadUInt32();
                if (val1 == 0)
                    val1 = gr.ReadUInt32();
                pos = gr.ReadUInt32();
                total = gr.ReadUInt32();
                type = (UpdateFieldType)gr.ReadUInt32();
                flags = (UpdatafieldFlags)gr.ReadUInt32();

                long oldpos = gr.BaseStream.Position;
                gr.BaseStream.Position = val1 - FIELDS_NAMES_OFFSET;

                try
                {
                    cur = gr.ReadStringNull();
                }
                catch(EndOfStreamException exc)
                {
                    exc.ToString();
                    break;
                }

                gr.BaseStream.Position = oldpos;

                UpdateField uf = new UpdateField(cur, pos, total, type, flags);
                list.Add(uf);

                old = cur;
            } while (true);
        }

        void DumpToFile()
        {
            StreamWriter sw = new StreamWriter("UpdateFields.h");

            sw.WriteLine("// Auto generated for version {0}", WOW_VERSION);
            sw.WriteLine();
            sw.WriteLine("struct sObjectFields");
            sw.WriteLine("{");

            for (int i = 0; i < list.Count; ++i)
            {
                UpdateField uf = list[i];

                if (uf.Pos == 0)
                {
                    if (i > 1)
                    {
                        sw.WriteLine("};");
                        sw.WriteLine();

                        switch (list[i].Name)
                        {
                            case "ITEM_FIELD_OWNER":
                                sw.WriteLine("struct sItemFields");
                                sw.WriteLine("{");
                                break;
                            case "CONTAINER_FIELD_NUM_SLOTS":
                                sw.WriteLine("struct sContainerFields");
                                sw.WriteLine("{");
                                break;
                            case "UNIT_FIELD_CHARM":
                                sw.WriteLine("struct sUnitFields");
                                sw.WriteLine("{");
                                break;
                            case "OBJECT_FIELD_CREATED_BY":
                                sw.WriteLine("struct sGameObjectFields");
                                sw.WriteLine("{");
                                break;
                            case "DYNAMICOBJECT_CASTER":
                                sw.WriteLine("struct sDynamicObjectFields");
                                sw.WriteLine("{");
                                break;
                            case "CORPSE_FIELD_OWNER":
                                sw.WriteLine("struct sCorpseFields");
                                sw.WriteLine("{");
                                break;
                            case "PLAYER_DUEL_ARBITER":
                                sw.WriteLine("struct sPlayerFields");
                                sw.WriteLine("{");
                                break;
                        }
                    }
                }

                WriteFormat(sw, list[i].Type, list[i].Name, list[i].Total);
            }

            sw.WriteLine("};");

            sw.Flush();
            sw.Close();
        }

        void WriteFormat(StreamWriter sw, UpdateFieldType type, string name, long size)
        {
            if (type == UpdateFieldType.WGUID)
            {
                if (size == 2)
                    sw.WriteLine("    {0} {1};", type, name);
                else
                    sw.WriteLine("    {0} {1}[{2}];", type, name, size / 2);
            }
            else if (type == UpdateFieldType.WORD_char_2)
            {
                sw.WriteLine("    {0} {1}_LOW;", UpdateFieldType.WORD, name);
                sw.WriteLine("    {0} {1}_HIGH[{2}];", UpdateFieldType.@char, name, 2);
            }
            else
            {
                if (type == UpdateFieldType.WORD)
                    size *= 2;

                if (type == UpdateFieldType.@char)
                    size *= 4;

                if (size == 1)
                {
                    sw.WriteLine("    {0} {1};", type, name);
                }
                else
                {
                    sw.WriteLine("    {0} {1}[{2}];", type, name, size);
                }
            }
        }
    }
}
