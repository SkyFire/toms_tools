using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Blizzard
{
    public static class Extensions
    {
        public static string FourCC(this byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static bool Compare(this byte[] data, byte[] otherData)
        {
            if (data.Length != otherData.Length)
                return false;

            for (int i = 0; i < data.Length; ++i)
                if (data[i] != otherData[i])
                    return false;

            return true;
        }

        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            byte[] rawData = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            var returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return returnObject;
        }

        public static long Remaining(this BinaryReader reader)
        {
            return reader.BaseStream.Length - reader.BaseStream.Position;
        }

        public static byte[] ReadRemaining(this BinaryReader reader)
        {
            return reader.ReadBytes((int)reader.Remaining());
        }

        public static BinaryReader ToBinaryReader(this byte[] data)
        {
            return new BinaryReader(new MemoryStream(data));
        }

        public static string ToHexString(this byte[] byteArray)
        {
            string retStr = "";
            foreach (byte b in byteArray)
                retStr += b.ToString("X2");
            return retStr;
        }
    }
}
