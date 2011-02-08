using System;

namespace a9_parser
{
    /// <summary>
    /// WoW update fields class.
    /// </summary>
    public class UpdateFields
    {
        /// <summary>
        /// Update fields structure.
        /// </summary>
        public struct UpdateFields_s
        {
            /// <summary>
            /// Update field id.
            /// </summary>
            public uint updatefield;

            /// <summary>
            /// Update field type.
            /// </summary>
            public byte type;
            // 0 - uint32 (default)
            // 1 - float
            // 2 - uint64
            // 3 - array

            /// <summary>
            /// Update field name.
            /// </summary>
            public string name;

            /// <summary>
            /// Output format.
            /// </summary>
            public byte format;
            // 0 - decimal
            // 1 - hehadecimal

            /// <summary>
            /// Update field structure constructor.
            /// </summary>
            /// <param name="uf">Update field id.</param>
            /// <param name="tp">Update field type.</param>
            /// <param name="nm">Update field name.</param>
            /// <param name="fmt">Update field output format.</param>
            public UpdateFields_s(uint uf, byte tp, string nm, byte fmt)
            {
                updatefield = uf;
                type = tp;
                name = nm;
                format = fmt;
            }
        }

        // update fields count for WoW 2.0.10
        /// <summary>
        /// Item update fields end.
        /// </summary>
        public static uint ITEM_END = 134;
        /// <summary>
        /// Unit update fields end.
        /// </summary>
        public static uint UNIT_END = 226;
        /// <summary>
        /// Player updatefields end.
        /// </summary>
        public static uint PLAYER_END = 1422;
        /// <summary>
        /// Game object update fields end.
        /// </summary>
        public static uint GO_END = 26;
        /// <summary>
        /// Dynamic object fields end.
        /// </summary>
        public static uint DO_END = 16;
        /// <summary>
        /// Corpse fields end.
        /// </summary>
        public static uint CORPSE_END = 38;

        /// <summary>
        /// Item update fields array.
        /// </summary>
        public static UpdateFields_s[] item_updatefields = new UpdateFields_s[ITEM_END]; // items+containers
        /// <summary>
        /// Unit update fields array.
        /// </summary>
        public static UpdateFields_s[] unit_updatefields = new UpdateFields_s[PLAYER_END]; // units+players
        /// <summary>
        /// Game object update fields array.
        /// </summary>
        public static UpdateFields_s[] go_updatefields = new UpdateFields_s[GO_END]; // gameobjects
        /// <summary>
        /// Dynamic object update fields array.
        /// </summary>
        public static UpdateFields_s[] do_updatefields = new UpdateFields_s[DO_END]; // dynamic objects
        /// <summary>
        /// Corpse update fields array.
        /// </summary>
        public static UpdateFields_s[] corpse_updatefields = new UpdateFields_s[CORPSE_END]; // corpses

        /// <summary>
        /// Item update fields names array.
        /// </summary>
        public static string[] item_updatefields_names = new string[ITEM_END];
        /// <summary>
        /// Unit update fields names array.
        /// </summary>
        public static string[] unit_updatefields_names = new string[PLAYER_END];
        /// <summary>
        /// Game object update fields names array.
        /// </summary>
        public static string[] go_updatefields_names = new string[GO_END];
        /// <summary>
        /// Dynamic object update fields names array.
        /// </summary>
        public static string[] do_updatefields_names = new string[DO_END];
        /// <summary>
        /// Corpse update fields names array.
        /// </summary>
        public static string[] corpse_updatefields_names = new string[CORPSE_END];

