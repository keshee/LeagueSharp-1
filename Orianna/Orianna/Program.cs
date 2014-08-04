using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Orianna
{
    class Program
    {
        public static string ChampionName = "Orianna";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Menu Config;
        public static Vector3 BallPos;
        public static bool isBallMoving;
        public static Obj_AI_Hero target;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region OnGameLoad
        private static void Game_OnGameLoad(EventArgs args)
        {
            OriannaUpdater.InitializeOrianna();
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 825f);
            W = new Spell(SpellSlot.W, 0f);
            E = new Spell(SpellSlot.E, 1095f);
            R = new Spell(SpellSlot.R, 0f);

            Q.SetSkillshot(0f, 80f, 1200f, false, Prediction.SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 275f, float.MaxValue, false, Prediction.SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 100f, 1700f, false, Prediction.SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.6f, 500f, float.MaxValue, false, Prediction.SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            BallPos = ObjectManager.Player.ServerPosition;
            isBallMoving = true;

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
            Config.SubMenu("Combo").AddItem(new MenuItem("UltKillable", "Auto-Ult Killable").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoW", "Auto W Enemies").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("MinTargets", "Minimum Targets to Hit").SetValue(new Slider(1, 5, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("HealthSliderE", "Use E if health <=").SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("ManaSliderHarass", "Mana To Harass").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));
            Config.SubMenu("Farm").AddItem(new MenuItem("ManaSliderFarm", "Mana To Farm").SetValue(new Slider(25, 100, 0)));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("B".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));


            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQRange", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawWRange", "Draw W Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawERange", "Draw E Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawRRange", "Draw R Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameSendPacket += Game_OnSendPacket;
        }
        #endregion

        #region OnDraw
        private static void Drawing_OnDraw(EventArgs args)
        {
            var qValue = Config.Item("DrawQRange").GetValue<Circle>();
            if (qValue.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, Q.Range,
                    qValue.Color);

            var wValue = Config.Item("DrawWRange").GetValue<Circle>();
            if (wValue.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, W.Range,
                    wValue.Color);

            var eValue = Config.Item("DrawERange").GetValue<Circle>();
            if (eValue.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, E.Range,
                    eValue.Color);

            var rValue = Config.Item("DrawRRange").GetValue<Circle>();
            if (rValue.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range,
                    rValue.Color);

            var rValueM = Config.Item("DrawRRangeM").GetValue<Circle>();
            if (rValueM.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range,
                    rValueM.Color, 2, 30, true);
        }
        #endregion

        #region OnGameUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            BallPos = BallManager.CurrentBallPosition;
            isBallMoving = BallManager.IsBallMoving;
            R.UpdateSourcePosition(BallPos);
            W.UpdateSourcePosition(BallPos);
            //Combo & Harass
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active ||
                (Config.Item("HarassActive").GetValue<KeyBind>().Active &&
                    (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100) > Config.Item("ManaSliderHarass").GetValue<Slider>().Value))
            {
                target = SimpleTs.GetTarget(1125f, SimpleTs.DamageType.Magical);
                if (target != null)
                {
                    var comboActive = Config.Item("ComboActive").GetValue<KeyBind>().Active;
                    var harassActive = Config.Item("HarassActive").GetValue<KeyBind>().Active;
                    var useR = Config.Item("UseRCombo").GetValue<bool>();
                    var autoUlt = Config.Item("UltKillable").GetValue<bool>();
                    var useW = Config.Item("UseWCombo").GetValue<bool>();
                    var useQ = Config.Item("UseQCombo").GetValue<bool>();

                    if(comboActive && useQ && useW && useR)
                    {
                        if(isAlone(target) && ObjectManager.Player.ServerPosition.Distance(target.ServerPosition) <= 825 && GetComboDamage(target) >= target.Health)
                        {
                            var prediction = Q.GetPrediction(target);
                            if(prediction.HitChance >= Prediction.HitChance.HighHitchance)
                            {
                                Q.Cast(prediction.Position);
                                CastW(target);
                                if (GetNumberHitByR(target) >= 1)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
                                }
                                if (CastIgnite(target))
                                {
                                    Game.PrintChat("Enemy ignited");
                                }
                            }

                        }
                    }

                    if (((comboActive &&
                          Config.Item("UseQCombo").GetValue<bool>()) ||
                         (harassActive &&
                          Config.Item("UseQHarass").GetValue<bool>())) && Q.IsReady())
                    {
                        var check = getMECQPos(target);
                        var position = check.Item1;
                        var num = check.Item2;

                        if(num == 3 && R.IsReady() && useR)
                        {
                            Q.Cast(position, true);
                        }
                        if(num == 2 && W.IsReady() && useW)
                        {
                            Q.Cast(position, true);
                        }
                        if(num == 1)
                        {
                            Q.Cast(position, true);
                        }
                        if(num == 4)
                        {
                            Q.Cast(position, true);
                        }
                    }

                    if (Config.Item("AutoW").GetValue<bool>() && W.IsReady())
                    {
                        if (!isBallMoving)
                        {
                            CastW(target);
                        }
                    }

                    if (((comboActive && 
                        Config.Item("UseWCombo").GetValue<bool>()) || 
                        (harassActive && 
                        Config.Item("UseWHarass").GetValue<bool>())) && W.IsReady())
                    {
                        if(!isBallMoving)
                        {
                            CastW(target);
                        }
                    }

                    if (((comboActive &&
                          Config.Item("UseECombo").GetValue<bool>()) ||
                         (harassActive &&
                          Config.Item("UseEHarass").GetValue<bool>())) && E.IsReady())
                    {
                        CastE(target);
                    }

                    if (((comboActive && Config.Item("UseRCombo").GetValue<bool>())))
                    {
                        if(!isBallMoving)
                        {
                            CastR(target);
                        }
                    }

                    //R if killable
                    if (comboActive && useR && autoUlt && R.IsReady() && DamageLib.getDmg(target, DamageLib.SpellType.R) > target.Health)
                    {
                        if(willHitRKill(target))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
                        }
                    }
                }
            }


            //Farm
            var shouldLaneClear = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
            if (shouldLaneClear || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
            {
                Farm(shouldLaneClear);
            }

            //Jungle farm.
            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {

                JungleFarm();
            }
        }
        #endregion

        #region Casts

        private static void CastQ(Obj_AI_Base target)
        {
            var prediction = Q.GetPrediction(target);
            if (prediction.HitChance >= Prediction.HitChance.HighHitchance)
            {
                if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) <= Q.Range + Q.Width)
                {
                    Q.Cast(prediction.CastPosition, true);
                }
            }
        }

        private static void CastW(Obj_AI_Base target)
        {
            int hit = GetNumberHitByW(target);
            if(hit >= 1)
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
            }
        }

        private static void CastE(Obj_AI_Base target)
        {
            int numHit = GetNumberHitByE();
            float healthPer = (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100;
            float useEHealthBelow = Config.Item("HealthSliderE").GetValue<Slider>().Value;
            bool useE = healthPer <= useEHealthBelow;
            if (!isBallMoving && BallPos != ObjectManager.Player.Position && numHit >= 1 && useE)
            {
                E.CastOnUnit(ObjectManager.Player);
            }
        }


        private static void CastR(Obj_AI_Base target)
        {
            if (GetNumberHitByR(target) >= Config.Item("MinTargets").GetValue<Slider>().Value)
            {
                R.Cast(target, true, true);
            }
        }
        #endregion

        #region HitCountChecks
        private static int GetNumberHitByW(Obj_AI_Base target)
        {
            int totalHit = 0;
            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (current.IsEnemy && Vector3.Distance(BallPos, current.ServerPosition) <= W.Width - 8)
                {
                    totalHit = totalHit + 1;
                }
            }
            return totalHit;
        }

        private static int GetNumberHitByE()
        {
            List<SharpDX.Vector2> To = new List<Vector2>();
            To.Add(ObjectManager.Player.ServerPosition.To2D());
            return GetECollision(BallPos.To2D(), To, E.Type, E.Width, E.Delay, E.Speed, E.Range).Count;
        }

        private static int GetNumberHitByR(Obj_AI_Base target)
        {
            int totalHit = 0;
            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {
                var prediction = R.GetPrediction(current, true);
                if (prediction.HitChance >= Prediction.HitChance.HighHitchance && !current.IsMe && current.IsEnemy && Vector3.Distance(BallPos, prediction.Position) <= R.Width - current.BoundingRadius - 14)
                {
                    totalHit = totalHit + 1;
                }
            }
            return totalHit;
        }

        #endregion

        #region Utility functions
        private static Tuple<Vector3, int> getMECQPos(Obj_AI_Hero target)
        {
            var pointsList = new List<Vector2>();
            var targetPred = Q.GetPrediction(target);
            if (targetPred.HitChance >= Prediction.HitChance.HighHitchance)
            {
                pointsList.Add(targetPred.Position.To2D());
            }
            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!current.IsMe && current.NetworkId != target.NetworkId && current.IsEnemy && current.IsValidTarget(Q.Range + (R.Width / 2)))
                {
                    var prediction = Q.GetPrediction(current);
                    if(prediction.HitChance >= Prediction.HitChance.HighHitchance)
                    {
                        pointsList.Add(prediction.Position.To2D());
                    }
                }
            }

            while (pointsList.Count != 0)
            {
                var circle = MEC.GetMec(pointsList);
                var numPoints = pointsList.Count;

                if (circle.Radius <= (R.Width / 2) && numPoints >= 3 && R.IsReady())
                {
                    return Tuple.Create(circle.Center.To3D(), 3);
                }

                if (circle.Radius <= (W.Width / 2) && numPoints >= 2 && W.IsReady())
                {
                    return Tuple.Create(circle.Center.To3D(), 2);
                }

                if (pointsList.Count == 1)
                {
                    return Tuple.Create(circle.Center.To3D(), 1);
                }

                else if (circle.Radius <= ((Q.Width / 2) + 50) && numPoints > 1)
                {
                    return Tuple.Create(circle.Center.To3D(), 4);
                }

                try
                {
                    var distance = -1f;
                    var index = 0;
                    var point = pointsList.ElementAt(0); 
                    for (int i = 1; i == numPoints; i++)
                    {
                        if (Vector2.Distance(pointsList.ElementAt(i), point) >= distance)
                        {
                            distance = Vector2.Distance(pointsList.ElementAt(i), point);
                            index = i;
                        }
                    }
                    pointsList.RemoveAt(index);
                }
                catch(System.ArgumentOutOfRangeException)
                {
                    Vector3 outOfRange = new Vector3(0);
                    return Tuple.Create(outOfRange, -1);
                }
            }
            Vector3 noResult = new Vector3(0);
            return Tuple.Create(noResult, -1);
        }

        private static bool willHitRKill(Obj_AI_Base target)
        {
            var prediction = R.GetPrediction(target);
            if (prediction.HitChance >= Prediction.HitChance.HighHitchance && Vector3.Distance(BallPos, prediction.Position) <= R.Width - target.BoundingRadius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<Obj_AI_Base> GetECollision(Vector2 from, List<Vector2> To, Prediction.SkillshotType stype, float width,
            float delay, float speed, float range)
        {
            var result = new List<Obj_AI_Base>();

            foreach (var TestPosition in To)
            {
                foreach (var collisionObject in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (collisionObject.IsValidTarget() && collisionObject.Team != ObjectManager.Player.Team &&
                        Vector2.DistanceSquared(from, collisionObject.Position.To2D()) <= Math.Pow(range * 1.5, 2))
                    {
                        var objectPrediction = Prediction.GetBestPosition(collisionObject, delay, width, speed,
                            from.To3D(), float.MaxValue,
                            false, stype, @from.To3D());
                        if (
                            objectPrediction.Position.To2D().Distance(from, TestPosition, true, true) <=
                            Math.Pow((width + 15 + collisionObject.BoundingRadius), 2))
                        {
                            result.Add(collisionObject);
                            Drawing.DrawCircle(objectPrediction.Position, width + collisionObject.BoundingRadius,
                                Color.Red);
                        }
                    }
                }
            }

            /*Remove duplicates*/
            result = result.Distinct().ToList();
            return result;
        }
        private static void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Cast.Header)
            {
                var decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (decodedPacket.Slot == SpellSlot.R)
                {
                    if(target != null)
                    {
                        if (GetNumberHitByR(target) == 0)
                        {
                            //Block packet if enemies hit is 0
                            args.Process = false;
                        }
                    }
                    if (target == null)
                    {
                        args.Process = false;
                    }
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            var Player = ObjectManager.Player;

            var igniteSlot = Player.GetSpellSlot("SummonerIgnite");
            var igniteReady = ObjectManager.Player.Spellbook.CanUseSpell(igniteSlot) == SpellState.Ready;
            if (igniteSlot != SpellSlot.Unknown && igniteReady)
                damage += DamageLib.getDmg(enemy, DamageLib.SpellType.IGNITE);

            damage += DamageLib.getDmg(enemy, DamageLib.SpellType.AD);

            damage += DamageLib.getDmg(enemy, DamageLib.SpellType.Q);

            if (W.IsReady())
                damage += DamageLib.getDmg(enemy, DamageLib.SpellType.W);

            if (R.IsReady())
                damage += DamageLib.getDmg(enemy, DamageLib.SpellType.R);

            damage += DamageLib.getDmg(enemy, DamageLib.SpellType.AD);

            damage += DamageLib.getDmg(enemy, DamageLib.SpellType.Q);

            return (float)damage;
        }

        private static bool isAlone(Obj_AI_Hero target)
        {
            int numEnemies = 0;
            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!current.IsMe && current.NetworkId != target.NetworkId && current.IsEnemy && current.Distance(target.ServerPosition) <= 1000)
                {
                    numEnemies += 1; 
                }
            }
            if(numEnemies == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static SpellDataInst GetIgnite()
        {
            var spells = ObjectManager.Player.SummonerSpellbook.Spells;
            return spells.FirstOrDefault(spell => spell.Name == "SummonerDot");
        }

        private static bool CastIgnite(Obj_AI_Hero enemy)
        {
            if (!enemy.IsValid || !enemy.IsVisible || !enemy.IsTargetable || enemy.IsDead)
            {
                return false;
            }
            var ignite = GetIgnite();
            if (ignite != null && ignite.Slot != SpellSlot.Unknown && ignite.State == SpellState.Ready &&
                ObjectManager.Player.CanCast)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(ignite.Slot, enemy);
                return true;
            }
            return false;
        }
        #endregion

        #region Farming
        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40))
            {
                return;
            }
            if (Config.Item("ManaSliderFarm").GetValue<Slider>().Value > ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100)
            {
                return;
            }

            var rangedMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.Ranged);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);

            var useQConfig = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWConfig = Config.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEConfig = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQConfig == 1 || useQConfig == 2)) || (!laneClear && (useQConfig == 0 || useQConfig == 2));
            var useW = (laneClear && (useWConfig == 1 || useWConfig == 2)) || (!laneClear && (useWConfig == 0 || useWConfig == 2));
            var useE = (laneClear && (useEConfig == 1 || useEConfig == 2)) || (!laneClear && (useEConfig == 0 || useEConfig == 2));

            if (laneClear)
            {
                if (Q.IsReady() && useQ)
                {
                    var rangedLocation = Q.GetCircularFarmLocation(rangedMinions);
                    var location = Q.GetCircularFarmLocation(allMinions);

                    var betterLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;

                    if (betterLocation.MinionsHit > 0)
                    {
                        Q.Cast(betterLocation.Position.To3D());
                    }
                }

                if (W.IsReady() && useW)
                {
                    int hitCount = W.CountHits(allMinions, BallPos);
                    if (hitCount >= 3)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                    }
                }

                if (E.IsReady() && useE)
                {
                    List<SharpDX.Vector2> To = new List<SharpDX.Vector2>();
                    To.Add(ObjectManager.Player.ServerPosition.To2D());
                    int hitCount = Prediction.GetCollision(BallPos.To2D(), To, E.Type, E.Width, E.Delay, E.Speed, E.Range).Count;
                    if (hitCount >= 1 || (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth < 0.40 && ObjectManager.Player.Mana / ObjectManager.Player.MaxMana >= 0.20))
                    {
                        E.CastOnUnit(ObjectManager.Player);
                    }
                }
            }
            else
            {
                if (useQ && Q.IsReady())
                    foreach (var minion in allMinions)
                    {
                        if (!Orbwalking.InAutoAttackRange(minion))
                        {
                            var prediction = Q.GetPrediction(minion);
                            var Qdamage = DamageLib.getDmg(minion, DamageLib.SpellType.Q) * 0.85;

                            if (Qdamage >= Q.GetHealthPrediction(minion))
                            {
                                Q.Cast(prediction.Position);
                            }
                        }
                    }
            }
        }

        static void JungleFarm()
        {
            var useQ = Config.Item("UseQJFarm").GetValue<bool>();
            var useW = Config.Item("UseWJFarm").GetValue<bool>();
            var useE = Config.Item("UseEJFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (Q.IsReady() && useQ)
                {
                    Q.Cast(mob);
                }

                if (useW && W.IsReady())
                {
                    int hitCount = W.CountHits(mobs, BallPos);
                    if (hitCount >= 1)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                    }
                }

                if (useE && E.IsReady())
                {
                    List<SharpDX.Vector2> To = new List<SharpDX.Vector2>();
                    To.Add(ObjectManager.Player.ServerPosition.To2D());
                    int hitCount = Prediction.GetCollision(BallPos.To2D(), To, E.Type, E.Width, E.Delay, E.Speed, E.Range).Count;
                    float healthPer = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth;
                    float manaPer = ObjectManager.Player.Mana / ObjectManager.Player.MaxMana;
                    if (hitCount >= 1 || (healthPer < 0.40 && manaPer >= 0.20))
                    {
                        E.CastOnUnit(ObjectManager.Player);
                    }
                }
            }
        }
        #endregion
    }
}
