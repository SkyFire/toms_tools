using System;

namespace Defines
{
    [Flags]
    enum Flags
    {
        flag01 = 0x00000001,
        flag02 = 0x00000002,
        flag03 = 0x00000004,
        flag04 = 0x00000008,
        flag05 = 0x00000010,
        flag06 = 0x00000020,
        flag07 = 0x00000040,
        flag08 = 0x00000080,
        flag09 = 0x00000100, // running
        flag10 = 0x00000200, // taxi
        flag11 = 0x00000400,
        flag12 = 0x00000800,
        flag13 = 0x00001000,
        flag14 = 0x00002000,
        flag15 = 0x00004000,
        flag16 = 0x00008000,
        flag17 = 0x00010000,
        flag18 = 0x00020000,
        flag19 = 0x00040000,
        flag20 = 0x00080000,
        flag21 = 0x00100000,
        flag22 = 0x00200000,
        flag23 = 0x00400000,
        flag24 = 0x00800000,
        flag25 = 0x01000000,
        flag26 = 0x02000000,
        flag27 = 0x04000000,
        flag28 = 0x08000000,
        flag29 = 0x10000000,
        flag30 = 0x20000000,
        flag31 = 0x40000000
    };

    struct Node
    {
        public float x;
        public float y;
        public float z;
    };

    enum TrainerType
    {
        type1 = 0,
        type2 = 1,
        type3 = 2,
        type4 = 3,
        type5 = 4
    };

    enum TrainerSpellState
    {
        TRAINER_SPELL_GREEN = 0,
        TRAINER_SPELL_RED = 1,
        TRAINER_SPELL_GRAY = 2
    };

    [Flags]
    enum HitInfo
    {
        HITINFO_NORMALSWING = 0x00000000,
        HITINFO_UNK1 = 0x00000001,
        HITINFO_NORMALSWING2 = 0x00000002,
        HITINFO_LEFTSWING = 0x00000004,
        HITINFO_UNK2 = 0x00000008,
        HITINFO_MISS = 0x00000010,
        HITINFO_ABSORB = 0x00000020,
        HITINFO_RESIST = 0x00000040,
        HITINFO_CRITICALHIT = 0x00000080,
        HITINFO_UNK3 = 0x00000100,
        HITINFO_UNK4 = 0x00000200,
        HITINFO_UNK5 = 0x00000400,
        HITINFO_UNK6 = 0x00000800,
        HITINFO_UNK7 = 0x00001000,
        HITINFO_UNK8 = 0x00002000,
        HITINFO_GLANCING = 0x00004000,
        HITINFO_CRUSHING = 0x00008000,
        HITINFO_NOACTION = 0x00010000,
        HITINFO_UNK9 = 0x00020000,
        HITINFO_UNK10 = 0x00040000,
        HITINFO_SWINGNOHITSOUND = 0x00080000,
        HITINFO_UNK11 = 0x00100000,
        HITINFO_UNK12 = 0x00200000,
        HITINFO_UNK13 = 0x00400000,
        HITINFO_UNK14 = 0x00800000,
        HITINFO_UNK15 = 0x01000000,
        HITINFO_UNK16 = 0x02000000,
        HITINFO_UNK17 = 0x04000000,
        HITINFO_UNK18 = 0x08000000,
        HITINFO_UNK19 = 0x10000000,
        HITINFO_UNK20 = 0x20000000,
        HITINFO_UNK21 = 0x40000000
    };

    enum ITEM_DAMAGE_TYPE
    {
        NORMAL_DAMAGE = 0,
        HOLY_DAMAGE = 1,
        FIRE_DAMAGE = 2,
        NATURE_DAMAGE = 3,
        FROST_DAMAGE = 4,
        SHADOW_DAMAGE = 5,
        ARCANE_DAMAGE = 6
    };

