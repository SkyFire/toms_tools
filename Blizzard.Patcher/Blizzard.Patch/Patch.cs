using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Blizzard
{
    public class Patch : IDisposable
    {
        PTCH m_PTCH;
        MD5_ m_MD5;
        XFRM m_XFRM;
        BSDIFF40 m_BSDIFF40;

        // BSD0
        uint m_unpackedSize;
        byte[] m_compressedDiff;

        // BSDIFF40
        byte[] m_ctrlBlock, m_diffBlock, m_extraBlock;

        string m_type;

        public Patch(string patchFile)
        {
            using (FileStream fs = new FileStream(patchFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                m_PTCH = br.ReadStruct<PTCH>();
                Debug.Assert(m_PTCH.m_magic.FourCC() == "PTCH");

                m_MD5 = br.ReadStruct<MD5_>();
                Debug.Assert(m_MD5.m_magic.FourCC() == "MD5_");

                m_XFRM = br.ReadStruct<XFRM>();
                Debug.Assert(m_XFRM.m_magic.FourCC() == "XFRM");

                m_type = m_XFRM.m_type.FourCC();

                switch (m_type)
                {
                    case "BSD0":
                        m_unpackedSize = br.ReadUInt32();
                        m_compressedDiff = br.ReadRemaining();
                        BSDIFFParse();
                        break;
                    case "COPY":
                        m_compressedDiff = br.ReadRemaining();
                        return;
                    default:
                        Debug.Assert(false, String.Format("Unknown patch type: {0}", m_type));
                        break;
                }
            }
        }

        public void Dispose()
        {
            // TODO
        }

        public void PrintHeaders()
        {
            Console.WriteLine("PTCH: patchSize {0}, sizeBefore {1}, sizeAfter {2}", m_PTCH.m_patchSize, m_PTCH.m_sizeBefore, m_PTCH.m_sizeAfter);
            Console.WriteLine("MD5_: md5BlockSize {0}\n md5Before {1}\n md5After {2}", m_MD5.m_md5BlockSize, m_MD5.m_md5Before.ToHexString(), m_MD5.m_md5After.ToHexString());
            Console.WriteLine("XFRM: xfrmBlockSize {0}, patch type: {1}", m_XFRM.m_xfrmBlockSize, m_XFRM.m_type.FourCC());
        }

        private void BSDIFFParseHeader(BinaryReader br)
        {
            m_BSDIFF40 = br.ReadStruct<BSDIFF40>();

            Debug.Assert(m_BSDIFF40.m_magic.FourCC() == "BSDIFF40");

            Debug.Assert(m_BSDIFF40.m_ctrlBlockSize > 0 && m_BSDIFF40.m_diffBlockSize > 0);

            Debug.Assert(m_BSDIFF40.m_sizeAfter == m_PTCH.m_sizeAfter);
        }

        private void BSDIFFParse()
        {
            var diff = RLEUnpack();

            using (MemoryStream ms = new MemoryStream(diff))
            using (BinaryReader br = new BinaryReader(ms))
            {
                BSDIFFParseHeader(br);

                m_ctrlBlock = br.ReadBytes((int)m_BSDIFF40.m_ctrlBlockSize);
                m_diffBlock = br.ReadBytes((int)m_BSDIFF40.m_diffBlockSize);
                m_extraBlock = br.ReadRemaining();
            }
        }

        private byte[] RLEUnpack()
        {
            List<byte> ret = new List<byte>();

            using (MemoryStream ms = new MemoryStream(m_compressedDiff))
            using (BinaryReader br = new BinaryReader(ms))
            {
                while (br.PeekChar() >= 0)
                {
                    byte b = br.ReadByte();
                    if ((b & 0x80) != 0)
                        ret.AddRange(br.ReadBytes((b & 0x7F) + 1));
                    else
                        ret.AddRange(new byte[b + 1]);
                }
            }

            Debug.Assert(ret.Count == m_unpackedSize);

            return ret.ToArray();
        }

        public void Apply(string oldFileName, string newFileName, bool validate)
        {
            if (m_type == "COPY")
            {
                File.WriteAllBytes(newFileName, m_compressedDiff);
                return;
            }

            byte[] oldFile = File.ReadAllBytes(oldFileName);

            if (validate)            // pre-validate
            {
                Debug.Assert(oldFile.Length == m_PTCH.m_sizeBefore);
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(oldFile);
                Debug.Assert(hash.Compare(m_MD5.m_md5Before), "Input MD5 mismatch!");
            }

            var ctrlBlock = m_ctrlBlock.ToBinaryReader();
            var diffBlock = m_diffBlock.ToBinaryReader();
            var extraBlock = m_extraBlock.ToBinaryReader();

            byte[] newFile = new byte[m_PTCH.m_sizeAfter];

            int newFileOffset = 0, oldFileOffset = 0;

            while (newFileOffset < m_PTCH.m_sizeAfter)
            {
                var diffChunkSize = ctrlBlock.ReadInt32();
                var extraChunkSize = ctrlBlock.ReadInt32();
                var extraOffset = ctrlBlock.ReadUInt32();

                Debug.Assert(newFileOffset + diffChunkSize <= m_PTCH.m_sizeAfter);

                newFile.SetBytes(diffBlock.ReadBytes(diffChunkSize), newFileOffset);

                for (int i = 0; i < diffChunkSize; ++i)
                {
                    if ((oldFileOffset + i >= 0) && (oldFileOffset + i < m_PTCH.m_sizeBefore))
                    {
                        var nb = newFile[newFileOffset + i];
                        var ob = oldFile[oldFileOffset + i];
                        newFile[newFileOffset + i] = (byte)((nb + ob) % 256);
                    }
                }

                newFileOffset += diffChunkSize;
                oldFileOffset += diffChunkSize;

                Debug.Assert(newFileOffset + extraChunkSize <= m_PTCH.m_sizeAfter);

                newFile.SetBytes(extraBlock.ReadBytes(extraChunkSize), newFileOffset);

                newFileOffset += extraChunkSize;
                oldFileOffset += (int)xsign(extraOffset);
            }

            ctrlBlock.Close();
            diffBlock.Close();
            extraBlock.Close();

            if (validate)            // post-validate
            {
                Debug.Assert(newFile.Length == m_PTCH.m_sizeAfter);
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(newFile);
                Debug.Assert(hash.Compare(m_MD5.m_md5After), "Output MD5 mismatch!");
            }

            File.WriteAllBytes(newFileName, newFile);
        }

        private static uint xsign(uint i)
        {
            if ((i & 0x80000000) != 0)
                return (0x80000000 - i);
            return i;
        }
    }
}
