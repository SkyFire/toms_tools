using System;
using System.IO;
using StormLib;

namespace Test
{
    static class Mpq
    {
        static readonly string[] archiveNames = {
                    "Expansion3.mpq",
                    "Expansion2.mpq",
                    "Expansion1.mpq",
                    "world.mpq",
                    "art.mpq",
                    "enGB\\locale-enGB.MPQ" };

        static readonly MpqArchiveSet archive = new MpqArchiveSet();
        static readonly string regGameDir = MpqArchiveSet.GetGameDirFromReg();

        static Mpq()
        {
            var dir = Path.Combine(regGameDir, "Data\\");
            archive.SetGameDir(dir);

            Console.WriteLine("Game dir is {0}", dir);

            archive.AddArchives(archiveNames);
        }

        public static bool ExtractFile(string from, string to, OpenFile dwSearchScope)
        {
            return archive.ExtractFile(from, to, dwSearchScope);
        }

        public static void Close()
        {
            archive.Close();
        }
    }
}
