﻿namespace Flowers_Riven
{
    using myCommon;
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using Color = System.Drawing.Color;
    using Orbwalking = myCommon.Orbwalking;

    internal class Program
    {
        private static Spell Q, W, E, R;
        private static SpellSlot Ignite = SpellSlot.Unknown, Flash = SpellSlot.Unknown;
        private static Menu Menu;
        private static Obj_AI_Hero Me;
        private static bool CanQ;
        private static int QStack;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Base QTarget;

        private static void Main(string[] Args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs Args)
        {
            if (ObjectManager.Player.ChampionName != "Riven")
            {
                return;
            }

            Me = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 325f);
            W = new Spell(SpellSlot.W, 260f);
            E = new Spell(SpellSlot.E, 312f);
            R = new Spell(SpellSlot.R, 900f);
            R.SetSkillshot(0.25f, 45f, 1600f, false, SkillshotType.SkillshotCone);

            Ignite = Me.GetSpellSlot("SummonerDot");
            Flash = Me.GetSpellSlot("SummonerFlash");

            Menu = new Menu("Flowers' Riven", "Flowers' Riven", true);

            var targetMenu = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            {
                TargetSelector.AddToMenu(targetMenu);
            }

            var orbMenu = Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            {
                Orbwalker = new Orbwalking.Orbwalker(orbMenu);
            }

            var comboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            {
                comboMenu.AddItem(new MenuItem("ComboW", "Use W", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboE", "Use E", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboR", "Use R", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("R1Combo", "Use R1", true).SetValue(new KeyBind('L', KeyBindType.Toggle, true))).Permashow();
                comboMenu.AddItem(
                    new MenuItem("R2Mode", "Use R2 Mode: ", true).SetValue(
                        new StringList(new[] { "Killable", "Max Damage", "First Cast", "Off" }, 1)));
                comboMenu.AddItem(new MenuItem("ComboIgnite", "Use Ignite", true).SetValue(true));
            }

            var burstMenu = Menu.AddSubMenu(new Menu("Burst", "Burst"));
            {
                burstMenu.AddItem(new MenuItem("BurstFlash", "Use Flash", true).SetValue(true));
                burstMenu.AddItem(new MenuItem("BurstIgnite", "Use Ignite", true).SetValue(true));
                burstMenu.AddItem(new MenuItem("Note...", "Note: ", true));
                burstMenu.AddItem(new MenuItem("target...", "Left Cilck the Target", true));
                burstMenu.AddItem(new MenuItem("range...", "And Target in Burst Range", true));
                burstMenu.AddItem(new MenuItem("press...", "And then Press the Burst Key", true));
            }

            var harassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            {
                harassMenu.AddItem(new MenuItem("HarassQ", "Use Q", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassW", "Use W", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassE", "Use E", true).SetValue(true));
                //harassMenu.AddItem(
                //    new MenuItem("HarassMode", "Harass Mode", true).SetValue(new StringList(new[] {"Smart", "Burst"})));
            }

            var clearMenu = Menu.AddSubMenu(new Menu("Clear", "Clear"));
            {
                var laneClearMenu = clearMenu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
                {
                    laneClearMenu.AddItem(new MenuItem("LaneClearQ", "Use Q", true).SetValue(true));
                    laneClearMenu.AddItem(new MenuItem("LaneClearW", "Use W", true).SetValue(true));
                }

                var jungleClearMenu = clearMenu.AddSubMenu(new Menu("JungleClear", "JungleClear"));
                {
                    jungleClearMenu.AddItem(new MenuItem("JungleClearQ", "Use Q", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearW", "Use W", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearWLogic", "Use W| Smart", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearE", "Use E", true).SetValue(true));
                }
            }

            var killStealMenu = Menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            {
                killStealMenu.AddItem(new MenuItem("KillStealW", "Use W", true).SetValue(true));
                killStealMenu.AddItem(
                    new MenuItem("KillStealE", "Use E", true).SetValue(true).SetTooltip("E Gapcloser and R2 Kill"));
                killStealMenu.AddItem(new MenuItem("KillStealR", "Use R", true).SetValue(true));
            }

            var miscMenu = Menu.AddSubMenu(new Menu("Misc", "Misc"));
            {
                var qMenu = miscMenu.AddSubMenu(new Menu("Q Setting", "Q Setting"));
                {
                    var qDelayMenu = qMenu.AddSubMenu(new Menu("Delay Settings", "Delay Settings"));
                    {
                        qDelayMenu.AddItem(new MenuItem("Q1Delay", "Q1 Delay: ", true).SetValue(new Slider(242, 200, 300)));
                        qDelayMenu.AddItem(new MenuItem("Q2Delay", "Q2 Delay: ", true).SetValue(new Slider(242, 200, 300)));
                        qDelayMenu.AddItem(new MenuItem("Q3Delay", "Q3 Delay: ", true).SetValue(new Slider(342, 300, 400)));
                        qDelayMenu.AddItem(new MenuItem("AutoSetDelay", "Auto Set Q Delay?", true).SetValue(false)).ValueChanged +=
                            DelayChanged;
                    }

                    qMenu.AddItem(new MenuItem("KeepQALive", "Keep Q alive", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Dance", "Dance Emote in QA", true).SetValue(false));
                }

                var wMenu = miscMenu.AddSubMenu(new Menu("W Setting", "W Setting"));
                {
                    wMenu.AddItem(new MenuItem("AntiGapCloserW", "AntiGapCloser", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("InterruptTargetW", "Interrupt Danger Spell", true).SetValue(true));
                }

                var skinMenu = miscMenu.AddSubMenu(new Menu("SkinChance", "SkinChance"));
                {
                    SkinManager.AddToMenu(skinMenu, 7);
                }
            }

            var drawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            {
                drawMenu.AddItem(new MenuItem("DrawW", "Draw W Range", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("DrawBurst", "Draw Burst Range", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawRStatus", "Draw R Status", true).SetValue(true));
                DamageIndicator.AddToMenu(drawMenu, GetComboDamage);
            }

            Menu.AddItem(new MenuItem("asdvre1w56", "  "));
            Menu.AddItem(new MenuItem("Credit", "Credit : NightMoon"));
            Menu.AddItem(new MenuItem("Version", "Version : 1.0.0.5"));

            Menu.AddToMainMenu();

            if (!Menu.Item("AutoSetDelay", true).GetValue<bool>())
            {
                Menu.Item("Q1Delay", true).SetValue(new Slider(242, 200, 300));
                Menu.Item("Q2Delay", true).SetValue(new Slider(242, 200, 300));
                Menu.Item("Q3Delay", true).SetValue(new Slider(342, 300, 400));
            }

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnPlayAnimation += OnPlayAnimation;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void DelayChanged(object obj, OnValueChangeEventArgs Args)
        {
            if (!Args.GetNewValue<bool>())
            {
                Menu.Item("Q1Delay", true).SetValue(new Slider(242, 200, 300));
                Menu.Item("Q2Delay", true).SetValue(new Slider(242, 200, 300));
                Menu.Item("Q3Delay", true).SetValue(new Slider(342, 300, 400));
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser Args)
        {
            if (Menu.GetBool("AntiGapCloserW") && W.IsReady() && Args.Sender.IsValidTarget(W.Range) && 
                Me.CountEnemiesInRange(1000) < 3)
            {
                W.Cast(true);
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs Args)
        {
            if (Menu.GetBool("InterruptTargetW")&& W.IsReady() && sender.IsValidTarget(W.Range) && 
                !sender.ServerPosition.UnderTurret(true))
            {
                W.Cast(true);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            switch (Args.SData.Name)
            {
                case "ItemTiamatCleave":
                    if (!HeroManager.Enemies.Any(x => x.DistanceToPlayer() <= W.Range))
                    {
                        return;
                    }

                    switch (Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            if (Menu.GetBool("ComboW") && W.IsReady())
                            {
                                W.Cast();
                            }
                            break;
                        case Orbwalking.OrbwalkingMode.Burst:
                            W.Cast();
                            break;
                        case Orbwalking.OrbwalkingMode.Mixed:
                            if (Menu.GetBool("HarassW") && W.IsReady())
                            {
                                W.Cast();
                            }
                            break;
                    }
                    break;
                case "RivenTriCleave":
                    CanQ = false;

                    if (!HeroManager.Enemies.Any(x => x.DistanceToPlayer() <= 400))
                    {
                        return;
                    }

                    switch (Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            CastItem(true);

                            if (Menu.GetBool("ComboR") && R.IsReady())
                            {
                                var target = TargetSelector.GetSelectedTarget() ??
                                             TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                                if (target.IsValidTarget(R.Range))
                                {
                                    R2Logic(target);
                                }
                            }
                            break;
                        case Orbwalking.OrbwalkingMode.Burst:
                            CastItem(true);

                            if (R.IsReady())
                            {
                                if (TargetSelector.GetSelectedTarget().IsValidTarget(R.Range))
                                {
                                    var rPred = R.GetPrediction(TargetSelector.GetSelectedTarget(), true);

                                    if (rPred.Hitchance >= HitChance.High)
                                    {
                                        R.Cast(rPred.CastPosition, true);
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case "RivenTriCleaveBuffer":
                    switch (Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            if (Menu.GetBool("ComboR") && R.IsReady())
                            {
                                var target = TargetSelector.GetSelectedTarget() ??
                                             TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                                if (target.IsValidTarget(R.Range))
                                {
                                    R2Logic(target);
                                }
                            }
                            break;
                        case Orbwalking.OrbwalkingMode.Burst:
                            if (R.IsReady())
                            {
                                if (TargetSelector.GetSelectedTarget().IsValidTarget(R.Range))
                                {
                                    var rPred = R.GetPrediction(TargetSelector.GetSelectedTarget(), true);

                                    if (rPred.Hitchance >= HitChance.High)
                                    {
                                        R.Cast(rPred.CastPosition, true);
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case "RivenMartyr":
                    if ((Orbwalking.isCombo || Orbwalking.isBurst) && HeroManager.Enemies.Any(x => x.DistanceToPlayer() <= 400))
                    {
                        CastItem(true);
                    }
                    break;
                case "RivenFeint":
                    if (!R.IsReady())
                    {
                        return;
                    }

                    switch (Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            var target = TargetSelector.GetSelectedTarget() ??
                                         TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                            if (target.IsValidTarget(R.Range))
                            {
                                switch (R.Instance.Name)
                                {
                                    case "RivenFengShuiEngine":
                                        if (Menu.GetKey("R1Combo"))
                                        {
                                            if (target.DistanceToPlayer() <= 500 &&
                                                HeroManager.Enemies.Any(x => x.DistanceToPlayer() <= 500))
                                            {
                                                R.Cast(true);
                                            }
                                        }
                                        break;
                                    case "RivenIzunaBlade":
                                        R2Logic(target);
                                        break;
                                }
                            }
                            break;
                        case Orbwalking.OrbwalkingMode.Burst:
                            if (TargetSelector.GetSelectedTarget().IsValidTarget(R.Range))
                            {
                                switch (R.Instance.Name)
                                {
                                    case "RivenFengShuiEngine":
                                        R.Cast();
                                        break;
                                    case "RivenIzunaBlade":
                                        var rPred = R.GetPrediction(TargetSelector.GetSelectedTarget(), true);

                                        if (rPred.Hitchance >= HitChance.High)
                                        {
                                            R.Cast(rPred.CastPosition, true);
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    break;
                case "RivenIzunaBlade":
                    switch (Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            var target = TargetSelector.GetSelectedTarget() ??
                                         TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                            if (target != null && target.IsValidTarget())
                            {
                                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    CastQ(target);
                                }
                                else if (W.IsReady() && target.IsValidTarget(W.Range))
                                {
                                    W.Cast();
                                }
                            }
                            break;
                        case Orbwalking.OrbwalkingMode.Burst:
                            if (TargetSelector.GetSelectedTarget().IsValidTarget())
                            {
                                if (TargetSelector.GetSelectedTarget().IsValidTarget(Q.Range))
                                {
                                    CastQ(TargetSelector.GetSelectedTarget());
                                }
                                else if (TargetSelector.GetSelectedTarget().IsValidTarget(W.Range) && W.IsReady())
                                {
                                    W.Cast();
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        private static void OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs Args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (Orbwalking.isNone || Orbwalking.isFlee)
            {
                return;
            }

            if (Args.Animation.Contains("c29"))
            {
                QStack = 1;
                ResetQA(Menu.Item("Q1Delay", true).GetValue<Slider>().Value);
            }
            else if (Args.Animation.Contains("c39"))
            {
                QStack = 2;
                ResetQA(Menu.Item("Q2Delay", true).GetValue<Slider>().Value);
            }
            else if (Args.Animation.Contains("c49"))
            {
                QStack = 0;
                ResetQA(Menu.Item("Q3Delay", true).GetValue<Slider>().Value);
            }
            else
            {
                QStack = 0;
            }
        }

        private static void ResetQA(int time)
        {
            if (Menu.Item("Dance", true).GetValue<bool>())
            {
                Game.SendEmote(Emote.Dance);
            }
            Utility.DelayAction.Add(time, () =>
            {
                Game.SendEmote(Emote.Dance);
                Orbwalking.ResetAutoAttackTimer();
                Me.IssueOrder(GameObjectOrder.MoveTo, Me.Position.Extend(Game.CursorPos, +10));
            });
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(Args.SData.Name))
            {
                return;
            }

            if (Args.Target == null)
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var target = Args.Target as Obj_AI_Hero;

                if (target != null && !target.IsDead && !target.IsZombie)
                {
                    CastItem(true, true);

                    if (Q.IsReady())
                    {
                        CastQ(target);
                    }
                    else if (W.IsReady() && target.IsValidTarget(W.Range) && !target.HasBuffOfType(BuffType.SpellShield) &&
                             (target.IsMelee || target.IsFacing(Me) || !Q.IsReady() || Me.HasBuff("RivenFeint") ||
                              QStack != 0))
                    {
                        W.Cast();
                    }
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Burst)
            {
                var target = TargetSelector.GetSelectedTarget();

                if (target != null && !target.IsDead && !target.IsZombie)
                {
                    CastItem(true, true);

                    if (Q.IsReady())
                    {
                        CastQ(target);
                    }
                    else if (W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                var target = Args.Target as Obj_AI_Hero;

                if (target != null && !target.IsDead && !target.IsZombie)
                {
                    CastItem(true);

                    if (Menu.GetBool("HarassQ") && Q.IsReady())
                    {
                        //if (Menu.GetList("HarassMode") == 0)
                        //{
                        //    if (QStack == 2)
                        //    {
                        //        return;
                        //    }

                        //    CastQ(target);
                        //}
                        //else
                        {
                            CastQ(target);
                        }
                    }
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear(Args);
                JungleClear(Args);
            }
        }

        private static void LaneClear(GameObjectProcessSpellCastEventArgs Args)
        {
            if (Menu.GetBool("LaneClearQ") && Q.IsReady())
            {
                if (Args.Target.Type == GameObjectType.obj_AI_Turret || Args.Target.Type == GameObjectType.obj_Turret ||
                    Args.Target.Type == GameObjectType.obj_LampBulb)
                {
                    if (Q.IsReady() && !Args.Target.IsDead)
                    {
                        CastQ((Obj_AI_Base)Args.Target);
                    }
                }
                else
                {
                    var minion = Args.Target as Obj_AI_Minion;
                    var minions = MinionManager.GetMinions(Me.Position, 500f);

                    if (minion != null)
                    {
                        CastItem(true);

                        if (minions.Count >= 2)
                        {
                            CastQ(minion);
                        }
                    }
                }
            }
        }

        private static void JungleClear(GameObjectProcessSpellCastEventArgs Args)
        {
            if (Args.Target is Obj_AI_Minion)
            {
                var mobs = MinionManager.GetMinions(E.Range + Me.AttackRange, MinionTypes.All,
                    MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                var mob = mobs.FirstOrDefault();

                if (mob != null)
                {
                    CastItem(true);

                    if (Menu.GetBool("JungleClearE") && E.IsReady())
                    {
                        E.Cast(mob.Position, true);
                    }
                    else if (Menu.GetBool("JungleClearQ") && Q.IsReady())
                    {
                        CastQ(mob);
                    }
                    else if (Menu.GetBool("JungleClearW") && W.IsReady() &&
                             mob.IsValidTarget(W.Range))
                    {
                        W.Cast(true);
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            QADelaySet();

            if (W.Level > 0)
            {
                W.Range = Me.HasBuff("RivenFengShuiEngine") ? 330 : 260;
            }

            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            Autobool();
            KeelQLogic();
            KillStealLogic();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Burst:
                    Brust();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Flee:
                    FleeLogic();
                    break;
            }
        }

        private static void QADelaySet()
        {//inspiration from Nechrito
            if (Menu.Item("AutoSetDelay", true).GetValue<bool>())
            {
                var delay = 0;

                if (Game.Ping <= 20)
                {
                    delay = Game.Ping;
                }
                else if (Game.Ping <= 50)
                {
                    delay = Game.Ping/2 + 5;
                }
                else
                {
                    delay = Game.Ping/2 - 5;
                }

                Menu.Item("Q1Delay", true).SetValue(new Slider(220 + delay, 200, 300));
                Menu.Item("Q2Delay", true).SetValue(new Slider(220 + delay, 200, 300));
                Menu.Item("Q3Delay", true).SetValue(new Slider(320 + delay, 300, 400));
            }
        }

        private static void Autobool()
        {
            if (QTarget != null && CanQ)
            {
                Q.Cast(QTarget.Position, true);
            }
        }

        private static void KeelQLogic()
        {
            if (Menu.GetBool("KeepQALive") && !Me.UnderTurret(true) && Me.HasBuff("RivenTriCleave"))
            {
                if (Me.GetBuff("RivenTriCleave").EndTime - Game.Time < 0.3)
                {
                    Q.Cast(Me.Position.Extend(Game.CursorPos, 350f), true);
                }
            }
        }

        private static void KillStealLogic()
        {
            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)))
            {
                if (target.Check(R.Range + E.Range - 100))
                {
                    if (W.IsReady() && Menu.GetBool("KillStealW") && target.IsValidTarget(W.Range) &&
                        target.Health < GetWDamage(target))
                    {
                        W.Cast(true);
                    }

                    if (R.IsReady() && Menu.GetBool("KillStealR") && R.Instance.Name == "RivenIzunaBlade" &&
                        GetRDamage(target) > target.Health + target.HPRegenRate)
                    {
                        if (E.IsReady() && Menu.GetBool("KillStealE"))
                        {
                            if (Me.ServerPosition.CountEnemiesInRange(R.Range + E.Range) < 3 &&
                                Me.HealthPercent > 50 && target.IsValidTarget(R.Range + E.Range - 100))
                            {
                                if (E.IsReady())
                                {
                                    E.Cast(target.Position, true);
                                    Utility.DelayAction.Add(100,
                                        () => R.CastIfHitchanceEquals(target, HitChance.High, true));
                                }
                            }
                        }
                        else
                        {
                            if (target.IsValidTarget(R.Range - 50))
                            {
                                R.CastIfHitchanceEquals(target, HitChance.High, true);
                            }
                        }
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetSelectedTarget() ?? 
                TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);

            if (target.Check(900f))
            {
                if (Menu.GetBool("ComboIgnite") && Ignite != SpellSlot.Unknown && Ignite.IsReady() &&
                    GetComboDamage(target) > target.Health)
                {
                    Me.Spellbook.CastSpell(Ignite, target);
                }

                if (Menu.GetBool("ComboW") && W.IsReady() &&
                    target.IsValidTarget(W.Range) && !target.HasBuffOfType(BuffType.SpellShield) && 
                    (target.IsMelee || target.IsFacing(Me) || !Q.IsReady() || Me.HasBuff("RivenFeint") || QStack != 0))
                {
                    W.Cast();
                }

                if (Menu.GetBool("ComboE") && E.IsReady())
                {
                    if (target.DistanceToPlayer() <= W.Range + E.Range &&
                        target.DistanceToPlayer() > Orbwalking.GetRealAutoAttackRange(Me) + 100)
                    {
                        E.Cast(target.IsMelee ? Game.CursorPos : target.ServerPosition);
                    }
                }

                if (Menu.GetBool("ComboR") && R.IsReady())
                {
                    if (Menu.GetKey("R1Combo") && R.Instance.Name == "RivenFengShuiEngine" && !E.IsReady())
                    {
                        if (target.DistanceToPlayer() < 500 && Me.CountEnemiesInRange(500) >= 1)
                        {
                            R.Cast();
                        }
                    }

                    if (R.Instance.Name == "RivenIzunaBlade")
                    {
                        R2Logic(target);
                    }
                }
            }
        }

        private static void R2Logic(Obj_AI_Base target)
        {
            if (target == null || R.Instance.Name == "RivenFengShuiEngine")
            {
                return; 
            }

            if (target.Check(850))
            {
                switch (Menu.GetList("R2Mode"))
                {
                    case 0:
                        if (GetRDamage(target) > target.Health && target.DistanceToPlayer() < 600)
                        {
                            var pred = R.GetPrediction(target, true);

                            if (pred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(pred.CastPosition, true);
                            }
                        }
                        break;
                    case 1:
                        if (target.HealthPercent < 20 ||
                            (target.Health > GetRDamage(target) + Me.GetAutoAttackDamage(target)*2 &&
                             target.HealthPercent < 40) ||
                            (target.Health <= GetRDamage(target)))
                        {
                            var pred = R.GetPrediction(target, true);

                            if (pred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(pred.CastPosition, true);
                            }
                        }
                        break;
                    case 2:
                        if (target.DistanceToPlayer() < 600)
                        {
                            var pred = R.GetPrediction(target, true);

                            if (pred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(pred.CastPosition, true);
                            }
                        }
                        break;
                }
            }
        }

        private static void Brust()
        {
            var target = TargetSelector.GetSelectedTarget();

            if (target != null && !target.IsDead && target.IsValidTarget() && !target.IsZombie)
            {
                if (R.IsReady() && R.Instance.Name == "RivenFengShuiEngine")
                {
                    if (Q.IsReady() && E.IsReady() &&
                        W.IsReady() &&
                        target.Distance(Me.ServerPosition) < E.Range + Me.AttackRange + 100)
                    {
                        E.Cast(target.Position);
                    }

                    if (E.IsReady() &&
                        target.Distance(Me.ServerPosition) < Me.AttackRange + E.Range + 100)
                    {
                        R.Cast();
                        E.Cast(target.Position);
                    }
                }

                if (W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }

                if ((QStack == 1 || QStack == 2 || target.HealthPercent < 50) && R.Instance.Name == "RivenIzunaBlade")
                {
                    R.Cast(target.ServerPosition);
                }

                if (Menu.Item("BurstIgnite", true).GetValue<bool>() && Ignite != SpellSlot.Unknown && Ignite.IsReady())
                {
                    if (target.HealthPercent < 50)
                    {
                        Me.Spellbook.CastSpell(Ignite, target);
                    }
                }

                if (Menu.Item("BurstFlash", true).GetValue<bool>() && Flash != SpellSlot.Unknown)
                {
                    if (Flash.IsReady() && R.IsReady() &&
                        R.Instance.Name == "RivenFengShuiEngine" && E.IsReady() &&
                        W.IsReady() && target.Distance(Me.ServerPosition) <= 780 &&
                        target.Distance(Me.ServerPosition) >= E.Range + Me.AttackRange + 85)
                    {
                        R.Cast();
                        E.Cast(target.Position);
                        Utility.DelayAction.Add(150,
                            () => { Me.Spellbook.CastSpell(Flash, target.Position); });
                    }
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetSelectedTarget() ??  
                TargetSelector.GetTarget(E.Range + Me.BoundingRadius, TargetSelector.DamageType.Physical);

            if (target.Check())
            {
                //if (Menu.GetList("HarassMode") == 0)
                //{
                //    if (QStack == 2)
                //    {
                //        if (E.IsReady() && Menu.GetBool("HarassE"))
                //        {
                //            var pos = Me.ServerPosition +
                //                      (Me.ServerPosition - target.ServerPosition).Normalized()*E.Range;
                //            E.Cast(pos);

                //            if (Menu.GetBool("HarassQ"))
                //            {
                //                Utility.DelayAction.Add(100, () => Q.Cast(pos, true));
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (W.IsReady() && Menu.GetBool("HarassW"))
                //        {
                //            if (target.IsValidTarget(W.Range) && QStack == 1)
                //            {
                //                W.Cast(true);
                //            }
                //        }

                //        if (Q.IsReady() && Menu.GetBool("HarassQ"))
                //        {
                //            if (target.IsValidTarget(Me.AttackRange + Me.BoundingRadius + 150))
                //            {
                //                CastQ(target);
                //            }
                //        }
                //    }
                //}
                //else
                {
                    if (E.IsReady() && Menu.GetBool("HarassE") && target.IsValidTarget(500f))
                    {
                        E.Cast(target.Position, true);
                    }

                    if (W.IsReady() && Menu.GetBool("HarassW") && target.IsValidTarget(W.Range))
                    {
                        W.Cast(true);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            if (!Me.UnderTurret(true) && Menu.GetBool("LaneClearW") && W.IsReady())
            {
                var minions = MinionManager.GetMinions(Me.ServerPosition, W.Range);

                if (minions.Count >= 3)
                {
                    W.Cast(true);
                }
            }
        }

        private static void JungleClear()
        {
            if (Menu.GetBool("JungleClearE") && E.IsReady())
            {
                var mobs = MinionManager.GetMinions(Me.Position, 500f, MinionTypes.All, MinionTeam.Neutral);

                if (!mobs.Any(x => x.DistanceToPlayer() < E.Range) && mobs.Any(x => x.DistanceToPlayer() <= 500f))
                {
                    var mob = mobs.FirstOrDefault();

                    if (mob != null)
                    {
                        E.Cast(mob.Position, true);
                    }
                }
            }

            if (Menu.GetBool("JungleClearWLogic") && W.IsReady())
            {
                var mobs = MinionManager.GetMinions(Me.Position, W.Range, MinionTypes.All, MinionTeam.Neutral);

                if (mobs.Any())
                {
                    if ((!Q.IsReady() && QStack == 0) || ((QStack == 1 || QStack == 2) && Q.IsReady()))
                    {
                        W.Cast(true);
                    }
                }
            }
        }

        private static void FleeLogic()
        {
            if (
                HeroManager.Enemies.Any(
                    x => x.DistanceToPlayer() <= W.Range && !x.HasBuffOfType(BuffType.SpellShield)) && W.IsReady())
            {
                W.Cast(true);
            }

            if (E.IsReady() && !Me.IsDashing() && ((!Q.IsReady() && QStack == 0) || (Q.IsReady() && QStack == 2)))
            {
                E.Cast(Me.Position.Extend(Game.CursorPos, E.Range), true);
            }

            if (Q.IsReady() && !Me.IsDashing())
            {
                Q.Cast(Me.Position.Extend(Game.CursorPos, 350f), true);
            }
        }

        private static void OnDraw(EventArgs Args)
        {
            if (Menu.GetBool("DrawW") && W.IsReady())
            {
                Render.Circle.DrawCircle(Me.Position, W.Range, Color.FromArgb(3, 136, 253), 3);
            }

            if (Menu.GetBool("DrawBurst") && R.Level > 0 && R.IsReady())
            {
                if (E.IsReady() && Flash != SpellSlot.Unknown && Flash.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, 465 + E.Range, Color.FromArgb(3, 136, 253), 3);
                }
            }

            if (Menu.GetBool("DrawRStatus") && R.Level > 0)
            {
                var useRCombo = Menu.Item("R1Combo", true).GetValue<KeyBind>();
                var MePos = Drawing.WorldToScreen(Me.Position);

                Drawing.DrawText(MePos[0] - 40, MePos[1] + 25, Color.MediumSlateBlue,
                    "Use R(" + new string(System.Text.Encoding.Default.GetChars(BitConverter.GetBytes(useRCombo.Key))));
                Drawing.DrawText(MePos[0] + 18, MePos[1] + 25, Color.MediumSlateBlue, "): " + (useRCombo.Active ? "On" : "Off"));
            }
        }

        private static float GetComboDamage(Obj_AI_Hero target)
        {
            if (target == null)
            {
                return 0;
            }

            var damage = 0f;

            if (Q.IsReady())
            {
                damage += GetQDamage(target);
            }

            if (W.IsReady())
            {
                damage += GetWDamage(target);
            }

            if (R.IsReady())
            {
                damage += GetRDamage(target);
            }

            return damage;
        }

        private static double GetPassive
        {
            get
            {
                if (Me.Level == 18)
                {
                    return 0.5;
                }

                if (Me.Level >= 15)
                {
                    return 0.45;
                }

                if (Me.Level >= 12)
                {
                    return 0.4;
                }

                if (Me.Level >= 9)
                {
                    return 0.35;
                }

                if (Me.Level >= 6)
                {
                    return 0.3;
                }

                if (Me.Level >= 3)
                {
                    return 0.25;
                }

                return 0.2;
            }
        }

        private static float GetQDamage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return 0;
            }

            var qhan = 3 - QStack;

            return (float) (Q.GetDamage(target)*qhan + Me.GetAutoAttackDamage(target)*qhan*(1 + GetPassive));
        }

        private static float GetWDamage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return 0;
            }

            return W.GetDamage(target);
        }

        private static float GetRDamage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return 0;
            }

            return (float) Me.CalcDamage(target, Damage.DamageType.Physical,
                (new double[] {80, 120, 160}[R.Level - 1] +
                 0.6*Me.FlatPhysicalDamageMod)*
                (1 + (target.MaxHealth - target.Health)/
                 target.MaxHealth > 0.75
                    ? 0.75
                    : (target.MaxHealth - target.Health)/target.MaxHealth)*8/3);
        }

        private static void CastItem(bool tiamat = false, bool youmuu = false)
        {
            if (tiamat)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                {
                    Items.UseItem(3077);
                }

                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                {
                    Items.UseItem(3074);
                }

                if (Items.HasItem(3053) && Items.CanUseItem(3053))
                {
                    Items.UseItem(3053);
                }
            }

            if (youmuu)
            {
                if (Items.HasItem(3142) && Items.CanUseItem(3142))
                {
                    Items.UseItem(3142);
                }
            }
        }

        private static void CastQ(Obj_AI_Base target)
        {
            CanQ = true;
            QTarget = target;
        }
    }
}