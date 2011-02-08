using System;

namespace a9_parser
{
    /// <summary>
    /// WoW Update Types.
    /// </summary>
    public enum UpdateTypes
    {
        /// <summary>
        /// Update type that update only object field values.
        /// </summary>
        UPDATETYPE_VALUES = 0,
        /// <summary>
        /// Update type that update only object movement.
        /// </summary>
        UPDATETYPE_MOVEMENT = 1,
        /// <summary>
        /// Update type that create an object (full update).
        /// </summary>
        UPDATETYPE_CREATE_OBJECT = 2,
        /// <summary>
        /// Update type that create an object (gull update, self use).
        /// </summary>
        UPDATETYPE_CREATE_OBJECT2 = 3,
        /// <summary>
        /// Update type that update only objects out of range.
        /// </summary>
        UPDATETYPE_OUT_OF_RANGE_OBJECTS = 4,
        /// <summary>
        /// Update type that update only near objects.
        /// </summary>
        UPDATETYPE_NEAR_OBJECTS = 5,
    }

    /// <summary>
    /// WoW Update Flags
    /// </summary>
    [Flags]
    public enum UpdateFlags
    {
        /// <summary>
        /// Update flag for self.
        /// </summary>
        UPDATEFLAG_SELFTARGET = 0x01,
        /// <summary>
        /// Update flag for transport object.
        /// </summary>
        UPDATEFLAG_TRANSPORT = 0x02,
        /// <summary>
        /// Update flag with full guid.
        /// </summary>
        UPDATEFLAG_FULLGUID = 0x04,
        /// <summary>
        /// Update flag with high guid.
        /// </summary>
        UPDATEFLAG_HIGHGUID = 0x08,
        /// <summary>
        /// Common update flag.
        /// </summary>
        UPDATEFLAG_ALL = 0x10,
        /// <summary>
        /// Update flag for living objects.
        /// </summary>
        UPDATEFLAG_LIVING = 0x20,
        /// <summary>
        /// Update flag for world objects (players, units, go, do, corpses).
        /// </summary>
        UPDATEFLAG_HASPOSITION = 0x40
    }

    /// <summary>
    /// WoW object types.
    /// </summary>
    public enum ObjectTypes
    {
        /// <summary>
        /// An object.
        /// </summary>
        TYPEID_OBJECT = 0,
        /// <summary>
        /// Item.
        /// </summary>
        TYPEID_ITEM = 1,
        /// <summary>
        /// Container (item).
        /// </summary>
        TYPEID_CONTAINER = 2,
        /// <summary>
        /// Unit.
        /// </summary>
        TYPEID_UNIT = 3,
        /// <summary>
        /// Player (unit).
        /// </summary>
        TYPEID_PLAYER = 4,
        /// <summary>
        /// Game object.
        /// </summary>
        TYPEID_GAMEOBJECT = 5,
        /// <summary>
        /// Dynamic object.
        /// </summary>
        TYPEID_DYNAMICOBJECT = 6,
        /// <summary>
        /// Player corpse (not used for units?).
        /// </summary>
        TYPEID_CORPSE = 7,
        /// <summary>
        /// It really exist?
        /// </summary>
        TYPEID_AIGROUP = 8,
        /// <summary>
        /// It really exist?
        /// </summary>
        TYPEID_AREATRIGGER = 9,
    }
}
