using System;
using System.IO;

namespace WdbParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string folderName = args.Length == 0 ? "wdb" : args[0];
            if (!Directory.Exists(folderName))
            {
                Console.WriteLine("Please specify folder");
                return;
            }
            var wdbParser = new WdbParser(folderName);
        }
    }
}
