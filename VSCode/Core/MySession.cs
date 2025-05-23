using TowerFall;
using Microsoft.Xna.Framework;
using Monocle;

namespace TFModFortRiseWinCounters
{
  public class MySession
  {

    public static Color[] playerColorForLevel = new Color[8];

    internal static void Load()
    {
      On.TowerFall.Session.StartRound += StartRound_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.Session.StartRound -= StartRound_patch;
    }

    public static void StartRound_patch(On.TowerFall.Session.orig_StartRound orig, global::TowerFall.Session self)
    {
      orig(self);
      foreach (Entity entity in self.CurrentLevel.Players)
      {
        Player p = (Player)entity;

        if (p != null)
        {
          playerColorForLevel[p.PlayerIndex] = p.ArcherData.ColorA;
        }
      }
    }
  }
}
