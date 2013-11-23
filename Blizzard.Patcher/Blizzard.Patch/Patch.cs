using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Blizzard
{
    public class Patch : IDisposable
    {
        PTCH m_PTCH;
        MD5_ m_MD5;
        XFRM m_XFRM;

        // BSD0
        int m_unpackedSize;
        MemoryStream m_compressedDiffStream;

        // BSDIFF40
        BinaryReader m_ctrlBlock;
        MemoryStream m_diffBlock, m_extraBlock;

        string m_type;

        public string PatchType { get { return m_type; } }

        public Patch(string patchFile)
        {
            using (FileStream fs = new FileStream(patchFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                m_PTCH = br.ReadStruct<PTCH>();
                //Debug.Assert(m_PTCH.m_magic.FourCC() == "PTCH");

                if (m_PTCH.m_magic.FourCC() != "PTCH")
                    throw new InvalidDataException("not PTCH");

                m_MD5 = br.ReadStruct<MD5_>();
                Debug.Assert(m_MD5.m_magic.FourCC() == "MD5_");

                m_XFRM = br.ReadStruct<XFRM>();
                Debug.Assert(m_XFRM.m_magic.FourCC() == "XFRM");

                m_type = m_XFRM.m_type.FourCC();

                switch (m_type)
                {
                    case "BSD0":
                        m_unpackedSize = br.ReadInt32();
                        m_compressedDiffStream = new MemoryStream(br.ReadRemaining());
                        BSDIFFParse();
                        break;
                    case "COPY":
                        m_compressedDiffStream = new MemoryStream(br.ReadRemaining());
                        return;
                    default:
                        Debug.Assert(false, String.Format("Unknown patch type: {0}", m_type));
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (m_compressedDiffStream != null)
                m_compressedDiffStream.Close();

            if (m_ctrlBlock != null)
                m_ctrlBlock.Close();

            if (m_diffBlock != null)
                m_diffBlock.Close();

            if (m_extraBlock != null)
                m_extraBlock.Close();
        }

        public void PrintHeaders()
        {
            Console.WriteLine("PTCH: patchSize {0}, sizeBefore {1}, sizeAfter {2}", m_PTCH.m_patchSize, m_PTCH.m_sizeBefore, m_PTCH.m_sizeAfter);
            Console.WriteLine("MD5_: md5BlockSize {0}\n md5Before {1}\n md5After {2}", m_MD5.m_md5BlockSize, m_MD5.m_md5Before.ToHexString(), m_MD5.m_md5After.ToHexString());
            Console.WriteLine("XFRM: xfrmBlockSize {0}, patch type: {1}", m_XFRM.m_xfrmBlockSize, m_XFRM.m_type.FourCC());
        }

        private void BSDIFFParse()
        {
            using (MemoryStream ms = RLEUnpack())
            using (BinaryReader br = new BinaryReader(ms))
            {
                BSDIFF40 m_BSDIFF40 = br.ReadStruct<BSDIFF40>();

                Debug.Assert(m_BSDIFF40.m_magic.FourCC() == "BSDIFF40");

                Debug.Assert(m_BSDIFF40.m_ctrlBlockSize > 0 && m_BSDIFF40.m_diffBlockSize > 0);

                Debug.Assert(m_BSDIFF40.m_sizeAfter == m_PTCH.m_sizeAfter);

                m_ctrlBlock = br.ReadBytes((int)m_BSDIFF40.m_ctrlBlockSize).ToBinaryReader();
                m_diffBlock = new MemoryStream(br.ReadBytes((int)m_BSDIFF40.m_diffBlockSize));
                m_extraBlock = new MemoryStream(br.ReadRemaining());
            }
        }

        private MemoryStream RLEUnpack()
        {
            MemoryStream ret = new MemoryStream(m_unpackedSize);

            using (BinaryReader br = new BinaryReader(m_compressedDiffStream, Encoding.ASCII))
            {
                while (br.PeekChar() >= 0)
                {
                    byte b = br.ReadByte();
                    if ((b & 0x80) != 0)
                    {
                        var bytes = br.ReadBytes((b & 0x7F) + 1);
                        ret.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        var bytes = new byte[b + 1];
                        ret.Write(bytes, 0, bytes.Length);
                    }

                }
            }

            Debug.Assert(ret.Length == m_unpackedSize);

            ret.Position = 0;
            return ret;
        }

        public void Apply(string oldFileName, string newFileName, bool validate)
        {
            if (m_type == "COPY")
            {
                using (var fs = File.OpenWrite(newFileName))
                {
                    m_compressedDiffStream.CopyTo(fs);
                }
                return;
            }

            var oldFileStream = File.OpenRead(oldFileName);

            if (validate)            // pre-validate
            {
                Debug.Assert(oldFileStream.Length == m_PTCH.m_sizeBefore);
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(oldFileStream);
                Debug.Assert(hash.Compare(m_MD5.m_md5Before), "Input MD5 mismatch!");
                oldFileStream.Position = 0;
            }

            string dir = Path.GetDirectoryName(newFileName);

            if (!Directory.Exists(dir) && !String.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var newFileStream = File.Open(newFileName, FileMode.Create);

            int newFileOffset = 0, oldFileOffset = 0;

            while (newFileOffset < m_PTCH.m_sizeAfter)
            {
                var diffChunkSize = m_ctrlBlock.ReadInt32();
                var extraChunkSize = m_ctrlBlock.ReadInt32();
                var extraOffset = m_ctrlBlock.ReadUInt32();

                Debug.Assert(newFileOffset + diffChunkSize <= m_PTCH.m_sizeAfter);

                byte[] newChunk = new byte[diffChunkSize];
                m_diffBlock.Read(newChunk, 0, diffChunkSize);
                newFileStream.Write(newChunk, 0, diffChunkSize);

                byte[] oldChunk = new byte[diffChunkSize];
                oldFileStream.Position = oldFileOffset;
                oldFileStream.Read(oldChunk, 0, diffChunkSize);

                newFileStream.Position = newFileOffset;

                for (int i = 0; i < diffChunkSize; ++i)
                {
                    if ((oldFileOffset + i >= 0) && (oldFileOffset + i < m_PTCH.m_sizeBefore))
                    {
                        var nb = newChunk[i];
                        var ob = oldChunk[i];

                        newFileStream.WriteByte((byte)((nb + ob) % 256));
                    }
                }

                newFileOffset += diffChunkSize;
                oldFileOffset += diffChunkSize;

                Debug.Assert(newFileOffset + extraChunkSize <= m_PTCH.m_sizeAfter);

                byte[] extraChunk = new byte[extraChunkSize];
                m_extraBlock.Read(extraChunk, 0, extraChunkSize);
                newFileStream.Write(extraChunk, 0, extraChunkSize);

                newFileOffset += extraChunkSize;
                oldFileOffset += (int)xsign(extraOffset);
            }

            if (validate)            // post-validate
            {
                newFileStream.Position = 0;
                Debug.Assert(newFileStream.Length == m_PTCH.m_sizeAfter);
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(newFileStream);
                Debug.Assert(hash.Compare(m_MD5.m_md5After), "Output MD5 mismatch!");
            }

            oldFileStream.Close();
            newFileStream.Close();
        }

        private static uint xsign(uint i)
        {
            if ((i & 0x80000000) != 0)
                return (0x80000000 - i);
            return i;
        }
    }
}
