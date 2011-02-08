using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace WoWPacketViewer
{
    public class Utility
    {
        public static byte[] HexStringToBinary(string data)
        {
            var bytes = new List<byte>();
            for (var i = 0; i < data.Length; i += 2)
            {
                bytes.Add(Byte.Parse(data.Substring(i, 2), NumberStyles.HexNumber));
            }
            return bytes.ToArray();
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            var str = String.Empty;
            for (var i = 0; i < data.Length; ++i)
                str += data[i].ToString("X2", CultureInfo.InvariantCulture);
            return str;
        }

        public static string PrintHex(byte[] data, int start, int size)
        {
            var result = String.Empty;
            var counter = start;

            while (counter != size)
            {
                for (var i = 0; i < 0x10; ++i)
                {
                    result += String.Format("{0:X2} ", data[counter]);
                    counter++;

                    if (counter == size)
                    {
                        result += "\r\n";
                        break;
                    }
                }
                result += "\r\n";
            }
            return result;
        }

        public static byte[] Decompress(byte[] data)
        {
            var uncompressedLength = BitConverter.ToUInt32(data, 0);
            var output = new byte[uncompressedLength];

            var dStream = new DeflateStream(new MemoryStream(data, 6, data.Length - 6), CompressionMode.Decompress);
            dStream.Read(output, 0, output.Length);
            dStream.Close();

            return output;
        }

        public static string HexLike(Packet packet)
        {
            var length = packet.Data.Length;
            var dir = (packet.Direction == Direction.Client) ? "C->S" : "S->C";

            var result = new StringBuilder();
            result.AppendFormat("Packet {0}, {1} ({2}), len {3}", dir, packet.Code, (ushort)packet.Code, length);
            result.AppendLine();

            if (length == 0)
            {
                result.AppendLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- : ................");
            }
            else
            {
                var offset = 0;
                for (var i = 0; i < length; i += 0x10)
                {
                    var bytes = new StringBuilder();
                    var chars = new StringBuilder();

                    for (var j = 0; j < 0x10; ++j)
                    {
                        if (offset < length)
                        {
                            int c = packet.Data[offset];
                            offset++;

                            bytes.AppendFormat("{0,-3:X2}", c);
                            chars.Append((c >= 0x20 && c < 0x80) ? (char)c : '.');
                        }
                        else
                        {
                            bytes.Append("-- ");
                            chars.Append('.');
                        }
                    }

                    result.AppendLine(i.ToString("X4") + ": " + bytes + ": " + chars);
                }
            }

            result.AppendLine();

            return result.ToString();
        }
    }
}
