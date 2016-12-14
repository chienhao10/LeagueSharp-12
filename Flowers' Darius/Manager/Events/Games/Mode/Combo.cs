﻿using System.Linq;

namespace Flowers_Darius.Manager.Events.Games.Mode
{
    using Spells;
    using myCommon;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Orbwalking = myCommon.Orbwalking;

    internal class Combo : Logic
    {
        internal static void Init()
        {
            var target = TargetSelector.GetTarget(600f, TargetSelector.DamageType.Physical);

            if (target != null)
            {
                if (Menu.GetBool("ComboIgnite") && Ignite != SpellSlot.Unknown && Ignite.IsReady() &&
                    target.DistanceToPlayer() <= 600f &&
                    (target.Health < DamageCalculate.GetComboDamage(target) ||
                     target.Health < DamageCalculate.GetIgniteDmage(target)))
                {
                    Me.Spellbook.CastSpell(Ignite, target);
                }

                if (HeroManager.Enemies.Any(x => x.DistanceToPlayer() <= 400))
                {
                    SpellManager.CastItem();
                }

                if (Menu.GetKey("ComboR") && R.IsReady())
                {
                    foreach (var rTarget in HeroManager.Enemies.Where(x => x.DistanceToPlayer() <= R.Range &&
                                                                           x.Health <= DamageCalculate.GetRDamage(x) &&
                                                                           !x.HasBuff("willrevive")))
                    {
                        if (rTarget.IsValidTarget(R.Range))
                        {
                            R.CastOnUnit(target, true);
                        }
                    }
                }

                if (Menu.GetBool("ComboQ") && Q.IsReady() && target.DistanceToPlayer() <= Q.Range &&
                    SpellManager.CanQHit(target) && Me.CanMoveMent())
                {
                    if (Menu.GetBool("ComboSaveMana") && Me.Mana < SpellManager.RMana + Q.Instance.ManaCost)
                    {
                        return;
                    }

                    if (Utils.TickCount - lastETime > 1000)
                    {
                        Q.Cast(true);
                    }
                }

                if (Menu.GetBool("ComboE") && E.IsReady() && target.DistanceToPlayer() <= E.Range - 30 &&
                    !Orbwalking.InAutoAttackRange(target) && !target.HaveShiled())
                {
                    if (Menu.GetBool("ComboSaveMana") && Me.Mana < SpellManager.RMana + Q.Instance.ManaCost)
                    {
                        return;
                    }

                    var pred = E.GetPrediction(target);
                    E.Cast(pred.UnitPosition, true);
                }
            }
        }
    }
}
