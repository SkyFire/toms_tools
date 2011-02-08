using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace StormLib
{
    // Flags for SFileOpenArchive
    [Flags]
    public enum OpenArchiveFlags : uint
    {
        NO_LISTFILE         = 0x0010, // Don't load the internal listfile
        NO_ATTRIBUTES       = 0x0020, // Don't open the attributes
        MFORCE_MPQ_V1       = 0x0040, // Always open the archive as MPQ v 1.00, ignore the "wFormatVersion" variable in the header
        MCHECK_SECTOR_CRC   = 0x0080, // On files with MPQ_FILE_SECTOR_CRC, the CRC will be checked when reading file
        READ_ONLY           = 0x0100, // Open the archive for read-only access
        ENCRYPTED           = 0x0200, // Opens an encrypted MPQ archive (Example: Starcraft II installation)
    };

    // Values for SFileOpenFileEx
    [Flags]
    public enum OpenFileFlags : uint
    {
        FROM_MPQ        = 0x00000000,  // Open the file from the MPQ archive
        PATCHED_FILE    = 0x00000001,  // Open the file from the MPQ archive
        BY_INDEX        = 0x00000002,  // The 'szFileName' parameter is actually the file index
        ANY_LOCALE      = 0xFFFFFFFE,  // Reserved for StormLib internal use
        LOCAL_FILE      = 0xFFFFFFFF,  // Open the file from the MPQ archive
    };

    public class StormLib
    {
        [DllImport("StormLib.dll")]
        public static extern bool SFileOpenArchive([MarshalAs(UnmanagedType.LPStr)] string szMpqName,
            uint dwPriority, [MarshalAs(UnmanagedType.U4)] OpenArchiveFlags dwFlags, IntPtr phMpq);

        [DllImport("StormLib.dll")]
        public static extern bool SFileCloseArchive(IntPtr hMpq);

        [DllImport("StormLib.dll")]
        public static extern bool SFileOpenFileEx(IntPtr hMpq, [MarshalAs(UnmanagedType.LPStr)] string szFileName,
            [MarshalAs(UnmanagedType.U4)] OpenFileFlags dwSearchScope, IntPtr phFile);

        [DllImport("StormLib.dll")]
        public static extern bool SFileCloseFile(IntPtr hFile);

        [DllImport("StormLib.dll")]
        public static extern uint SFileGetFileSize(IntPtr hFile, out uint pdwFileSizeHigh);

        [DllImport("StormLib.dll")]
        public static extern bool SFileReadFile(IntPtr hFile, [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer, int dwToRead,
            out int pdwRead, IntPtr lpOverlapped);

        [DllImport("StormLib.dll")]
        public static extern bool SFileExtractFile(IntPtr hMpq,
            [MarshalAs(UnmanagedType.LPStr)] string szToExtract,
            [MarshalAs(UnmanagedType.LPStr)] string szExtracted);

        [DllImport("StormLib.dll")]
        public static extern bool SFileHasFile(IntPtr hMpq, [MarshalAs(UnmanagedType.LPStr)] string szFileName);

        [DllImport("StormLib.dll")]
        public static extern bool SFileOpenPatchArchive(IntPtr hMpq, [MarshalAs(UnmanagedType.LPStr)] string szMpqName,
            [MarshalAs(UnmanagedType.LPStr)] string szPatchPathPrefix, uint dwFlags);
    }

    public class MpqArchiveSet : IDisposable
    {
        private List<MpqArchive> archives = new List<MpqArchive>();
        private string GameDir = ".\\";

        public void SetGameDir(string dir)
        {
            GameDir = dir;
        }

        public static string GetGameDirFromReg()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft");
            if (key == null)
                return null;
            Object val = key.GetValue("InstallPath");
            if (val == null)
                return null;
            return val.ToString();
        }

        public bool AddArchive(string file)
        {
            MpqArchive a = new MpqArchive(GameDir + file, 0, OpenArchiveFlags.READ_ONLY);
            if (a.IsOpen)
            {
                archives.Add(a);
                Console.WriteLine("Add archive: {0}", file);
                return true;
            }
            return false;
        }

        public int AddArchives(string[] files)
        {
            int n = 0;
            foreach (string s in files)
                if (AddArchive(s))
                    n++;
            return n;
        }

        public bool HasFile(string name)
        {
            foreach (MpqArchive a in archives)
                if (a.HasFile(name))
                    return true;
            return false;
        }

        public bool ExtractFile(string from, string to)
        {
            foreach (MpqArchive a in archives)
                if (a.HasFile(from))
                    return a.ExtractPatchedFile(from, to);
            return false;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            foreach (MpqArchive a in archives)
                a.Close();
            archives.Clear();
        }
    }

    public class MpqLocale
    {
        private static readonly string[] Locales = new string[] {
            "enUS", "koKR", "frFR", "deDE", "zhTW", "esES", "esMX", "ruRU", "enGB", "enTW" };

        public static string GetPrefix(string file)
        {
            foreach (var loc in Locales)
                if (file.Contains(loc))
                    return loc;
            return "base";
        }
    }

    public class MpqArchive : IDisposable
    {
        private IntPtr handle = IntPtr.Zero;

        public MpqArchive(string file, uint Prio, OpenArchiveFlags Flags)
        {
            bool r = Open(file, Prio, Flags);
        }

        public bool IsOpen { get { return handle != IntPtr.Zero; } }

        private unsafe bool Open(string file, uint Prio, OpenArchiveFlags Flags)
        {
            IntPtr h;
            IntPtr hp = (IntPtr)(&h);
            bool r = StormLib.SFileOpenArchive(file, Prio, Flags, hp);
            if (r)
            {
                handle = h;
                OpenPatch(file);
            }
            return r;
        }

        private void OpenPatch(string file)
        {
            var patches = Directory.GetFiles(MpqArchiveSet.GetGameDirFromReg(), "Data\\wow-update-*.mpq");

            var prefix = MpqLocale.GetPrefix(file);

            foreach (var patch in patches)
            {
                // hack due to multiple variants of game world in current client (world.mpq + oldworld.mpq), which can't be used at same time
                if (patch.Contains("oldworld"))
                    continue;

                bool r = StormLib.SFileOpenPatchArchive(handle, patch, prefix, 0);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public bool Close()
        {
            bool r = StormLib.SFileCloseArchive(handle);
            if (r)
                handle = IntPtr.Zero;
            return r;
        }

        public unsafe MpqFile OpenFile(string szFileName, OpenFileFlags dwSearchScope)
        {
            IntPtr h;
            IntPtr hp = (IntPtr)(&h);
            bool r = StormLib.SFileOpenFileEx(handle, szFileName, dwSearchScope, hp);
            if (!r)
                return null;
            return new MpqFile(this, h);
        }

        public bool HasFile(string name)
        {
            return StormLib.SFileHasFile(handle, name);
        }

        public bool ExtractFile(string from, string to)
        {
            return StormLib.SFileExtractFile(handle, from, to);
        }

        public bool ExtractPatchedFile(string from, string to)
        {
            using (MpqFile f = OpenFile(from, OpenFileFlags.PATCHED_FILE))
            {
                if (f == null)
                    return false;

                return f.ExtractTo(to);
            }
        }
    }

    public class MpqFile : IDisposable
    {
        IntPtr handle;
        MpqArchive archive;

        public MpqFile(MpqArchive a, IntPtr h)
        {
            archive = a;
            handle = h;
        }

        public void Dispose()
        {
            Close();
        }

        public bool Close()
        {
            bool r = StormLib.SFileCloseFile(handle);
            if (r)
                handle = IntPtr.Zero;
            return r;
        }

        public uint GetSize()
        {
            uint high;
            return StormLib.SFileGetFileSize(handle, out high);
        }

        public bool ExtractTo(string to)
        {
            uint dwSize = GetSize();

            if (dwSize == 0)
                return false;

            // hope we won't run OOM
            byte[] szBuffer = new byte[dwSize];

            using (FileStream file = File.Create(to))
            {
                int dwBytes;

                bool r = StormLib.SFileReadFile(handle, szBuffer, szBuffer.Length, out dwBytes, IntPtr.Zero);

                if (dwBytes != szBuffer.Length)
                    throw new IOException(String.Format("Can't extract {0} properly!", to));

                if (dwBytes > 0)
                    file.Write(szBuffer, 0, dwBytes);

                return r;
            }
        }
    }
}
