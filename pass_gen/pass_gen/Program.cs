using System;
using System.Text;
using System.Security.Cryptography;

namespace pass_gen
{
    class Program
    {
        static void Main(string[] args)
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();

            Console.Write("Enter username: ");
            string username = Console.ReadLine().ToUpper();
            Console.Write("Enter password: ");
            string password = Console.ReadLine().ToUpper();

            string temp = username + ":" + password;

            byte[] temp2 = Encoding.ASCII.GetBytes(temp);

            sha.ComputeHash(temp2, 0, temp2.Length);

            string hash = String.Empty;

            for (int i = 0; i < sha.Hash.Length; i++)
            {
                hash += sha.Hash[i].ToString("X2");
            }

            Console.WriteLine("Hash: {0}", hash);
            Console.ReadKey();
        }
    }
}
