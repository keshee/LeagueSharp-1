using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Riven
{
    class Program
    {
        public static string ChampionName = "Riven";

        //Spells
        public static Spell Q;
        public static Spell Q3;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        //RQ
        public static Spell RQ;
        //RQ3
        public static Spell RQ3;
        //RW
        public static Spell RW; 


        //Flags, Menu, Orbwalker
        public static bool ROn = false;
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName) return;
            Game.PrintChat("Riven -  Story of a Broken Blade: Loaded");
            //ToDo: Add Auto Updater

            //SPELLS:
            Q = new Spell(SpellSlot.Q, 260f);
            RQ = new Spell(SpellSlot.Q, 325);
            Q3 = new Spell(SpellSlot.Q, 300);
            RQ3 = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 250);
            RW = new Spell(SpellSlot.W, 270);
            E = new Spell(SpellSlot.E, 390f);
            R = new Spell(SpellSlot.R, 900f);

            //Spell Initilization
            Q.SetSkillshot(0.25f, 50, 780, false, Prediction.SkillshotType.SkillshotCone); //First and Second Q
            RQ.SetSkillshot(0.25f, 50, 780, false, Prediction.SkillshotType.SkillshotCone); // R, First Q, Second Q
            Q3.SetSkillshot(0.40f, 50, 565, false, Prediction.SkillshotType.SkillshotCone); // Third Q
            RQ3.SetSkillshot(0.40f, 50, 565, false, Prediction.SkillshotType.SkillshotCone); // R, Third Q
            E.SetSkillshot(0.25f, 100, 1235, false, Prediction.SkillshotType.SkillshotLine); // E
            R.SetSkillshot(0.25f, 60, 2000, false, Prediction.SkillshotType.SkillshotCone); // R

            //Menu 
            Config = new Menu(ChampionName, ChampionName, true);

            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("FleeActive", "Flee!").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQRange", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawWRange", "Draw W Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawERange", "Draw E Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQRRange", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawWRRange", "Draw W Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawRRange", "Draw R Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.OnGameSendPacket += Game_OnSendPacket;
            Game.OnGameProcessPacket += Game_OnProcessPacket;
            //Orbwalking.AfterAttack += AfterAttack;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var useWCombo = Config.Item("UseWCombo").GetValue<bool>();
            var useECombo = Config.Item("UseECombo").GetValue<bool>();
            var useRCombo = Config.Item("UseRCombo").GetValue<bool>();
            //Fleeing logic 
            if (Config.Item("FleeActive").GetValue<KeyBind>().Active)
            {
                MoveTo(Game.CursorPos);
                if (Buffmanager.qStacks == 0 && Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }
                if(Buffmanager.qStacks > 0 && E.IsReady())
                {
                    if (Buffmanager.qStacks == 2)
                    {
                        if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                        {
                            E.Cast(Game.CursorPos);
                        }
                    }
                    else if (Buffmanager.qStacks != 2 && Q.IsReady())
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
                if(ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Cooldown || ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.NotLearned)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
                if (!Q.IsReady() && Buffmanager.qStacks == 0 && E.IsReady())
                {
                    E.Cast(Game.CursorPos);
                }
                
            }

            // Combo logic (this is getting a major overhaul soon TM
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var target = SimpleTs.GetTarget(1000f, SimpleTs.DamageType.Physical);
                if (target != null)
                {
                    var noRComboDamage = GetComboDamage(target);
                    var RComboDamage = GetRComboDamage(target);
                    bool useRComboToKill = false;

                    if(noRComboDamage < target.Health && RComboDamage >= target.Health)
                    {
                        useRComboToKill = true;
                    }

                    if (Buffmanager.windSlashReady)
                    {
                        float rdamage = R.GetDamage(target);
                        if (rdamage >= target.Health)
                        {
                            R.Cast(target.ServerPosition);
                        }
                    }
                    //E
                    if (useRComboToKill && useECombo && E.IsReady() && useRCombo && R.IsReady() && !Orbwalking.InAutoAttackRange(target) && Vector3.Distance(target.ServerPosition, ObjectManager.Player.ServerPosition) < E.Range + target.BoundingRadius + 75)
                    {
                        E.Cast(target.ServerPosition, true);
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
                    }

                    //W
                    if (Buffmanager.ROn && useWCombo && W.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition)
                        <= RW.Range + target.BoundingRadius)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if(ROn)
            {
                var qRValue = Config.Item("DrawQRRange").GetValue<Circle>();
                if (qRValue.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, RQ.Range,
                        qRValue.Color);

                var wRValue = Config.Item("DrawWRRange").GetValue<Circle>();
                if (wRValue.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, RW.Width,
                        wRValue.Color);

                var rValue = Config.Item("DrawRRange").GetValue<Circle>();
                if (rValue.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Width, rValue.Color);
                }
            }
            else
            {
                var qValue = Config.Item("DrawQRange").GetValue<Circle>();
                if (qValue.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range,
                        qValue.Color);

                var wValue = Config.Item("DrawWRange").GetValue<Circle>();
                if (wValue.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Width,
                        wValue.Color);
            }

            var eValue = Config.Item("DrawERange").GetValue<Circle>();
            if (eValue.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, E.Range,
                    eValue.Color);
        }

        public static void AnimationCancel()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, (ObjectManager.Player.ServerPosition + (new Vector3(5, 5, 0))));
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            String spellName = args.SData.Name;
            if (Config.Item("FleeActive").GetValue<KeyBind>().Active)
            {
                if (spellName == "RivenTriCleave")
                {
                    AnimationCancel();
                }
            }
        }

        private static double GetComboDamage(Obj_AI_Base target)
        {
            var totalDamage = 0d;
            if(Q.IsReady() && !Buffmanager.hasQStacks())
            {
                totalDamage += (DamageLib.getDmg(target, DamageLib.SpellType.Q, DamageLib.StageType.Default));
            }
            if(W.IsReady())
            {
                totalDamage += DamageLib.getDmg(target, DamageLib.SpellType.W);
            }
            totalDamage += (DamageLib.getDmg(target, DamageLib.SpellType.AD)) * 4;

            return totalDamage;
        }

        private static double GetRComboDamage(Obj_AI_Base target)
        {
            var totalDamage = 0d;
            if (Q.IsReady() && !Buffmanager.hasQStacks())
            {
                totalDamage += (DamageLib.getDmg(target, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage));
            }
            if (W.IsReady())
            {
                totalDamage += DamageLib.getDmg(target, DamageLib.SpellType.W);
            }

            totalDamage += ((DamageLib.getDmg(target, DamageLib.SpellType.AD)) * 4) * (ObjectManager.Player.BaseAttackDamage * (ObjectManager.Player.PercentPhysicalDamageMod + 20));

            if (R.IsReady() && !Buffmanager.windSlashReady)
            {
                totalDamage += DamageLib.getDmg(target, DamageLib.SpellType.R);
            }

            return totalDamage;
        }

        private static void Game_OnSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (args.PacketData[0] == 154 && Orbwalker.ActiveMode.ToString() == "Combo")
                {
                    Packet.C2S.Cast.Struct cast = Packet.C2S.Cast.Decoded(args.PacketData);
                    if ((int)cast.Slot >= 0 && (int)cast.Slot <= 4)
                    {
                        Utility.DelayAction.Add(Game.Ping, delegate { AnimationCancel(); });
                    }
                    if (cast.Slot == SpellSlot.Q)
                    {
                        Orbwalking.ResetAutoAttackTimer();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public static void Game_OnProcessPacket(GamePacketEventArgs args)
        {
            try
            {
                if (Orbwalker.ActiveMode.ToString() == "Combo")
                {
                    if (args.PacketData[0] == 101 && Q.IsReady())
                    {
                        GamePacket aaPacket = new GamePacket(args.PacketData);
                        aaPacket.Position = 5;
                        int damageType = (int)aaPacket.ReadByte();
                        int targetID = aaPacket.ReadInteger();
                        int source = aaPacket.ReadInteger();
                        if (ObjectManager.Player.NetworkId != source)
                        {
                            return;
                        }
                        Obj_AI_Hero aaTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(targetID);
                        if (damageType == 12 || damageType == 3)
                        {
                            Q.Cast(aaTarget.ServerPosition);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void MoveTo(Vector3 position)
        {
            if (ObjectManager.Player.ServerPosition.Distance(position) == 0)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player.ServerPosition);
                return;
            }

            var point = ObjectManager.Player.ServerPosition +
                        400 * (position.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized().To3D();
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, point);
        }






    }
}
