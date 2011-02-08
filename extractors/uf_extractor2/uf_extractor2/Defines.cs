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
        OWNER = 4,
        UNUSED1 = 8,
        ITEM_OWNER = 16,
        PARTY_LEADER = 32, // UNIT_OWNER may be?
        PARTY_MEMBER = 64,
        UNUSED2 = 128,
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
