using System.Collections.Generic;

namespace dbc2sql
{
    class StringTable : Dictionary<int, string>
    {
        public StringTable()
            : base()
        {
        }

        public new string this[int offset]
        {
            get { return base[offset]; }
            set { base[offset] = value; }
        }
    }
}
