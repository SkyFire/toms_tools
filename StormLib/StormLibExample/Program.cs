using System;
using StormLib;
using Test;

namespace StormLibExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Mpq.ExtractFile("DBFilesClient\\Item-sparse.db2", "Item-sparse.db2", OpenFile.PATCHED_FILE);
            Mpq.ExtractFile("DBFilesClient\\ItemCurrencyCost.db2", "ItemCurrencyCost.db2", OpenFile.PATCHED_FILE);
            
            Mpq.Close();

            Console.ReadKey();
        }
    }
}