        /// <summary>
        /// Fills update fields arrays with data.
        /// </summary>
        public static void FillArrays()
        {
            FillItemUpdateFieldsNames();
            for (uint i = 0; i < ITEM_END; i++)
            {
                if (i == 4)
                    item_updatefields[i] = new UpdateFields_s(i, 1, item_updatefields_names[i], 0);
                else
                    item_updatefields[i] = new UpdateFields_s(i, 0, item_updatefields_names[i], 0);
            }

            FillUnitUpdateFieldsNames();
            for (uint i = 0; i < PLAYER_END; i++)
            {
                if (i == 4 || i == 143 || i == 144 || (i >= 148 && i <= 151) || i == 159 || i == 205 ||
                    (i >= 208 && i <= 210) || i == 218 || (i >= 1248 && i <= 1260) ||
                    (i >= 1341 && i <= 1347) || //  may be wrong...
                    i == 1418 || i == 1419)
                {
                    unit_updatefields[i] = new UpdateFields_s(i, 1, unit_updatefields_names[i], 0); // float
                }
                else
                    unit_updatefields[i] = new UpdateFields_s(i, 0, unit_updatefields_names[i], 0); // uint
            }

            FillGoUpdateFieldsNames();
            for (uint i = 0; i < GO_END; i++)
            {
                if (i == 4 || (i >= 10 && i <= 13) || (i >= 15 && i <= 18))
                    go_updatefields[i] = new UpdateFields_s(i, 1, go_updatefields_names[i], 0); // float
                else
                    go_updatefields[i] = new UpdateFields_s(i, 0, go_updatefields_names[i], 0); // uint
            }

            FillDoUpdateFieldsNames();
            for (uint i = 0; i < DO_END; i++)
            {
                if (i == 4 || (i >= 11 && i <= 14))
                    do_updatefields[i] = new UpdateFields_s(i, 1, do_updatefields_names[i], 0); // float
                else
                    do_updatefields[i] = new UpdateFields_s(i, 0, do_updatefields_names[i], 0); // uint
            }

            FillCorpseUpdateFieldsNames();
            for (uint i = 0; i < CORPSE_END; i++)
            {
                if (i == 4 || (i >= 8 && i <= 11))
                    corpse_updatefields[i] = new UpdateFields_s(i, 1, corpse_updatefields_names[i], 0); // float
                else
                    corpse_updatefields[i] = new UpdateFields_s(i, 0, corpse_updatefields_names[i], 0); // uint
            }
        }

        /// <summary>
        /// Fills item update fields array with data.
        /// </summary>
        public static void FillItemUpdateFieldsNames()
        {
            item_updatefields_names[0] = "ITEM_FIELD_GUID_LOW";
            item_updatefields_names[1] = "ITEM_FIELD_GUID_HIGH";
            item_updatefields_names[2] = "ITEM_FIELD_TYPE";
            item_updatefields_names[3] = "ITEM_FIELD_ENTRY";
            item_updatefields_names[4] = "ITEM_FIELD_SCALE_X";
            item_updatefields_names[5] = "ITEM_FIELD_PADDING";
            item_updatefields_names[6] = "ITEM_FIELD_OWNER_LOW";
            item_updatefields_names[7] = "ITEM_FIELD_OWNER_HIGH";
            item_updatefields_names[8] = "ITEM_FIELD_CONTAINED_LOW";
            item_updatefields_names[9] = "ITEM_FIELD_CONTAINED_HIGH";
            item_updatefields_names[10] = "ITEM_FIELD_CREATOR_LOW";
            item_updatefields_names[11] = "ITEM_FIELD_CREATOR_HIGH";
            item_updatefields_names[12] = "ITEM_FIELD_GIFTCREATOR_LOW";
            item_updatefields_names[13] = "ITEM_FIELD_GIFTCREATOR_HIGH";
            item_updatefields_names[14] = "ITEM_FIELD_STACK_COUNT";
            item_updatefields_names[15] = "ITEM_FIELD_DURATION";

            for (uint i = 1; i < 6; i++) // 5
                item_updatefields_names[15 + i] = "ITEM_FIELD_SPELL_CHARGES" + i.ToString();

            item_updatefields_names[21] = "ITEM_FIELD_FLAGS";

            for (uint i = 1; i < 34; i++) // 33
                item_updatefields_names[21 + i] = "ITEM_FIELD_ENCHANTMENT" + i.ToString();

            item_updatefields_names[55] = "ITEM_FIELD_PROPERTY_SEED";
            item_updatefields_names[56] = "ITEM_FIELD_RANDOM_PROPERTIES_ID";
            item_updatefields_names[57] = "ITEM_FIELD_ITEM_TEXT_ID";
            item_updatefields_names[58] = "ITEM_FIELD_DURABILITY";
            item_updatefields_names[59] = "ITEM_FIELD_MAXDURABILITY";

            item_updatefields_names[60] = "CONTAINER_FIELD_NUM_SLOTS";
            item_updatefields_names[61] = "CONTAINER_ALIGN_PAD";

            for (uint i = 1; i < 73; i++) // 72
                item_updatefields_names[61 + i] = "CONTAINER_FIELD_SLOT_" + i.ToString();
        }

