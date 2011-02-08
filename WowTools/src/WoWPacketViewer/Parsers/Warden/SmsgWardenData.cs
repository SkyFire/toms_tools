using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers.Warden
{
    [Parser(OpCodes.SMSG_WARDEN_DATA)]
    internal class SmsgWardenData : Parser
    {
        public SmsgWardenData(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            BinaryReader gr = Packet.CreateReader();

            WardenData.CheckInfos.Clear();

            var wardenOpcode = gr.ReadByte();
            gr.BaseStream.Position = 0;
            //AppendFormatLine("S->C Warden Opcode: {0:X2}", wardenOpcode);

            switch (wardenOpcode)
            {
                case 0x00:
                    {
                        var opcode = gr.ReadByte();
                        var md5 = gr.ReadBytes(16); // md5
                        var rc4 = gr.ReadBytes(16); // rc4 key
                        var len = gr.ReadInt32();   // len
                        AppendFormatLine("MD5: 0x{0}", Utility.ByteArrayToHexString(md5));
                        AppendFormatLine("RC4: 0x{0}", Utility.ByteArrayToHexString(rc4));
                        AppendFormatLine("Len: {0}", len);
                        AppendLine();
                    }
                    break;
                case 0x01:
                    {
                        var opcode = gr.ReadByte();
                        var len = gr.ReadInt16();
                        var chunk = gr.ReadBytes(len);
                        AppendFormatLine("Received warden module chunk, len {0}", len);
                        AppendLine();
                    }
                    break;
                case 0x02:
                    Parse_CHEAT_CHECKS(gr);
                    break;
                case 0x03:
                    {
                        while (gr.BaseStream.Position != gr.BaseStream.Length)
                        {
                            var opcode = gr.ReadByte();
                            var len = gr.ReadInt16();
                            var checkSum = gr.ReadUInt32();
                            var data = gr.ReadBytes(len);

                            AppendFormatLine("Len: {0}", len);
                            AppendFormatLine("Checksum: 0x{0:X8} {1}", checkSum, WardenData.ValidateCheckSum(checkSum, data));
                            AppendFormatLine("Data: 0x{0}", Utility.ByteArrayToHexString(data));
                            AppendLine();
                        }
                    }
                    break;
                case 0x05:
                    {
                        var opcode = gr.ReadByte();
                        var seed = gr.ReadBytes(16);
                        AppendFormatLine("Seed: 0x{0}", Utility.ByteArrayToHexString(seed));
                        AppendLine();
                    }
                    break;
                default:
                    AppendFormatLine("Unknown warden opcode {0}", wardenOpcode);
                    break;
            }

            CheckPacket(gr);

            return GetParsedString();
        }

        private void Parse_CHEAT_CHECKS(BinaryReader gr)
        {
            //AppendFormatLine("====== CHEAT CHECKS START ======");
            //AppendLine();

            List<string> strings = new List<string>();
            strings.Add("");

            var opcode = gr.ReadByte();

            byte len;
            while ((len = gr.ReadByte()) != 0)
            {
                var strBytes = gr.ReadBytes(len);
                var str = Encoding.ASCII.GetString(strBytes);
                strings.Add(str);
            }
            var rest = gr.BaseStream.Length - gr.BaseStream.Position;
            var checks = gr.ReadBytes((int)rest);
            var reader = new BinaryReader(new MemoryStream(checks), Encoding.ASCII);

            while (reader.BaseStream.Position + 1 != reader.BaseStream.Length)
            {
                var xor = checks[checks.Length - 1];

                var check = reader.ReadByte();
                var checkType = (byte)(check ^ xor);

                CheckType checkType2;
                if (!WardenData.CheckTypes.TryGetValue(checkType, out checkType2))
                {
                    WardenData.ShowForm(strings, checks, check);
                    break;
                }

                switch (checkType2)
                {
                    case CheckType.TIMING_CHECK:
                        Parse_TIMING_CHECK(check, checkType);
                        break;
                    case CheckType.MEM_CHECK:
                        Parse_MEM_CHECK(strings, reader, check, checkType);
                        break;
                    case CheckType.PAGE_CHECK_A:
                    case CheckType.PAGE_CHECK_B:
                        Parse_PAGE_CHECK(reader, check, checkType);
                        break;
                    case CheckType.PROC_CHECK:
                        Parse_PROC_CHECK(strings, reader, check, checkType);
                        break;
                    case CheckType.MPQ_CHECK:
                        Parse_MPQ_CHECK(strings, reader, check, checkType);
                        break;
                    case CheckType.LUA_STR_CHECK:
                        Parse_LUA_STR_CHECK(strings, reader, check, checkType);
                        break;
                    case CheckType.DRIVER_CHECK:
                        Parse_DRIVER_CHECK(strings, reader, check, checkType);
                        break;
                    default:
                        AppendFormatLine("Unknown CheckType {0}", checkType2);
                        break;
                }
            }
            //AppendFormatLine("====== CHEAT CHECKS END ======");
            //AppendLine();
        }

        private void Parse_DRIVER_CHECK(IList<string> strings, BinaryReader reader, byte check, byte checkType)
        {
            var seed = reader.ReadUInt32();
            var sha1 = reader.ReadBytes(20);
            var stringIndex = reader.ReadByte();
            //AppendFormatLine("====== DRIVER_CHECK START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("Seed: 0x{0:X8}", seed);
            //AppendFormatLine("SHA1: 0x{0}", Utility.ByteArrayToHexString(sha1));
            //AppendFormatLine("Driver: {0}", strings[stringIndex]);
            //AppendFormatLine("====== DRIVER_CHECK END ======");
            //AppendLine();

            AppendFormatLine("INSERT INTO warden_data_result (`check`,`data`,`address`,`length`,`str`,`result`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');", checkType, Utility.ByteArrayToHexString(BitConverter.GetBytes(seed)) + Utility.ByteArrayToHexString(sha1), "", "", strings[stringIndex], "");

            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], 0));
        }

        private void Parse_LUA_STR_CHECK(IList<string> strings, BinaryReader reader, byte check, byte checkType)
        {
            var stringIndex = reader.ReadByte();
            //AppendFormatLine("====== LUA_STR_CHECK START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("String: {0}", strings[stringIndex]);
            //AppendFormatLine("====== LUA_STR_CHECK END ======");
            //AppendLine();

            AppendFormatLine("INSERT INTO warden_data_result (`check`,`data`,`address`,`length`,`str`,`result`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');", checkType, "", "", "", strings[stringIndex], "");

            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], 0));
        }

        private void Parse_MPQ_CHECK(IList<string> strings, BinaryReader reader, byte check, byte checkType)
        {
            var fileNameIndex = reader.ReadByte();
            //AppendFormatLine("====== MPQ_CHECK START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("File: {0}", strings[fileNameIndex]);
            //AppendFormatLine("====== MPQ_CHECK END ======");
            //AppendLine();

            AppendFormatLine("INSERT INTO warden_data_result (`check`,`data`,`address`,`length`,`str`,`result`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');", checkType, "", "", "", strings[fileNameIndex], "");

            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], 0));
        }

        private void Parse_PROC_CHECK(IList<string> strings, BinaryReader reader, byte check, byte checkType)
        {
            var seed = reader.ReadUInt32();
            var sha1 = reader.ReadBytes(20);
            var module = reader.ReadByte();
            var proc = reader.ReadByte();
            var addr = reader.ReadUInt32();
            var bytesToRead = reader.ReadByte();
            //AppendFormatLine("====== PROC_CHECK START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("Seed: 0x{0:X8}", seed);
            //AppendFormatLine("SHA1: 0x{0}", Utility.ByteArrayToHexString(sha1));
            //AppendFormatLine("Module: {0}", strings[module]);
            //AppendFormatLine("Proc: {0}", strings[proc]);
            //AppendFormatLine("Address: 0x{0:X8}", addr);
            //AppendFormatLine("Bytes to read: {0}", bytesToRead);
            //AppendFormatLine("====== PROC_CHECK END ======");
            //AppendLine();
            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], 0));
        }

        private void Parse_PAGE_CHECK(BinaryReader reader, byte check, byte checkType)
        {
            var seed = reader.ReadUInt32();
            var sha1 = reader.ReadBytes(20);
            var addr = reader.ReadUInt32();
            var bytesToRead = reader.ReadByte();
            //AppendFormatLine("====== PAGE_CHECK_A_B START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("Seed: 0x{0:X8}", seed);
            //AppendFormatLine("SHA1: 0x{0}", Utility.ByteArrayToHexString(sha1));
            //AppendFormatLine("Address: 0x{0:X8}", addr);
            //AppendFormatLine("Bytes to read: {0}", bytesToRead);
            //AppendFormatLine("====== PAGE_CHECK_A_B END ======");
            //AppendLine();

            AppendFormatLine("INSERT INTO warden_data_result (`check`,`data`,`address`,`length`,`str`,`result`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');", checkType, Utility.ByteArrayToHexString(BitConverter.GetBytes(seed)) + Utility.ByteArrayToHexString(sha1), addr, bytesToRead, "", "");

            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], 0));
        }

        private void Parse_MEM_CHECK(IList<string> strings, BinaryReader reader, byte check, byte checkType)
        {
            var strIndex = reader.ReadByte(); // string index
            var offset = reader.ReadUInt32(); // offset
            var readLen = reader.ReadByte();  // bytes to read
            //AppendFormatLine("====== MEM_CHECK START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("Module: {0}", (strings[strIndex] == "") ? "base" : strings[strIndex]);
            //AppendFormatLine("Offset: 0x{0:X8}", offset);
            //AppendFormatLine("Bytes to read: {0}", readLen);
            //AppendFormatLine("====== MEM_CHECK END ======");
            //AppendLine();

            AppendFormatLine("INSERT INTO warden_data_result (`check`,`data`,`address`,`length`,`str`,`result`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');", checkType, "", offset, readLen, strings[strIndex], "");

            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], readLen));
        }

        private void Parse_TIMING_CHECK(byte check, byte checkType)
        {
            //AppendFormatLine("====== TIMING_CHECK START ======");
            //AppendFormatLine("CheckType {0:X2} ({1:X2})", checkType, check);
            //AppendFormatLine("====== TIMING_CHECK END ======");
            //AppendLine();
            WardenData.CheckInfos.Add(new CheckInfo(WardenData.CheckTypes[checkType], 0));
        }
    }
}
