﻿namespace Flowers_Riven_Reborn
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using Orbwalking = myCommon.Orbwalking;

    internal class Logic
    {
        internal static Spell Q, W, E, R;
        internal static SpellSlot Ignite = SpellSlot.Unknown, Flash = SpellSlot.Unknown;
        internal static Menu Menu;
        internal static Obj_AI_Hero Me = ObjectManager.Player;
        internal static int qStack;
        internal static int lastQTime;
        internal static Orbwalking.Orbwalker Orbwalker;
    }
}