        /// <summary>
        /// Fills unit and player update fields array with data.
        /// </summary>
        public static void FillUnitUpdateFieldsNames()
        {
            unit_updatefields_names[0] = "UNIT_FIELD_GUID_LOW";
            unit_updatefields_names[1] = "UNIT_FIELD_GUID_HIGH";
            unit_updatefields_names[2] = "UNIT_FIELD_TYPE";
            unit_updatefields_names[3] = "UNIT_FIELD_ENTRY";
            unit_updatefields_names[4] = "UNIT_FIELD_SCALE_X";
            unit_updatefields_names[5] = "UNIT_FIELD_PADDING";
            unit_updatefields_names[6] = "UNIT_FIELD_CHARM_LOW";
            unit_updatefields_names[7] = "UNIT_FIELD_CHARM_HIGH";
            unit_updatefields_names[8] = "UNIT_FIELD_SUMMON_LOW";
            unit_updatefields_names[9] = "UNIT_FIELD_SUMMON_HIGH";
            unit_updatefields_names[10] = "UNIT_FIELD_CHARMEDBY_LOW";
            unit_updatefields_names[11] = "UNIT_FIELD_CHARMEDBY_HIGH";
            unit_updatefields_names[12] = "UNIT_FIELD_SUMMONEDBY_LOW";
            unit_updatefields_names[13] = "UNIT_FIELD_SUMMONEDBY_HIGH";
            unit_updatefields_names[14] = "UNIT_FIELD_CREATEDBY_LOW";
            unit_updatefields_names[15] = "UNIT_FIELD_CREATEDBY_HIGH";
            unit_updatefields_names[16] = "UNIT_FIELD_TARGET_LOW";
            unit_updatefields_names[17] = "UNIT_FIELD_TARGET_HIGH";
            unit_updatefields_names[18] = "UNIT_FIELD_PERSUADED_LOW";
            unit_updatefields_names[19] = "UNIT_FIELD_PERSUADED_HIGH";
            unit_updatefields_names[20] = "UNIT_FIELD_CHANNEL_OBJECT_LOW";
            unit_updatefields_names[21] = "UNIT_FIELD_CHANNEL_OBJECT_HIGH";
            unit_updatefields_names[22] = "UNIT_FIELD_HEALTH";
            unit_updatefields_names[23] = "UNIT_FIELD_POWER1";
            unit_updatefields_names[24] = "UNIT_FIELD_POWER2";
            unit_updatefields_names[25] = "UNIT_FIELD_POWER3";
            unit_updatefields_names[26] = "UNIT_FIELD_POWER4";
            unit_updatefields_names[27] = "UNIT_FIELD_POWER5";
            unit_updatefields_names[28] = "UNIT_FIELD_MAXHEALTH";
            unit_updatefields_names[29] = "UNIT_FIELD_MAXPOWER1";
            unit_updatefields_names[30] = "UNIT_FIELD_MAXPOWER2";
            unit_updatefields_names[31] = "UNIT_FIELD_MAXPOWER3";
            unit_updatefields_names[32] = "UNIT_FIELD_MAXPOWER4";
            unit_updatefields_names[33] = "UNIT_FIELD_MAXPOWER5";
            unit_updatefields_names[34] = "UNIT_FIELD_LEVEL";
            unit_updatefields_names[35] = "UNIT_FIELD_FACTIONTEMPLATE";
            unit_updatefields_names[36] = "UNIT_FIELD_BYTES_0";

            for (uint i = 1; i < 4; i++) // 3
                unit_updatefields_names[36 + i] = "UNIT_VIRTUAL_ITEM_SLOT_DISPLAY" + i.ToString();

            for (uint i = 1; i < 7; i++) // 6
                unit_updatefields_names[39 + i] = "UNIT_VIRTUAL_ITEM_INFO" + i.ToString();

            unit_updatefields_names[46] = "UNIT_FIELD_FLAGS";
            unit_updatefields_names[47] = "UNIT_FIELD_FLAGS_2";

            for (uint i = 1; i < 57; i++) // 56
                unit_updatefields_names[47 + i] = "UNIT_FIELD_AURA" + i.ToString();

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[103 + i] = "UNIT_FIELD_AURAFLAGS" + i.ToString();

            for (uint i = 1; i < 15; i++) // 14
                unit_updatefields_names[110 + i] = "UNIT_FIELD_AURALEVELS" + i.ToString();

            for (uint i = 1; i < 15; i++) // 14
                unit_updatefields_names[124 + i] = "UNIT_FIELD_AURAAPPLICATIONS" + i.ToString();

            unit_updatefields_names[139] = "UNIT_FIELD_AURASTATE";
            unit_updatefields_names[140] = "UNIT_FIELD_BASEATTACKTIME";
            unit_updatefields_names[141] = "UNIT_FIELD_OFFHANDATTACKTIME"; // unofficial
            unit_updatefields_names[142] = "UNIT_FIELD_RANGEDATTACKTIME";
            unit_updatefields_names[143] = "UNIT_FIELD_BOUNDINGRADIUS";
            unit_updatefields_names[144] = "UNIT_FIELD_COMBATREACH";
            unit_updatefields_names[145] = "UNIT_FIELD_DISPLAYID";
            unit_updatefields_names[146] = "UNIT_FIELD_NATIVEDISPLAYID";
            unit_updatefields_names[147] = "UNIT_FIELD_MOUNTDISPLAYID";
            unit_updatefields_names[148] = "UNIT_FIELD_MINDAMAGE";
            unit_updatefields_names[149] = "UNIT_FIELD_MAXDAMAGE";
            unit_updatefields_names[150] = "UNIT_FIELD_MINOFFHANDDAMAGE";
            unit_updatefields_names[151] = "UNIT_FIELD_MAXOFFHANDDAMAGE";
            unit_updatefields_names[152] = "UNIT_FIELD_BYTES_1";
            unit_updatefields_names[153] = "UNIT_FIELD_PETNUMBER";
            unit_updatefields_names[154] = "UNIT_FIELD_PET_NAME_TIMESTAMP";
            unit_updatefields_names[155] = "UNIT_FIELD_PETEXPERIENCE";
            unit_updatefields_names[156] = "UNIT_FIELD_PETNEXTLEVELEXP";
            unit_updatefields_names[157] = "UNIT_DYNAMIC_FLAGS";
            unit_updatefields_names[158] = "UNIT_CHANNEL_SPELL";
            unit_updatefields_names[159] = "UNIT_MOD_CAST_SPEED";
            unit_updatefields_names[160] = "UNIT_CREATED_BY_SPELL";
            unit_updatefields_names[161] = "UNIT_NPC_FLAGS";
            unit_updatefields_names[162] = "UNIT_NPC_EMOTESTATE";
            unit_updatefields_names[163] = "UNIT_TRAINING_POINTS";
            unit_updatefields_names[164] = "UNIT_FIELD_STAT0";
            unit_updatefields_names[165] = "UNIT_FIELD_STAT1";
            unit_updatefields_names[166] = "UNIT_FIELD_STAT2";
            unit_updatefields_names[167] = "UNIT_FIELD_STAT3";
            unit_updatefields_names[168] = "UNIT_FIELD_STAT4";
            unit_updatefields_names[169] = "UNIT_FIELD_POSSTAT0";
            unit_updatefields_names[170] = "UNIT_FIELD_POSSTAT1";
            unit_updatefields_names[171] = "UNIT_FIELD_POSSTAT2";
            unit_updatefields_names[172] = "UNIT_FIELD_POSSTAT3";
            unit_updatefields_names[173] = "UNIT_FIELD_POSSTAT4";
            unit_updatefields_names[174] = "UNIT_FIELD_NEGSTAT0";
            unit_updatefields_names[175] = "UNIT_FIELD_NEGSTAT1";
            unit_updatefields_names[176] = "UNIT_FIELD_NEGSTAT2";
            unit_updatefields_names[177] = "UNIT_FIELD_NEGSTAT3";
            unit_updatefields_names[178] = "UNIT_FIELD_NEGSTAT4";

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[178 + i] = "UNIT_FIELD_RESISTANCES" + i.ToString();

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[185 + i] = "UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE" + i.ToString();

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[192 + i] = "UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE" + i.ToString();

            unit_updatefields_names[200] = "UNIT_FIELD_BASE_MANA";
            unit_updatefields_names[201] = "UNIT_FIELD_BASE_HEALTH";
            unit_updatefields_names[202] = "UNIT_FIELD_BYTES_2";
            unit_updatefields_names[203] = "UNIT_FIELD_ATTACK_POWER";
            unit_updatefields_names[204] = "UNIT_FIELD_ATTACK_POWER_MODS";
            unit_updatefields_names[205] = "UNIT_FIELD_ATTACK_POWER_MULTIPLIER";
            unit_updatefields_names[206] = "UNIT_FIELD_RANGED_ATTACK_POWER";
            unit_updatefields_names[207] = "UNIT_FIELD_RANGED_ATTACK_POWER_MODS";
            unit_updatefields_names[208] = "UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER";
            unit_updatefields_names[209] = "UNIT_FIELD_MINRANGEDDAMAGE";
            unit_updatefields_names[210] = "UNIT_FIELD_MAXRANGEDDAMAGE";

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[210 + i] = "UNIT_FIELD_POWER_COST_MODIFIER" + i.ToString();

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[217 + i] = "UNIT_FIELD_POWER_COST_MULTIPLIER" + i.ToString();

            unit_updatefields_names[225] = "UNIT_FIELD_MAXRANGEDDAMAGE";
            unit_updatefields_names[226] = "PLAYER_DUEL_ARBITER_LOW";
            unit_updatefields_names[227] = "PLAYER_DUEL_ARBITER_HIGH";
            unit_updatefields_names[228] = "PLAYER_FLAGS";
            unit_updatefields_names[229] = "PLAYER_GUILDID";
            unit_updatefields_names[230] = "PLAYER_GUILDRANK";
            unit_updatefields_names[231] = "PLAYER_BYTES";
            unit_updatefields_names[232] = "PLAYER_BYTES_2";
            unit_updatefields_names[233] = "PLAYER_BYTES_3";
            unit_updatefields_names[234] = "PLAYER_DUEL_TEAM";
            unit_updatefields_names[235] = "PLAYER_GUILD_TIMESTAMP";

            uint fn = 235;
            for (uint i = 1; i < 26; i++) // 25
            {
                for (uint j = 0; j < 3; j++)
                {
                    unit_updatefields_names[fn + i + j] = "PLAYER_QUEST_LOG_" + i.ToString() + "_" + j.ToString();
                }
                fn += 2;
            }

            fn = 310;
            for (uint i = 1; i < 20; i++) // 19
            {
                unit_updatefields_names[fn + i] = "PLAYER_VISIBLE_ITEM_" + i.ToString() + "_CREATOR_LOW";
                fn++;
                unit_updatefields_names[fn + i] = "PLAYER_VISIBLE_ITEM_" + i.ToString() + "_CREATOR_HIGH";
                fn++;
                for (uint j = 0; j < 12; j++)
                {
                    unit_updatefields_names[fn + i] = "PLAYER_VISIBLE_ITEM_" + i.ToString() + "_0" + "_" + j.ToString();
                    fn++;
                }
                unit_updatefields_names[fn + i] = "PLAYER_VISIBLE_ITEM_" + i.ToString() + "_PROPERTIES";
                fn++;
                unit_updatefields_names[fn + i] = "PLAYER_VISIBLE_ITEM_" + i.ToString() + "_PAD";
                //fn++;
            }

            unit_updatefields_names[615] = "PLAYER_CHOSEN_TITLE";

            for (uint i = 1; i < 47; i++) // 23 x uint64
                unit_updatefields_names[615 + i] = "PLAYER_FIELD_INV_SLOT_HEAD" + i.ToString();

            for (uint i = 1; i < 33; i++) // 32
                unit_updatefields_names[661 + i] = "PLAYER_FIELD_PACK_SLOT_" + i.ToString();

            for (uint i = 1; i < 57; i++) // 56
                unit_updatefields_names[693 + i] = "PLAYER_FIELD_BANK_SLOT_" + i.ToString();

            for (uint i = 1; i < 15; i++) // 14
                unit_updatefields_names[749 + i] = "PLAYER_FIELD_BANKBAG_SLOT_" + i.ToString();

            for (uint i = 1; i < 25; i++) // 12 x uint64
                unit_updatefields_names[763 + i] = "PLAYER_FIELD_VENDORBUYBACK_SLOT_" + i.ToString();

            for (uint i = 1; i < 65; i++) // 32 x uint64
                unit_updatefields_names[787 + i] = "PLAYER_FIELD_KEYRING_SLOT_" + i.ToString();

            unit_updatefields_names[852] = "PLAYER_FARSIGHT_LOW";
            unit_updatefields_names[853] = "PLAYER_FARSIGHT_HIGH";
            unit_updatefields_names[854] = "PLAYER_FIELD_COMBO_TARGET_LOW";
            unit_updatefields_names[855] = "PLAYER_FIELD_COMBO_TARGET_HIGH";
            unit_updatefields_names[856] = "PLAYER_FIELD_KNOWN_TITLES_LOW";
            unit_updatefields_names[857] = "PLAYER_FIELD_KNOWN_TITLES_HIGH";
            unit_updatefields_names[858] = "PLAYER_XP";
            unit_updatefields_names[859] = "PLAYER_NEXT_LEVEL_XP";

            for (uint i = 1; i < 385; i++) // 384
                unit_updatefields_names[859 + i] = "PLAYER_SKILL_INFO_1_" + i.ToString();

            unit_updatefields_names[1244] = "PLAYER_CHARACTER_POINTS1";
            unit_updatefields_names[1245] = "PLAYER_CHARACTER_POINTS2";
            unit_updatefields_names[1246] = "PLAYER_TRACK_CREATURES";
            unit_updatefields_names[1247] = "PLAYER_TRACK_RESOURCES";
            unit_updatefields_names[1248] = "PLAYER_BLOCK_PERCENTAGE";
            unit_updatefields_names[1249] = "PLAYER_DODGE_PERCENTAGE";
            unit_updatefields_names[1250] = "PLAYER_PARRY_PERCENTAGE";
            unit_updatefields_names[1251] = "PLAYER_CRIT_PERCENTAGE";
            unit_updatefields_names[1252] = "PLAYER_RANGED_CRIT_PERCENTAGE";
            unit_updatefields_names[1253] = "PLAYER_OFFHAND_CRIT_PERCENTAGE";

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[1253 + i] = "PLAYER_SPELL_CRIT_PERCENTAGE" + i.ToString();

            for (uint i = 1; i < 65; i++) // 64
                unit_updatefields_names[1260 + i] = "PLAYER_EXPLORED_ZONES_" + i.ToString();

            unit_updatefields_names[1325] = "PLAYER_REST_STATE_EXPERIENCE";
            unit_updatefields_names[1326] = "PLAYER_FIELD_COINAGE";

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[1326 + i] = "PLAYER_FIELD_MOD_DAMAGE_DONE_POS" + i.ToString();

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[1333 + i] = "PLAYER_FIELD_MOD_DAMAGE_DONE_NEG" + i.ToString();

            for (uint i = 1; i < 8; i++) // 7
                unit_updatefields_names[1340 + i] = "PLAYER_FIELD_MOD_DAMAGE_DONE_PCT" + i.ToString();

            unit_updatefields_names[1348] = "PLAYER_FIELD_MOD_HEALING_DONE_POS";
            unit_updatefields_names[1349] = "PLAYER_FIELD_MOD_TARGET_RESISTANCE";
            unit_updatefields_names[1350] = "PLAYER_FIELD_BYTES";
            unit_updatefields_names[1351] = "PLAYER_AMMO_ID";
            unit_updatefields_names[1352] = "PLAYER_SELF_RES_SPELL";
            unit_updatefields_names[1353] = "PLAYER_FIELD_PVP_MEDALS";

            for (uint i = 1; i < 13; i++) // 12
                unit_updatefields_names[1353 + i] = "PLAYER_FIELD_BUYBACK_PRICE_" + i.ToString();

            for (uint i = 1; i < 13; i++) // 12
                unit_updatefields_names[1365 + i] = "PLAYER_FIELD_BUYBACK_TIMESTAMP_" + i.ToString();

            unit_updatefields_names[1378] = "PLAYER_FIELD_KILLS";
            unit_updatefields_names[1379] = "PLAYER_FIELD_TODAY_CONTRIBUTION";
            unit_updatefields_names[1380] = "PLAYER_FIELD_YESTERDAY_CONTRIBUTION";
            unit_updatefields_names[1381] = "PLAYER_FIELD_LIFETIME_HONORBALE_KILLS";
            unit_updatefields_names[1382] = "PLAYER_FIELD_BYTES2";
            unit_updatefields_names[1383] = "PLAYER_FIELD_WATCHED_FACTION_INDEX";

            for (uint i = 1; i < 24; i++) // 23
                unit_updatefields_names[1383 + i] = "PLAYER_FIELD_COMBAT_RATING_" + i.ToString();

            for (uint i = 1; i < 10; i++) // 9
                unit_updatefields_names[1406 + i] = "PLAYER_FIELD_ARENA_TEAM_INFO_1_" + i.ToString();

            unit_updatefields_names[1416] = "PLAYER_FIELD_HONOR_CURRENCY";
            unit_updatefields_names[1417] = "PLAYER_FIELD_ARENA_CURRENCY";
            unit_updatefields_names[1418] = "PLAYER_FIELD_MOD_MANA_REGEN";
            unit_updatefields_names[1419] = "PLAYER_FIELD_MOD_MANA_REGEN_INTERRUPT";
            unit_updatefields_names[1420] = "PLAYER_FIELD_MAX_LEVEL";
            unit_updatefields_names[1421] = "PLAYER_FIELD_PADDING";
        }

