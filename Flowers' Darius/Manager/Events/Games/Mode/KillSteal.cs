﻿namespace Flowers_Darius.Manager.Events.Games.Mode
{
    using System.Linq;
    using myCommon;
    using LeagueSharp.Common;
    using Orbwalking = myCommon.Orbwalking;

    internal class KillSteal : Logic
    {
        internal static void Init()
        {
            if (Menu.GetBool("KillStealR") && R.IsReady())
            {
                if (Menu.GetBool("KillStealRNotCombo") && Orbwalking.isCombo)
                {
                    return;
                }

                foreach (
                    var target in
                    HeroManager.Enemies.Where(
                        x =>
                            x.DistanceToPlayer() <= R.Range && x.Health <= DamageCalculate.GetRDamage(x) &&
                            !x.HasBuff("willrevive")))
                {
                    if (target.DistanceToPlayer() <= R.Range && !target.IsDead && Me.CanMoveMent())
                    {
                        R.CastOnUnit(target, true);
                    }
                }
            }

            if (Menu.GetBool("KillStealQ") && Q.IsReady())
            {
                foreach (
                    var target in
                    HeroManager.Enemies.Where(
                        x => x.DistanceToPlayer() <= Q.Range && x.Health <= DamageCalculate.GetQDamage(x)))
                {
                    if (target.IsValidTarget(Q.Range) && !target.IsDead && Me.CanMoveMent())
                    {
                        Q.Cast(true);
                        return;
                    }
                }
            }
        }
    }
}
