using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WowTools.Core
{
    public abstract class Parser
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public void Append(string str)
        {
            stringBuilder.Append(str);
        }

        public void AppendLine()
        {
            stringBuilder.AppendLine();
        }

        public void AppendLine(string str)
        {
            stringBuilder.AppendLine(str);
        }

        public void AppendFormat(string format, params object[] args)
        {
            stringBuilder.AppendFormat(format, args);
        }

        public void AppendFormatLine(string format, params object[] args)
        {
            stringBuilder.AppendFormat(format, args).AppendLine();
        }

        public void CheckPacket(BinaryReader gr)
        {
            if (gr.BaseStream.Position != gr.BaseStream.Length)
            {
                string msg = String.Format("{0}: Packet size changed, should be {1} instead of {2}", Packet.Code, gr.BaseStream.Position, gr.BaseStream.Length);
                MessageBox.Show(msg);
            }
        }

        protected Packet Packet { get; private set; }

        protected Parser(Packet packet)
        {
            Packet = packet;
            Parse();
        }

        public abstract void Parse();

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