        /// <summary>
        /// Fills game object update fields array with data.
        /// </summary>
        public static void FillGoUpdateFieldsNames()
        {
            go_updatefields_names[0] = "GAMEOBJECT_GUID_LOW";
            go_updatefields_names[1] = "GAMEOBJECT_GUID_HIGH";
            go_updatefields_names[2] = "GAMEOBJECT_TYPE";
            go_updatefields_names[3] = "GAMEOBJECT_ENTRY";
            go_updatefields_names[4] = "GAMEOBJECT_SCALE_X";
            go_updatefields_names[5] = "GAMEOBJECT_PADDING";
            go_updatefields_names[6] = "GAMEOBJECT_CREATED_BY_LOW";
            go_updatefields_names[7] = "GAMEOBJECT_CREATED_BY_HIGH";
            go_updatefields_names[8] = "GAMEOBJECT_DISPLAYID";
            go_updatefields_names[9] = "GAMEOBJECT_FLAGS";

            for (uint i = 1; i < 5; i++) // 4
                go_updatefields_names[9 + i] = "GAMEOBJECT_ROTATION" + i.ToString();

            go_updatefields_names[14] = "GAMEOBJECT_STATE";
            go_updatefields_names[15] = "GAMEOBJECT_POS_X";
            go_updatefields_names[16] = "GAMEOBJECT_POS_Y";
            go_updatefields_names[17] = "GAMEOBJECT_POS_Z";
            go_updatefields_names[18] = "GAMEOBJECT_FACING";
            go_updatefields_names[19] = "GAMEOBJECT_DYN_FLAGS";
            go_updatefields_names[20] = "GAMEOBJECT_FACTION";
            go_updatefields_names[21] = "GAMEOBJECT_TYPE_ID";
            go_updatefields_names[22] = "GAMEOBJECT_LEVEL";
            go_updatefields_names[23] = "GAMEOBJECT_ARTKIT";
            go_updatefields_names[24] = "GAMEOBJECT_ANIMPROGRESS";
            go_updatefields_names[25] = "GAMEOBJECT_PADDING";
        }

