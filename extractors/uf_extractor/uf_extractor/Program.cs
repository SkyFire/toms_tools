using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace uf_extractor
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        struct UpdateField
        {
            public string Name;
            public long Pos;
            public long Total;
            public string Descr;

            public UpdateField(string name, long pos, long total, string descr)
            {
                Name = name;
                Pos = pos;
                Total = total;
                Descr = descr;
            }
        };

        [Flags]
        enum UpdatafieldFlags
        {
            NONE = 0,
            PUBLIC = 1,
            PRIVATE = 2,
            OWNER_ONLY = 4,
            UNK1 = 8,
            UNK2 = 16,
            UNK3 = 32,
            GROUP_ONLY = 64,
            UNK4 = 128,
            DYNAMIC = 256
        };

        enum UpdateFieldType
        {
            NONE = 0,
            INT = 1,
            TWO_SHORT = 2,
            FLOAT = 3,
            GUID = 4,
            BYTES = 5
        };

        private static string wow_exe_name = "WoW.exe";
        private static FileStream fs;
        private static GenericReader gr;
        private static TextReader tr;
        private static long start_str_ofs, start_ofs, str_ofs, str_ofs_delta;
        private static long last_end, delta;
        private static string version;
        private static string stream_string;
        private static long object_end, container_end, item_end, unit_end, player_end, gameobject_end, dynobject_end;
        private static List<UpdateField> list = new List<UpdateField>();

        private static string zsp(string s, int sp_max, bool size_left)
        {
            string t = String.Empty, result;

            result = s;
            if (s.Length > sp_max)
            {
                return result; // not change
            }

            int length = s.Length;
            for (int i = 1; i < sp_max - length; i++) s += " ";

            if (size_left)
                result = t + s;
            else
                result = s + t;

            return result;
        }

        private static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(TopLevelErrorHandler);

            if (args.Length == 1) wow_exe_name = args[0];

            if (!File.Exists(wow_exe_name))
            {
                Console.WriteLine("{0} not found!", wow_exe_name);
                Console.ReadKey();
                return;
            }

            fs = new FileStream(wow_exe_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            gr = new GenericReader(fs);

            Console.WriteLine("{0} kb readed to filestream", fs.Length / 1024);

            tr = new StreamReader(fs, Encoding.ASCII);
            stream_string = tr.ReadToEnd();

            if (!FindVersionInfo())
                return;

            Console.WriteLine("Client version {0}", version);

            if (!FindFieldsNamesStart())
                return;

            if (!FindFieldsInfoStart())
                return;

            GetStringsOffsetDelta();

            int count = FillList();

            if (count == 0)
                return;

            tr.Close();
            gr.Close();
            fs.Close();

            GetEnds(count);

            DumpToFile(count);

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        private static void TopLevelErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Error Occured : " + e.Message);
        }

        private static bool FindVersionInfo()
        {
            string dir = Directory.GetCurrentDirectory();

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(dir + "\\" + wow_exe_name);

            version = info.FileVersion;

            return true;
        }

        private static bool FindFieldsNamesStart()
        {
            start_str_ofs = 0;

            int i = stream_string.IndexOf("OBJECT_FIELD_GUID" + (char)0);

            if (i > 0)
            {
                start_str_ofs = i;

                Console.WriteLine("Fields names start position found at 0x{0}", i.ToString("X8"));

                return true;
            }
            Console.WriteLine("Fields names start position not found");
            return false;
        }

        private static bool FindFieldsInfoStart()
        {
            string sstr = String.Empty;
            byte[] temp = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };

            for (int n = 0; n < temp.Length; n++)
                sstr += (char)temp[n];

            int i = stream_string.IndexOf(sstr);
            if (i > 0)
            {
                start_ofs = i - 4;

                Console.WriteLine("Found fields info start position at 0x{0}", start_ofs.ToString("X8"));

                return true;
            }
            Console.WriteLine("Fields info start position not found");
            return false;
        }

        private static void GetStringsOffsetDelta()
        {
            long oldpos = gr.BaseStream.Position;
            gr.BaseStream.Position = start_ofs;
            long temp = gr.ReadInt64();
            gr.BaseStream.Position = oldpos;
            str_ofs_delta = temp - start_str_ofs;
            Console.WriteLine("str_ofs_delta = temp - start_str_ofs = 0x{0} - 0x{1} = 0x{2}", temp.ToString("X8"), start_str_ofs.ToString("X8"), str_ofs_delta.ToString("X8"));
        }

        private static int FillList()
        {
            int i = 0;

            string old_s = String.Empty;
            string s = String.Empty;

        	gr.BaseStream.Position = start_ofs;

            do
            {
                uint p1 = gr.ReadUInt32();

                if (p1 < 0x9999)
                {
                    p1 = gr.ReadUInt32();
                }
                uint p2 = gr.ReadUInt32();
                uint p3 = gr.ReadUInt32();
                uint p4 = gr.ReadUInt32();
                uint p5 = gr.ReadUInt32();

                str_ofs = p1 - str_ofs_delta;

                long oldpos = gr.BaseStream.Position;
                gr.BaseStream.Position = str_ofs;

                s = gr.ReadStringNull();
                gr.BaseStream.Position = oldpos;

                StringBuilder sb = new StringBuilder();
                sb.Append("Size: ");
                sb.Append(p3);
                sb.Append(", Type: ");
                UpdateFieldType type = (UpdateFieldType)p4;
                sb.Append(type);
                sb.Append(", Flags: ");
                UpdatafieldFlags flags = (UpdatafieldFlags)p5;
                sb.Append(flags);

                UpdateField uf = new UpdateField(s, p2, p3, sb.ToString());
                list.Add(uf);

                if (!old_s.Equals("CORPSE_FIELD_PAD") && s.Equals("CORPSE_FIELD_PAD"))
                    break;

                old_s = s;

                i++;
            } while (true);

            return i;
        }

        private static void GetEnds(int count)
        {
            object_end = 0;
            container_end = 0;
            item_end = 0;
            unit_end = 0;
            player_end = 0;
            gameobject_end = 0;
            dynobject_end = 0;
            delta = 0;
            last_end = 0;

            for (int k = 0; k <= count; k++)
            {
                if (list[k].Pos == 0)
                {
                    if (k > 1)
                    {
                        last_end = list[k - 1].Pos + list[k - 1].Total + delta;

                        delta = 0;

                        if (k > 5)
                            delta = 6;

                        if (list[k].Name.StartsWith("CONTAINER"))
                            object_end = last_end;
                        if (list[k].Name.StartsWith("ITEM"))
                            container_end = last_end;
                        if (list[k].Name.StartsWith("UNIT"))
                            item_end = last_end;
                        if (list[k].Name.StartsWith("PLAYER"))
                            unit_end = last_end;
                        if (list[k].Name.StartsWith("OBJECT_FIELD_CREATED_BY"))
                            player_end = last_end;
                        if (list[k].Name.StartsWith("DYNAMICOBJECT"))
                            gameobject_end = last_end;
                        if (list[k].Name.StartsWith("CORPSE"))
                            dynobject_end = last_end;
                    }
                }
            }
        }

        private static void DumpToFile(int count)
        {
            // пишем в файл
            StreamWriter sw = new StreamWriter("UpdateFields.h");
            sw.WriteLine("/*");
            sw.WriteLine(" * Copyright (C) 2005-2010 MaNGOS <http://www.mangosproject.org/>");
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
            sw.WriteLine("// Auto generated for version {0}", version);
            sw.WriteLine();
            sw.WriteLine("enum EObjectFields");
            sw.WriteLine("{");

            delta = 0;
            last_end = 0;
            for (int k = 0; k <= count; k++)
            {
                if (list[k].Pos == 0)
                {
                    if (k > 1)
                    {
                        last_end = list[k - 1].Pos + list[k - 1].Total + delta;

                        sw.WriteLine("    " + zsp(list[k - 1].Name.Substring(0, list[k - 1].Name.IndexOf("_")) + "_END", 42, true) + " = 0x{0}", last_end.ToString("X4"));

                        if (!list[k - 1].Name.Contains("UNIT"))
                        {
                            sw.WriteLine("};");
                            sw.WriteLine();
                        }
                        else
                        {
                            sw.WriteLine();
                        }

                        switch (list[k].Name)
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

                delta = 0;
                if (k > 5)
                    delta = 6;

                if (list[k].Name.StartsWith("CONTAINER"))
                    delta = item_end;
                if (list[k].Name.StartsWith("PLAYER"))
                    delta = unit_end;

                sw.WriteLine("    " + zsp(list[k].Name, 42, true) + " = 0x{0}, // {1}", (list[k].Pos + delta).ToString("X4"), list[k].Descr);
            }
            sw.WriteLine("    " + zsp(list[count].Name.Substring(0, list[count].Name.IndexOf("_")) + "_END", 42, true) + " = 0x{0}", (list[count].Pos + 1 + delta).ToString("X4"));
            sw.WriteLine("};");
            sw.WriteLine("#endif");

            sw.Flush();
            sw.Close();
        }
    }
}
