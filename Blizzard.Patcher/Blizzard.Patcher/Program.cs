using System;

namespace Blizzard
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: [oldFile] [patchFile] [newFile]");
                return;
            }

            using (Patch p = new Patch(args[1]))
            {
                p.PrintHeaders();
                p.Apply(args[0], args[2], true);
            }
        }
    }
}
