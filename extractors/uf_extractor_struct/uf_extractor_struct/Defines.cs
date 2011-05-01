using System;
using System.Runtime.InteropServices;

namespace uf_extractor_struct
{
    [StructLayout(LayoutKind.Sequential)]
    struct UpdateField
    {
        public string Name;
        public long Pos;
        public long Total;
        public UpdateFieldType Type;
        public UpdatafieldFlags Flags;

        public UpdateField(string name, long pos, long total, UpdateFieldType type, UpdatafieldFlags flags)
        {
            Name = name;
            Pos = pos;
            Total = total;
            Type = type;
            Flags = flags;
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
        DWORD = 1,
        WORD = 2,
        @float = 3,
        WGUID = 4,
        @char = 5,
        @WORD_char_2 = 6,
    };
}
