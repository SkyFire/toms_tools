using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using A9parser;
using OpcodeParsers;
using OpcodesEnum;
using UpdateFields;
using WoWReader;

namespace bin_parser
{
    internal class Binparser
    {
        private static NumberFormatInfo nfi;
        static uint packet = 1;
        // we must use clear dictionary for each new file...
        public static Dictionary<ulong, ObjectTypes> m_objects = new Dictionary<ulong, ObjectTypes>();
        //public static Dictionary<ulong, WoWObject> m_objects2 = new Dictionary<ulong, WoWObject>();

        Binparser()
        {
            nfi = new CultureInfo("en-us", false).NumberFormat;
            nfi.NumberDecimalSeparator = ".";
        }

        ~Binparser()
        {
            m_objects.Clear();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Binparser bp = new Binparser();

            UpdateFieldsLoader.LoadUpdateFields();

            DateTime starttime = DateTime.Now;

            Console.WriteLine("Starting at {0}", starttime);

            DirectoryInfo di = new DirectoryInfo("."); // DirectoryInfo
            FileInfo[] fi = di.GetFiles("*.sqlite", SearchOption.AllDirectories); // Get file list

            Console.WriteLine("Found {0} files to parse", fi.Length);

            foreach (FileInfo f in fi)
            {
                ParseFile(f);
            }

            TimeSpan worktime = DateTime.Now - starttime;
            Console.WriteLine("Done in " + worktime.ToString() + "!");
            Console.ReadKey();
        }

        private static bool ParseFile(FileInfo f)
        {
            DateTime filestarttime = DateTime.Now;

            Console.Write("Parsing {0}...", f.Name);

            SQLiteConnection connection = new SQLiteConnection("Data Source=" + f.FullName);
            SQLiteCommand command = new SQLiteCommand(connection);
            connection.Open();
            command.CommandText = "SELECT id, sess_id, timestamp, direction, opcode, data FROM packets ORDER BY id;";
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();

            MemoryStream ms = new MemoryStream();

            while (reader.Read())
            {
                uint id = (uint)reader.GetInt32(0);
                uint sess_id = (uint)reader.GetInt32(1);
                string timestamp = reader.GetString(2);
                byte direction = reader.GetByte(3);
                ushort opcode = (ushort)reader.GetInt32(4);
                if (opcode > 1054)
                {
                    Console.WriteLine(opcode);
                    throw new Exception("Test");
                }
                byte[] data_ = (byte[])reader.GetValue(5);

                uint size = sizeof(uint) + sizeof(uint) + (uint)timestamp.Length + 1 + sizeof(byte) + sizeof(ushort) + (uint)data_.Length;

                byte[] id_arr = BitConverter.GetBytes(id);
                byte[] sessid_arr = BitConverter.GetBytes(sess_id);
                byte[] time_arr = Encoding.ASCII.GetBytes(timestamp);
                byte[] op = BitConverter.GetBytes(opcode);
                byte[] sz = BitConverter.GetBytes(size);

                ms.Write(sz, 0, sz.Length);
                ms.Write(id_arr, 0, id_arr.Length);
                ms.Write(sessid_arr, 0, sessid_arr.Length);
                ms.Write(time_arr, 0, time_arr.Length);
                ms.WriteByte(0);
                ms.WriteByte(direction);
                ms.Write(op, 0, op.Length);
                ms.Write(data_, 0, data_.Length);
            }

            reader.Close();
            connection.Close();

            GenericReader gr = new GenericReader(ms, Encoding.ASCII);
            gr.BaseStream.Position = 0;

            string error_log = f.FullName + ".errors.txt";
            StreamWriter swe = new StreamWriter(error_log);

            string database_log = f.FullName + ".data.txt";
            StreamWriter data = new StreamWriter(database_log);

            string hex_log = f.FullName + ".hex.log";
            StreamWriter hex = new StreamWriter(hex_log);

            string ofn = f.FullName + ".data_out.txt";
            StreamWriter sw = new StreamWriter(ofn);

            sw.AutoFlush = true;
            swe.AutoFlush = true;
            data.AutoFlush = true;
            hex.AutoFlush = true;

            while (gr.PeekChar() >= 0)
            {
                //try
                //{
                if (ParseHeader(gr, sw, swe, data, hex))
                    packet++;
                //}
                //catch (Exception exc)
                //{
                //    MessageBox.Show(exc.ToString());
                //    swe.WriteLine("error in pos " + gr.BaseStream.Position.ToString("X16"));
                //}
            }

            // clear objects list...
            m_objects.Clear();

            sw.Close();
            swe.Close();
            data.Close();
            hex.Close();
            gr.Close();

            TimeSpan fileworktime = DateTime.Now - filestarttime;

            Console.WriteLine(" Parsed in {0}", fileworktime);

            return true;
        }