    enum VictimState
    {
        VICTIMSTATE_UNKNOWN1 = 0, // may be miss?
        VICTIMSTATE_NORMAL = 1,
        VICTIMSTATE_DODGE = 2,
        VICTIMSTATE_PARRY = 3,
        VICTIMSTATE_UNKNOWN2 = 4,
        VICTIMSTATE_BLOCKS = 5,
        VICTIMSTATE_EVADES = 6,
        VICTIMSTATE_IS_IMMUNE = 7,
        VICTIMSTATE_DEFLECTS = 8
    };

    enum Races
    {
        RACE_HUMAN = 1,
        RACE_ORC = 2,
        RACE_DWARF = 3,
        RACE_NIGHTELF = 4,
        RACE_UNDEAD_PLAYER = 5,
        RACE_TAUREN = 6,
        RACE_GNOME = 7,
        RACE_TROLL = 8,
        RACE_GOBLIN = 9,
        RACE_BLOODELF = 10,
        RACE_DRAENEI = 11,
        RACE_FEL_ORC = 12,
        RACE_NAGA = 13,
        RACE_BROKEN = 14,
        RACE_SKELETON = 15
    };

    enum Class
    {
        CLASS_WARRIOR = 1,
        CLASS_PALADIN = 2,
        CLASS_HUNTER = 3,
        CLASS_ROGUE = 4,
        CLASS_PRIEST = 5,
        // CLASS_UNK1   = 6, unused
        CLASS_SHAMAN = 7,
        CLASS_MAGE = 8,
        CLASS_WARLOCK = 9,
        // CLASS_UNK2   = 10,unused
        CLASS_DRUID = 11
    };

    enum Gender
    {
        MALE = 0,
        FEMALE = 1,
        NOSEX = 2
    };

    enum Powers
    {
        POWER_MANA = 0,
        POWER_RAGE = 1,
        POWER_FOCUS = 2,
        POWER_ENERGY = 3,
        POWER_HAPPINESS = 4
    };

    [Flags]
    enum ProfessionFlags
    {
        PROF_FLAG_NONE = 0,
        PROF_FLAG_PASSIVE = 1,
        PROF_FLAG_SECONDARY = 2,
        PROF_FLAG_PRIMARY = 4,
    };

    [Flags]
    enum GroupUpdateFlags
    {
        GROUP_UPDATE_FLAG_NONE = 0x00000000,
        GROUP_UPDATE_FLAG_ONLINE = 0x00000001, // uint8, flags
        GROUP_UPDATE_FLAG_CUR_HP = 0x00000002, // uint16
        GROUP_UPDATE_FLAG_MAX_HP = 0x00000004, // uint16
        GROUP_UPDATE_FLAG_POWER_TYPE = 0x00000008, // uint8
        GROUP_UPDATE_FLAG_CUR_POWER = 0x00000010, // uint16
        GROUP_UPDATE_FLAG_MAX_POWER = 0x00000020, // uint16
        GROUP_UPDATE_FLAG_LEVEL = 0x00000040, // uint16
        GROUP_UPDATE_FLAG_ZONE = 0x00000080, // uint16
        GROUP_UPDATE_FLAG_POSITION = 0x00000100, // uint16, uint16
        GROUP_UPDATE_FLAG_AURAS = 0x00000200, // uint64 mask, for each bit set uint16 spellid?
        GROUP_UPDATE_FLAG_PET_GUID = 0x00000400, // uint64 pet guid
        GROUP_UPDATE_FLAG_PET_NAME = 0x00000800, // pet name, NULL terminated string
        GROUP_UPDATE_FLAG_PET_MODEL_ID = 0x00001000, // uint16, model id
        GROUP_UPDATE_FLAG_PET_CUR_HP = 0x00002000, // uint16 pet cur health
        GROUP_UPDATE_FLAG_PET_MAX_HP = 0x00004000, // uint16 pet max health
        GROUP_UPDATE_FLAG_PET_POWER_TYPE = 0x00008000, // uint8 pet power type
        GROUP_UPDATE_FLAG_PET_CUR_POWER = 0x00010000, // uint16 pet cur power
        GROUP_UPDATE_FLAG_PET_MAX_POWER = 0x00020000, // uint16 pet max power
        GROUP_UPDATE_FLAG_PET_AURAS = 0x00040000, // uint64 mask, for each bit set uint16 spellid?, pet auras...
    };