        /// <summary>
        /// Fills dynamic object update fields array with data.
        /// </summary>
        public static void FillDoUpdateFieldsNames()
        {
            do_updatefields_names[0] = "DYNAMICOBJECT_GUID_LOW";
            do_updatefields_names[1] = "DYNAMICOBJECT_GUID_HIGH";
            do_updatefields_names[2] = "DYNAMICOBJECT_TYPE";
            do_updatefields_names[3] = "DYNAMICOBJECT_ENTRY";
            do_updatefields_names[4] = "DYNAMICOBJECT_SCALE_X";
            do_updatefields_names[5] = "DYNAMICOBJECT_PADDING";
            do_updatefields_names[6] = "DYNAMICOBJECT_CASTER_LOW";
            do_updatefields_names[7] = "DYNAMICOBJECT_CASTER_HIGH";
            do_updatefields_names[8] = "DYNAMICOBJECT_BYTES";
            do_updatefields_names[9] = "DYNAMICOBJECT_SPELLID";
            do_updatefields_names[10] = "DYNAMICOBJECT_RADIUS";
            do_updatefields_names[11] = "DYNAMICOBJECT_POS_X";
            do_updatefields_names[12] = "DYNAMICOBJECT_POS_Y";
            do_updatefields_names[13] = "DYNAMICOBJECT_POS_Z";
            do_updatefields_names[14] = "DYNAMICOBJECT_FACING";
            do_updatefields_names[15] = "DYNAMICOBJECT_PAD";
        }

