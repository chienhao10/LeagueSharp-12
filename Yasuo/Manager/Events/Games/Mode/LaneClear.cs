﻿namespace Flowers_Yasuo.Manager.Events.Games.Mode
{
    using System.Linq;
    using Common;
    using Spells;
    using LeagueSharp;
    using LeagueSharp.Common;
    using static Common.Common;

    internal class LaneClear : Logic
    {
        internal static void Init()
        {
            var minions = MinionManager.GetMinions(Me.Position, Q3.Range);

            if (minions.Any())
            {
                if (Menu.Item("LaneClearItems", true).GetValue<bool>())
                {
                    foreach (var min in minions.Where(x => x.DistanceToPlayer() <= 400))
                    {
                        UseItems(min);
                    }
                }

                if (Menu.Item("LaneClearE", true).GetValue<bool>() && E.IsReady())
                {
                    foreach (
                        var min in
                        minions.Where(
                            x =>
                                x.DistanceToPlayer() <= E.Range && SpellManager.CanCastE(x) &&
                                x.Health <=
                                (Q.IsReady()
                                    ? SpellManager.GetQDmg(x) + SpellManager.GetEDmg(x)
                                    : SpellManager.GetEDmg(x))))
                    {
                        if ((Menu.Item("LaneClearETurret", true).GetValue<bool>() ||
                            !UnderTower(PosAfterE(min))) &&
                            !HeroManager.Enemies.Any(x => x.Distance(PosAfterE(x).To3D()) <= 600))
                        {
                            E.CastOnUnit(min, true);
                        }
                    }
                }

                if (Menu.Item("LaneClearQ", true).GetValue<bool>() && Q.IsReady() && !SpellManager.HaveQ3)
                {
                    if (!IsDashing)
                    {
                        var qFarm =
                            MinionManager.GetBestLineFarmLocation(minions.Select(x => x.Position.To2D()).ToList(),
                                Q.Width, Q.Range);

                        if (qFarm.MinionsHit >= 1)
                        {
                            Q.Cast(qFarm.Position, true);
                        }
                    }
                    else
                    {
                        var qminions = MinionManager.GetMinions(lastEPos, 220);

                        if (qminions.Count >= 2)
                        {
                            Utility.DelayAction.Add(50 + Game.Ping/2, () => Q.Cast(Me.Position));
                        }
                    }
                }

                if (Menu.Item("LaneClearQ3", true).GetValue<bool>() && Q3.IsReady() && SpellManager.HaveQ3)
                {
                    if (!IsDashing)
                    {
                        var q3Farm =
                            MinionManager.GetBestLineFarmLocation(minions.Select(x => x.Position.To2D()).ToList(),
                                Q3.Width, Q3.Range);

                        if (q3Farm.MinionsHit >= Menu.Item("LaneClearQ3count", true).GetValue<Slider>().Value)
                        {
                            Q3.Cast(q3Farm.Position, true);
                        }
                    }
                    else
                    {
                        var q3minions = MinionManager.GetMinions(lastEPos, 220);

                        if (q3minions.Count >= 2)
                        {
                            Utility.DelayAction.Add(50 + Game.Ping/2, () => Q3.Cast(Me.Position));
                        }
                    }
                }
            }
        }
    }
}