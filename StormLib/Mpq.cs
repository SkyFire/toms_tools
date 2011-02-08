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

        public static bool ExtractFile(string from, string to)
        {
            if (!archive.HasFile(from))
                return false;

            var dir = Path.GetDirectoryName(to);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return archive.ExtractFile(from, to);
        }

        public static void Close()
        {
            archive.Close();
        }
    }
}
