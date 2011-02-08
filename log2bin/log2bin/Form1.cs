using System;
using System.Windows.Forms;
using System.IO;

namespace log2bin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo("."); // DirectoryInfo
            FileInfo[] fi = di.GetFiles("tcp*.log", SearchOption.AllDirectories); // Get file list

            foreach (FileInfo f in fi)
            {
                Console.WriteLine("Executing {0}", f.Name);

                string ofn = f.FullName + ".dump";

                BinaryWriter bw = new BinaryWriter(new FileStream(ofn, FileMode.Create));
                StreamReader sr = new StreamReader(f.FullName);

                while (sr.Peek() >= 0)
                {
                    string temp1 = sr.ReadLine();
                    if (temp1.StartsWith("Packet SMSG.COMPRESSED_UPDATE_OBJECT (502)"))
                    {
                        // read additional line
                        string temp2 = sr.ReadLine();
                        if (temp2.Contains("decompressed"))
                        {
                            string[] temp = temp2.Split('=');
                            uint size = Convert.ToUInt32(temp[1].Substring(0, temp[1].Length - 1));
                            bw.Write(size);
                        }
                    }
                    else if (temp1.StartsWith("Packet SMSG") && IsDataPresent(sr))
                    {
                        string[] temp = temp1.Split(' ');
                        uint size = Convert.ToUInt32(temp[4]);
                        bw.Write(size);
                    }
                    else if (temp1.StartsWith("Packet SMSG") && !IsDataPresent(sr))
                    {
                        sr.ReadLine();
                    }
                    else if (temp1.StartsWith("Log ") || temp1.Contains("Search string found at"))
                    {
                        ;
                    }
                    else
                    {
                        string[] test3 = temp1.Split(' ');
                        for (int j = 1; j < test3.Length - 2; j++) // skip not needed stuff
                        {
                            if (j < 1 || j > 0x10)
                                continue;

                            if (test3[j].Contains("-") == false || !test3[j].Contains(":") == false) // skip bytes at end of packet
                            {
                                bw.Write(Convert.ToByte(test3[j], 16));
                            }
                        }
                    }
                }
                bw.Flush();
                bw.Close();
            }
            MessageBox.Show("Done!");
        }

        public bool IsDataPresent(StreamReader streamreader)
        {
            // "(" or "0"
            //if (streamreader.Peek() == 0x28)
            //    return true;
            if (streamreader.Peek() == 0x30)
                return true;
            return false;
        }
    }
}
