using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace uf_extractor2
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
            uint val1, val2, val3, val4, val5;
            string cur = String.Empty, old = String.Empty;

            gr.BaseStream.Position = FIELDS_INFO_START;

            do
            {
                val1 = gr.ReadUInt32();
                if (val1 == 0)
                    val1 = gr.ReadUInt32();
                val2 = gr.ReadUInt32();
                val3 = gr.ReadUInt32();
                val4 = gr.ReadUInt32();
                val5 = gr.ReadUInt32();

                long oldpos = gr.BaseStream.Position;
                gr.BaseStream.Position = val1 - FIELDS_NAMES_OFFSET;

                try
                {
                    cur = gr.ReadStringNull();
                }
                catch(EndOfStreamException exc)
                {
                    break;
                }

                gr.BaseStream.Position = oldpos;

                string info = String.Format("Size: {0}, Type: {1}, Flags: {2}", val3, (UpdateFieldType)val4, (UpdatafieldFlags)val5);

                UpdateField uf = new UpdateField(cur, val2, val3, info);
                list.Add(uf);

                //if (!old.Equals("CORPSE_FIELD_PAD") && cur.Equals("CORPSE_FIELD_PAD"))
                //    break;

                old = cur;
            } while (true);
        }

        void DumpToFile()
        {
            StreamWriter sw = new StreamWriter("UpdateFields.h");
            sw.WriteLine("/*");
            sw.WriteLine(" * Copyright (C) 2005-2010 MaNGOS <http://getmangos.com/>");
            sw.WriteLine(" *");
            sw.WriteLine(" * This program is free software; you can redistribute it and/or modify");
            sw.WriteLine(" * it under the terms of the GNU General Public License as published by");
            sw.WriteLine(" * the Free Software Foundation; either version 2 of the License, or");
            sw.WriteLine(" * (at your option) any later version.");
            sw.WriteLine(" *");
            sw.WriteLine(" * This program is distributed in the hope that it will be useful,");
            sw.WriteLine(" * but WITHOUT ANY WARRANTY; without even the implied warranty of");
            sw.WriteLine(" * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the");
            sw.WriteLine(" * GNU General Public License for more details.");
            sw.WriteLine(" *");
            sw.WriteLine(" * You should have received a copy of the GNU General Public License");
            sw.WriteLine(" * along with this program; if not, write to the Free Software");
            sw.WriteLine(" * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA");
            sw.WriteLine(" */");
            sw.WriteLine();
            sw.WriteLine("#ifndef _UPDATEFIELDS_AUTO_H");
            sw.WriteLine("#define _UPDATEFIELDS_AUTO_H");
            sw.WriteLine();
            sw.WriteLine("// Auto generated for version {0}", WOW_VERSION);
            sw.WriteLine();
            sw.WriteLine("enum EObjectFields");
            sw.WriteLine("{");

            string delta = String.Empty;

            for (int i = 0; i < list.Count; ++i)
            {
                UpdateField uf = list[i];

                if (uf.Pos == 0)
                {
                    if (i > 1)
                    {
                        long last_end = list[i - 1].Pos + list[i - 1].Total;

                        WriteFormat(sw, list[i - 1].Name, delta, last_end.ToString("X4"), "", true);

                        if (!list[i - 1].Name.Contains("UNIT"))
                        {
                            sw.WriteLine("};");
                            sw.WriteLine();
                        }
                        else
                        {
                            sw.WriteLine();
                        }

                        switch (list[i].Name)
                        {
                            case "ITEM_FIELD_OWNER":
                                sw.WriteLine("enum EItemFields");
                                sw.WriteLine("{");
                                break;
                            case "CONTAINER_FIELD_NUM_SLOTS":
                                sw.WriteLine("enum EContainerFields");
                                sw.WriteLine("{");
                                break;
                            case "UNIT_FIELD_CHARM":
                                sw.WriteLine("enum EUnitFields");
                                sw.WriteLine("{");
                                break;
                            case "OBJECT_FIELD_CREATED_BY":
                                sw.WriteLine("enum EGameObjectFields");
                                sw.WriteLine("{");
                                break;
                            case "DYNAMICOBJECT_CASTER":
                                sw.WriteLine("enum EDynamicObjectFields");
                                sw.WriteLine("{");
                                break;
                            case "CORPSE_FIELD_OWNER":
                                sw.WriteLine("enum ECorpseFields");
                                sw.WriteLine("{");
                                break;
                        }
                    }
                }

                delta = String.Empty;
                if (i > 5)
                    delta = "OBJECT_END";
                if (list[i].Name.StartsWith("CONTAINER"))
                    delta = "ITEM_END";
                if (list[i].Name.StartsWith("PLAYER"))
                    delta = "UNIT_END";

                WriteFormat(sw, list[i].Name, delta, list[i].Pos.ToString("X4"), list[i].Descr, false);
            }
            WriteFormat(sw, list[list.Count - 1].Name, delta, (list[list.Count - 1].Pos + 1).ToString("X4"), "", true);
            sw.WriteLine("};");
            sw.WriteLine("#endif");

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
