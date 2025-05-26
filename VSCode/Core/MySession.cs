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
      //On.TowerFall.Session.StartGame += StartGame_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.Session.StartRound -= StartRound_patch;
      //On.TowerFall.Session.StartGame -= StartGame_patch;
    }

    public static void StartGame_patch(On.TowerFall.Session.orig_StartGame orig, global::TowerFall.Session self)
    {
      //always reload if settings has changed TODO change only if archer selection is used
      //TFModFortRiseWinCountersModule.loadPreviousResultIfExists();

      //if (TFModFortRiseWinCountersModule.Settings.resetTodayCounter)
      //{
      //  TFModFortRiseWinCountersModule.Settings.resetTodayCounter = false;
      //  MyVersusMatchResults.PlayerWinsByColors.Clear();
      //}
      orig(self);
    }

    public static void StartRound_patch(On.TowerFall.Session.orig_StartRound orig, global::TowerFall.Session self)
    {
      orig(self);
      foreach (Entity entity in self.CurrentLevel.Players)
      {
        Player p = (Player)entity;

        if (p != null)
        {
          //save color of each player because players entity will be destroy before result page
          playerColorForLevel[p.PlayerIndex] = p.ArcherData.ColorA;
        }
      }
    }
  }
}