        /// <summary>
        /// Packet header parser.
        /// </summary>
        /// <param name="gr">Main stream reader.</param>
        /// <param name="sw">Data stream writer.</param>
        /// <param name="swe">Error logger writer.</param>
        /// <param name="data">Data logger writer.</param>
        /// <param name="hex">HEX logger writer.</param>
        /// <returns>Successful</returns>
        private static bool ParseHeader(GenericReader gr, StreamWriter sw, StreamWriter swe, StreamWriter data, StreamWriter hex)
        {
            StringBuilder sb = new StringBuilder();

            int datasize = gr.ReadInt32();

            //sb.AppendLine("Packet offset " + (gr.BaseStream.Position - 4).ToString("X2"));

            //sb.AppendLine("Packet number: " + packet);

            //sb.AppendLine("Data size " + datasize);

            byte[] temp = gr.ReadBytes(datasize);
            MemoryStream ms = new MemoryStream(temp);
            GenericReader gr2 = new GenericReader(ms);

            uint id = 0;
            uint sess_id = 0;
            string time = "";
            byte direction = 0; // 0-CMSG, 1-SMSG
            OpCodes opcode = OpCodes.MSG_NULL_ACTION;

            id = gr2.ReadUInt32();
            sess_id = gr2.ReadUInt32();
            time = gr2.ReadStringNull();
            direction = gr2.ReadByte();
            opcode = (OpCodes)gr2.ReadUInt16();

            long cur_pos = gr2.BaseStream.Position;

            HexLike(gr2, hex, id, sess_id, time, direction, opcode);

            gr2.BaseStream.Position = cur_pos;

            switch (opcode)
            {
                /*case OpCodes.SMSG_MONSTER_MOVE:
                    OpcodeParser.ParseMonsterMoveOpcode(gr, gr2, sb, swe, direction);
                    break;*/
                /*case OpCodes.SMSG_INITIAL_SPELLS:
                    OpcodeParser.ParseInitialSpellsOpcode(gr, gr2, sb, swe, direction);
                    break;
                case OpCodes.SMSG_AUCTION_LIST_RESULT:
                    OpcodeParser.ParseAuctionListResultOpcode(gr, gr2, sb, swe, direction);
                    break;*/
                /*case OpCodes.SMSG_PARTY_MEMBER_STATS:
                case OpCodes.SMSG_PARTY_MEMBER_STATS_FULL:
                    OpcodeParser.ParsePartyMemberStatsOpcode(gr, gr2, sb, swe, direction);
                    break;*/
                case OpCodes.SMSG_UPDATE_OBJECT:
                case OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT:
                    if (opcode == OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT)
                    {
                        gr2 = A9.Decompress(gr2);
                        gr2.BaseStream.Position = 0;
                        hex.WriteLine("Decompressed SMSG_COMPRESSED_UPDATE_OBJECT:");
                        HexLike(gr2, hex, id, sess_id, time, direction, OpCodes.SMSG_UPDATE_OBJECT);
                        gr2.BaseStream.Position = 0;
                    }
                    A9.ParseUpdatePacket(gr, gr2, sb, swe);
                    break;
                /*case OpCodes.SMSG_SPELLNONMELEEDAMAGELOG:
                    OpcodeParser.ParseSpellNonMeleeDamageLogOpcode(gr, gr2, sb, swe, direction);
                    break;
                case OpCodes.SMSG_SPELLLOGEXECUTE:
                    OpcodeParser.ParseSpellLogExecuteOpcode(gr, gr2, sb, swe, direction);
                    break;*/
                /*case OpCodes.SMSG_LOGIN_SETTIMESPEED:
                    OpcodeParser.ParseLoginSetTimeSpeedOpcode(gr, gr2, sb, swe, direction);
                    break;
                case OpCodes.SMSG_TRAINER_LIST:
                    OpcodeParser.ParseTrainerListOpcode(gr, gr2, sb, swe, direction);
                    break;
                case OpCodes.SMSG_ATTACKERSTATEUPDATE:
                    OpcodeParser.ParseAttackerStateUpdateOpcode(gr, gr2, sb, swe, direction);
                    break;
                case OpCodes.MSG_CORPSE_QUERY:
                    OpcodeParser.ParseCorpseQueryOpcode(gr, gr2, sb, swe, direction);
                    break;
                case OpCodes.SMSG_LOGIN_VERIFY_WORLD:
                    OpcodeParser.ParseLoginVerifyWorldOpcode(gr, gr2, sb, swe, direction);
                    break;
                default:    // unhandled opcode
                    return false;*/
            }

            if (sb.ToString().Length != 0)
                sw.WriteLine(sb.ToString());

            ms.Close();
            gr2.Close();

            return true;
        }

        private static void HexLike(GenericReader gr, StreamWriter hex, uint id, uint sess_id, string time, byte direction, OpCodes opcode)
        {
            int length = (int)(gr.Remaining);

            string dir = "";

            if (direction == 0)
                dir = "C->S";
            else
                dir = "S->C";

            hex.WriteLine("Packet {0}, id {1}, {2} ({3}), len {4}", dir, id, opcode, (ushort)opcode, length);
            //hex.WriteLine("Session {0}, time {1}", sess_id, time);

            int byteIndex = 0;

            int full_lines = length >> 4;
            int rest = length & 0x0F;

            if ((full_lines == 0) && (rest == 0)) // empty packet (no data provided)
            {
                hex.WriteLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ................");
            }
            else
            {
                for (int i = 0; i < full_lines; ++i, byteIndex += 0x10)
                {
                    StringBuilder bytes = new StringBuilder();
                    StringBuilder chars = new StringBuilder();

                    for (int j = 0; j < 0x10; ++j)
                    {
                        int c = gr.ReadByte();

                        bytes.Append(c.ToString("X2"));

                        bytes.Append(' ');

                        if (c >= 0x20 && c < 0x80)
                            chars.Append((char)c);
                        else
                            chars.Append('.');
                    }

                    hex.WriteLine(byteIndex.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
                }

                if (rest != 0)
                {
                    StringBuilder bytes = new StringBuilder();
                    StringBuilder chars = new StringBuilder(rest);

                    for (int j = 0; j < 0x10; ++j)
                    {
                        if (j < rest)
                        {
                            int c = gr.ReadByte();

                            bytes.Append(c.ToString("X2"));

                            bytes.Append(' ');

                            if (c >= 0x20 && c < 0x80)
                                chars.Append((char)c);
                            else
                                chars.Append('.');
                        }
                        else
                            bytes.Append("-- ");
                    }
                    hex.WriteLine(byteIndex.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
                }
            }

            hex.WriteLine();
        }
    }
}
