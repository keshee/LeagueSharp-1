// In game skin changer, by Trelli

using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Windows.Input;
using LeagueSharp;

namespace SkinChanger
{
    public class SkinChanger
    {
        int currSkinId = 0
        bool canChange = true

        Dictionary<string, int> numSkins = new Dictionary<string, int>();
        numSkins.add("Aatrox", 1);
        numSkins.add("Ahri", 4);
        numSkins.add("Akali", 6);
        numSkins.add("Alistar", 7);
        numSkins.add("Amumu", 1);
        numSkins.add("Anivia", 5);
        numSkins.add("Annie", 8);
        numSkins.add("Ashe", 6);
        numSkins.add("Blitzcrank", 7);
        numSkins.add("Brand", 4);
        numSkins.add("Braum", 1);
        numSkins.add("Caitlyn", 5);
        numSkins.add("Cassiopeia", 4);
        numSkins.add("Chogath", 5);
        numSkins.add("Corki", 6);
        numSkins.add("Darius", 3);
        numSkins.add("Diana", 2);
        numSkins.add("Draven", 3);
        numSkins.add("DrMundo", 7);
        numSkins.add("Elise", 2);
        numSkins.add("Evelynn", 3);
        numSkins.add("Ezreal", 6);
        numSkins.add("Fiddlesticks", 8);
        numSkins.add("Fiora", 3);
        numSkins.add("Fizz", 4);
        numSkins.add("Galio", 4);
        numSkins.add("Gangplank", 6);
        numSkins.add("Garen", 6);
        numSkins.add("Gragas", 7);
        numSkins.add("Graves", 5);
        numSkins.add("Hecarim", 5);
        numSkins.add("Heimerdinger", 5);
        numSkins.add("Irelia", 4);
        numSkins.add("Janna", 5);
        numSkins.add("JarvanIV", 5);
        numSkins.add("Jax", 8);
        numSkins.add("Jayce", 2);
        numSkins.add("Jinx", 1);
        numSkins.add("Karma", 3);
        numSkins.add("Karthus", 4);
        numSkins.add("Kassadin", 4);
        numSkins.add("Katarina", 7);
        numSkins.add("Kayle", 6);
        numSkins.add("Kennen", 5);
        numSkins.add("Khazix", 1);
        numSkins.add("KogMaw", 7);
        numSkins.add("Leblanc", 3);
        numSkins.add("LeeSin", 6);
        numSkins.add("Leona", 4);
        numSkins.add("Lissandra", 2);
        numSkins.add("Lucian", 2);
        numSkins.add("Lulu", 4);
        numSkins.add("Lux", 5);
        numSkins.add("Malphite", 5);
        numSkins.add("Malzahar", 4);
        numSkins.add("Maokai", 5);
        numSkins.add("Masteryi", 5);
        numSkins.add("MasterYi", 5);
        numSkins.add("MissFortune", 6);
        numSkins.add("MonkeyKing", 3);
        numSkins.add("Mordekaiser", 4);
        numSkins.add("Morgana", 5);
        numSkins.add("Nami", 2);
        numSkins.add("Nasus", 5);
        numSkins.add("Nautilus", 3);
        numSkins.add("Nidalee", 6);
        numSkins.add("Nocturne", 5);
        numSkins.add("Nunu", 6);
        numSkins.add("Olaf", 4);
        numSkins.add("Orianna", 4);
        numSkins.add("Pantheon", 6);
        numSkins.add("Poppy", 6);
        numSkins.add("Quinn", 2);
        numSkins.add("Rammus", 6);
        numSkins.add("Random", 0);
        numSkins.add("Renekton", 6);
        numSkins.add("Rengar", 2);
        numSkins.add("Riven", 5);
        numSkins.add("Rumble", 3);
        numSkins.add("Ryze", 8);
        numSkins.add("Sejuani", 4);
        numSkins.add("Shaco", 6);
        numSkins.add("Shen", 6);
        numSkins.add("Shyvana", 4);
        numSkins.add("Singed", 6);
        numSkins.add("Sion", 4);
        numSkins.add("Sivir", 6);
        numSkins.add("Skarner", 2);
        numSkins.add("Sona", 5);
        numSkins.add("Soraka", 3);
        numSkins.add("Swain", 3);
        numSkins.add("Syndra", 2);
        numSkins.add("Talon", 3);
        numSkins.add("Taric", 3);
        numSkins.add("Teemo", 7);
        numSkins.add("Thresh", 2);
        numSkins.add("Tristana", 6);
        numSkins.add("Trundle", 3);
        numSkins.add("Tryndamere", 6);
        numSkins.add("TwistedFate", 8);
        numSkins.add("Twitch", 5);
        numSkins.add("Udyr", 3);
        numSkins.add("Urgot", 3);
        numSkins.add("Varus", 3);
        numSkins.add("Vayne", 5);
        numSkins.add("Veigar", 7);
        numSkins.add("Velkoz", 1);
        numSkins.add("Viktor", 3);
        numSkins.add("Vi", 2);
        numSkins.add("Vladimir", 6);
        numSkins.add("Volibear", 3);
        numSkins.add("Warwick", 7);
        numSkins.add("Xerath", 3);
        numSkins.add("XinZhao", 5);
        numSkins.add("Yasuo", 1);
        numSkins.add("Yorick", 2);
        numSkins.add("Zac", 1);
        numSkins.add("Zed", 3);
        numSkins.add("Ziggs", 4);
        numSkins.add("Zilean", 4);
        numSkins.add("Zyra", 3);
    }

    public SkinChanger()
    {
        Game.OnGameStart += OnGameStart;
        Game.OnGameUpdate += OnGameUpdate;

    }

    public void OnGameStart(EventArgs args)
    {
        Game.PrintChat(
            string.Format(
                "{0} v{1} by trelli loaded. Press 9 to cycle your skin.",
                Assembly.GetExecutingAssembly().GetName().Name,
                Assembly.GetExecutingAssembly().GetName().Version
            )
        );
    }

    public void OnGameUpdate(EventArgs args)
    {
        if(System.Windows.Input.Keyboard.IsKeyToggled(Key.Numpad9)
        {
            if(canChange)
            {
                canChange = false; 
                GenerateSkinPacket(ObjectManagr.Player.SkinName, currSkinId);
                if(numSkins[ObjectManagr.Player.SkinName] > currSkinId)
                {
                    currSkinId = currSkinId + 1; 
                }
                else
                {
                    currSkinId = 0; 
                }
            }
        }
        else
        {
            canChange = true; 
        }
    }

    public void GenerateSkinPacket(string currentChampion, int skinNumber, GameProcessPacket args)
    {
        //This needs to be updated for L# API. Functionality is here, needs packet investigations. 
        p = CLoLPacket(0x97)
        p:EncodeF(myHero.networkID)
        p.pos = 1
        t1 = p:Decode1()
        t2 = p:Decode1()
        t3 = p:Decode1()
        t4 = p:Decode1()
        p:Encode1(t1)
        p:Encode1(t2)
        p:Encode1(t3)
        p:Encode1(bit32.band(t4,0xB))
        p:Encode1(1)--hardcode 1 bitfield
        p:Encode4(skinId)
        for i = 1, #champ do
            p:Encode1(string.byte(champ:sub(i,i)))
        end
        for i = #champ + 1, 64 do
            p:Encode1(0)
        end
        p:Hide()
        RecvPacket(p)
    }
}
