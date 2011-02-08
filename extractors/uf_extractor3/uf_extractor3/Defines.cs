using System;
using System.Runtime.InteropServices;

namespace uf_extractor2
{
    [StructLayout(LayoutKind.Sequential)]
    struct UpdateField
    {
        public string Name;
        public long Pos;
        public long Total;
        public string Descr;

        public UpdateField(string name, long pos, long total, string descr)
        {
            Name = name;
            Pos = pos;
            Total = total;
            Descr = descr;
        }
    };

    [Flags]
    enum UpdatafieldFlags
    {
        NONE = 0,
        PUBLIC = 1,
        PRIVATE = 2,
        OWNER_ONLY = 4,
        UNK1 = 8,
        UNK2 = 16,
        UNK3 = 32,
        GROUP_ONLY = 64,
        UNK4 = 128,
        DYNAMIC = 256
    };

    enum UpdateFieldType
    {
        NONE = 0,
        INT = 1,
        TWO_SHORT = 2,
        FLOAT = 3,
        LONG = 4,
        BYTES = 5
    };
}