        /// <summary>
        /// Fills corpse update fields array with data.
        /// </summary>
        public static void FillCorpseUpdateFieldsNames()
        {
            corpse_updatefields_names[0] = "CORPSE_FIELD_GUID_LOW";
            corpse_updatefields_names[1] = "CORPSE_FIELD_GUID_HIGH";
            corpse_updatefields_names[2] = "CORPSE_FIELD_TYPE";
            corpse_updatefields_names[3] = "CORPSE_FIELD_ENTRY";
            corpse_updatefields_names[4] = "CORPSE_FIELD_SCALE_X";
            corpse_updatefields_names[5] = "CORPSE_FIELD_PADDING";
            corpse_updatefields_names[6] = "CORPSE_FIELD_OWNER_LOW";
            corpse_updatefields_names[7] = "CORPSE_FIELD_OWNER_HIGH";
            corpse_updatefields_names[8] = "CORPSE_FIELD_FACING";
            corpse_updatefields_names[9] = "CORPSE_FIELD_POS_X";
            corpse_updatefields_names[10] = "CORPSE_FIELD_POS_Y";
            corpse_updatefields_names[11] = "CORPSE_FIELD_POS_Z";
            corpse_updatefields_names[12] = "CORPSE_FIELD_DISPLAY_ID";

            for (uint i = 1; i < 20; i++) // 19
                corpse_updatefields_names[12 + i] = "CORPSE_FIELD_ITEM" + i.ToString();

            corpse_updatefields_names[32] = "CORPSE_FIELD_BYTES_1";
            corpse_updatefields_names[33] = "CORPSE_FIELD_BYTES_2";
            corpse_updatefields_names[34] = "CORPSE_FIELD_GUILD";
            corpse_updatefields_names[35] = "CORPSE_FIELD_FLAGS";
            corpse_updatefields_names[36] = "CORPSE_FIELD_DYNAMIC_FLAGS";
            corpse_updatefields_names[37] = "CORPSE_FIELD_PAD";
        }
    }
}
