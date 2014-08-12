using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace Riven
{
    internal class Buffmanager
    {
        public static int qStacks;
        public static int aaStacks;
        public static bool windSlashReady;
        public static bool ROn;
        static Buffmanager()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            ROn = RIsOn();
            BuffInstance[] bufflist = ObjectManager.Player.Buffs;
            foreach (var buff in bufflist)
            {
                if (buff.Name == "RivenTriCleave")
                {
                    qStacks = buff.Count;
                }
                if (buff.Name == "rivenpassiveaaboost")
                {
                    aaStacks = buff.Count;
                }
                if(buff.Name == "rivenwindslashready")
                {
                    windSlashReady = true;
                }
            }
            if (!hasQStacks())
            {
                qStacks = 0;
            }
            if(!hasWindSlash())
            {
                windSlashReady = false;
            }
        }

        public static bool hasQStacks()
        {
            BuffInstance[] bufflist = ObjectManager.Player.Buffs;
            foreach (var buff in bufflist)
            {
                if (buff.Name == "RivenTriCleave")
                {
                    return true;
                }
            }
            return false;
        }

        private static bool hasWindSlash()
        {
            BuffInstance[] bufflist = ObjectManager.Player.Buffs;
            foreach (var buff in bufflist)
            {
                if (buff.Name == "rivenwindslashready")
                {
                    return true;
                }
            }
            return false;
        }

        private static bool RIsOn()
        {
            BuffInstance[] bufflist = ObjectManager.Player.Buffs;
            foreach (var buff in bufflist)
            {
                if (buff.Name == "RivenFengShuiEngine")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
