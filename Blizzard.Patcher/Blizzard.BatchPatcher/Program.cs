using System;
using System.IO;
using System.Linq;

namespace Blizzard.BatchPatcher
{
    class Program
    {
        static string[] skipFiles = new string[]
        {
            "(attributes)",
            "(listfile)",
            "base-Win-md5.lst"
        };

        static void Main(string[] args)
        {
            //using (Patch p = new Patch(args[1]))
            //{
            //    p.PrintHeaders();
            //    p.Apply(args[0], args[2], true);
            //}

            string[] oldfiles = Directory.GetFiles(args[0], "*.*", SearchOption.AllDirectories);

            string[] ptchs = Directory.GetFiles(args[1], "*.*", SearchOption.AllDirectories);

            foreach (string of in oldfiles)
            {
                if (skipFiles.Contains(Path.GetFileName(of)))
                    continue;

                if (ptchs.Any(s => s.Substring(args[1].Length) == of.Substring(args[0].Length)))
                {
                    Console.WriteLine("has patch for {0}", of);

                    //string patch = Path.Combine(args[1], of.Substring(args[0].Length));
                    string patch = args[1] + of.Substring(args[0].Length);

                    FileInfo fi = new FileInfo(patch);
                    if (fi.Length == 0) // removed
                        continue;

                    try
                    {
                        using (Patch p = new Patch(patch))
                        {
                            //p.PrintHeaders();
                            p.Apply(of, args[2] + of.Substring(args[0].Length), true);
                        }
                    }
                    catch (InvalidDataException)
                    {
                        File.Copy(of, args[2] + of.Substring(args[0].Length), true);
                    }
                }
                else
                {
                    string file = args[2] + of.Substring(args[0].Length);

                    if (!Directory.Exists(Path.GetDirectoryName(file)))
                        Directory.CreateDirectory(Path.GetDirectoryName(file));

                    File.Copy(of, file, true);
                }
            }

            foreach (string pf in ptchs)
            {
                FileInfo fi = new FileInfo(pf);
                if (fi.Length == 0) // removed
                    continue;

                string newfile = args[2] + pf.Substring(args[1].Length);

                try
                {
                    using (Patch p = new Patch(pf))
                    {
                        //p.PrintHeaders();
                        if (p.PatchType != "COPY")
                            continue;

                        Console.WriteLine("new file {0}", newfile);

                        p.Apply("oldfile", newfile, true);
                    }
                }
                catch (InvalidDataException)
                {
                    File.Copy(pf, newfile, true);
                }
            }
        }
    }
}