    [Flags]
    enum GroupMemberOnlineStatus
    {
        MEMBER_STATUS_OFFLINE = 0x0000,
        MEMBER_STATUS_ONLINE = 0x0001,
        MEMBER_STATUS_PVP = 0x0002,
        MEMBER_STATUS_UNK0 = 0x0004, // dead? (health=0)
        MEMBER_STATUS_UNK1 = 0x0008, // ghost? (health=1)
        MEMBER_STATUS_UNK2 = 0x0010, // never seen
        MEMBER_STATUS_UNK3 = 0x0020, // never seen
        MEMBER_STATUS_UNK4 = 0x0040, // appears with dead and ghost flags
        MEMBER_STATUS_UNK5 = 0x0080, // never seen
        MEMBER_STATUS_UNK6 = 0x0100,
        MEMBER_STATUS_UNK7 = 0x0200,
        MEMBER_STATUS_UNK8 = 0x0400,
        MEMBER_STATUS_UNK9 = 0x0800,
        MEMBER_STATUS_UNK10 = 0x1000,
        MEMBER_STATUS_UNK11 = 0x2000,
        MEMBER_STATUS_UNK12 = 0x4000,
        MEMBER_STATUS_UNK13 = 0x8000,
    };

    [Flags]
    enum MovementFlags
    {
        MOVEMENTFLAG_NONE = 0x00000000,
        MOVEMENTFLAG_FORWARD = 0x00000001,
        MOVEMENTFLAG_BACKWARD = 0x00000002,
        MOVEMENTFLAG_STRAFE_LEFT = 0x00000004,
        MOVEMENTFLAG_STRAFE_RIGHT = 0x00000008,
        MOVEMENTFLAG_LEFT = 0x00000010,
        MOVEMENTFLAG_RIGHT = 0x00000020,
        MOVEMENTFLAG_PITCH_UP = 0x00000040,
        MOVEMENTFLAG_PITCH_DOWN = 0x00000080,
        MOVEMENTFLAG_WALK = 0x00000100,
        MOVEMENTFLAG_ONTRANSPORT = 0x00000200,
        MOVEMENTFLAG_0x400 = 0x00000400,
        MOVEMENTFLAG_FLY_UNK1 = 0x00000800,
        MOVEMENTFLAG_JUMPING = 0x00001000,
        MOVEMENTFLAG_UNK4 = 0x00002000,
        MOVEMENTFLAG_FALLING = 0x00004000,
        MOVEMENTFLAG_0x8000 = 0x00008000,
        MOVEMENTFLAG_0x10000 = 0x00010000,
        MOVEMENTFLAG_0x20000 = 0x00020000,
        MOVEMENTFLAG_0x40000 = 0x00040000,
        MOVEMENTFLAG_0x80000 = 0x00080000,
        MOVEMENTFLAG_0x100000 = 0x00100000,
        MOVEMENTFLAG_SWIMMING = 0x00200000,               // appears with fly flag also
        MOVEMENTFLAG_FLY_UP = 0x00400000,
        MOVEMENTFLAG_CAN_FLY = 0x00800000,
        MOVEMENTFLAG_FLYING = 0x01000000,
        MOVEMENTFLAG_UNK5 = 0x02000000,
        MOVEMENTFLAG_SPLINE = 0x04000000,               // probably wrong name
        MOVEMENTFLAG_SPLINE2 = 0x08000000,
        MOVEMENTFLAG_WATERWALKING = 0x10000000,
        MOVEMENTFLAG_SAFE_FALL = 0x20000000,               // active rogue safe fall spell (passive)
        MOVEMENTFLAG_UNK3 = 0x40000000
    };

    [Flags]
    enum SplineFlags
    {
        SPLINE_FLAG_1 = 0x10000,
        SPLINE_FLAG_2 = 0x20000,
        SPLINE_FLAG_3 = 0x40000
    };
}
