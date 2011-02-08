using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace WoWReader
{
    #region Coords3
    /// <summary>
    ///  Represents a coordinates of WoW object without orientation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct Coords3
    {
        public float X, Y, Z;

        /// <summary>
        ///  Converts the numeric values of this instance to its equivalent string representations, separator is space.
        /// </summary>
        public string GetCoords()
        {
            string coords = String.Empty;

            coords += X.ToString().Replace(",", ".");
            coords += " ";
            coords += Y.ToString().Replace(",", ".");
            coords += " ";
            coords += Z.ToString().Replace(",", ".");

            return coords;
        }
    }
    #endregion

    #region Coords4
    /// <summary>
    ///  Represents a coordinates of WoW object with specified orientation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct Coords4
    {
        public float X, Y, Z, O;

        /// <summary>
        ///  Converts the numeric values of this instance to its equivalent string representations, separator is space.
        /// </summary>
        public string GetCoordsAsString()
        {
            string coords = String.Empty;

            coords += X.ToString().Replace(",", ".");
            coords += " ";
            coords += Y.ToString().Replace(",", ".");
            coords += " ";
            coords += Z.ToString().Replace(",", ".");
            coords += " ";
            coords += O.ToString().Replace(",", ".");

            return coords;
        }
    }
    #endregion

    #region GenericReader
    /// <summary>
    ///  Reads WoW specific data types as binary values in a specific encoding.
    /// </summary>
    class GenericReader : BinaryReader
    {
        #region GenericReader_stream
        /// <summary>
        ///  Not yet.
        /// </summary>
        /// <param name="input">Input stream.</param>
        public GenericReader(Stream input)
            : base(input)
        {
        }
        #endregion

        #region GenericReader_stream_encoding
        /// <summary>
        ///  Not yet.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="encoding">Input encoding.</param>
        public GenericReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }
        #endregion

        #region GenericReader_filestream
        /// <summary>
        ///  Not yet.
        /// </summary>
        /// <param name="fname">Input file name.</param>
        public GenericReader(string fname)
            : base(new FileStream(fname, FileMode.Open, FileAccess.Read))
        {
        }
        #endregion

        #region GenericReader_filestream_encoding
        /// <summary>
        ///  Not yet.
        /// </summary>
        /// <param name="fname">Input file name.</param>
        /// <param name="encoding">Input encoding.</param>
        public GenericReader(string fname, Encoding encoding)
            : base(new FileStream(fname, FileMode.Open, FileAccess.Read), encoding)
        {
        }
        #endregion

        #region ReadPackedGuid
        /// <summary>
        ///  Reads the packed guid from the current stream and advances the current position of the stream by packed guid size.
        /// </summary>
        public ulong ReadPackedGuid()
        {
            ulong res = 0;
            byte mask = ReadByte();

            if (mask == 0)
                return res;

            int i = 0;

            while (i < 9)
            {
                if ((mask & 1 << i) != 0)
                    res += (ulong)ReadByte() << (i * 8);
                i++;
            }
            return res;
        }
        #endregion

        #region ReadStringNumber
        /// <summary>
        ///  Reads the string with known length from the current stream and advances the current position of the stream by string length.
        /// <seealso cref="GenericReader.ReadStringNull"/>
        /// </summary>
        public string ReadStringNumber()
        {
            string text = String.Empty;
            uint num = ReadUInt32(); // string length

            for (uint i = 0; i < num; i++)
            {
                text += (char)ReadByte();
            }
            return text;
        }
        #endregion

        #region ReadStringNull
        /// <summary>
        ///  Reads the NULL terminated string from the current stream and advances the current position of the stream by string length + 1.
        /// <seealso cref="GenericReader.ReadStringNumber"/>
        /// </summary>
        public string ReadStringNull()
        {
            byte num;
            string text = String.Empty;
            //ReadChar();
            while ((num = ReadByte()) != 0)
            {
                text += (char)num;
            }

            if (text.Length == 0)
                text = "empty";

            return text;
        }
        #endregion

        #region ReadCoords3
        /// <summary>
        ///  Reads the object coordinates from the current stream and advances the current position of the stream by 12 bytes.
        /// </summary>
        public Coords3 ReadCoords3()
        {
            Coords3 v;

            v.X = ReadSingle();
            v.Y = ReadSingle();
            v.Z = ReadSingle();

            return v;
        }
        #endregion

        #region ReadCoords4
        /// <summary>
        ///  Reads the object coordinates and orientation from the current stream and advances the current position of the stream by 16 bytes.
        /// </summary>
        public Coords4 ReadCoords4()
        {
            Coords4 v;

            v.X = ReadSingle();
            v.Y = ReadSingle();
            v.Z = ReadSingle();
            v.O = ReadSingle();

            return v;
        }
        #endregion
    }
    #endregion
}
