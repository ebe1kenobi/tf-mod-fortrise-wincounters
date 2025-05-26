using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerFall;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime;

namespace TFModFortRiseWinCounters
{
  internal class MyVersusMatchResults
  {
    //public static Dictionary<Color, int> PlayerWinsByColors = new Dictionary<Color, int>();
    //public static Dictionary<Color, int> PlayerTotalWinsByColors = new Dictionary<Color, int>();
    public static WinCounterData winCounter = new WinCounterData();

    internal static void Load()
    {
      On.TowerFall.VersusMatchResults.ctor += ctor_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.VersusMatchResults.ctor -= ctor_patch;
    }

    public static void ctor_patch(On.TowerFall.VersusMatchResults.orig_ctor orig, global::TowerFall.VersusMatchResults self, global::TowerFall.Session session, global::TowerFall.VersusRoundResults roundResults)
    {
      Logger.Info("MyVersusMatchResults ctor_patch");
      orig(self, session, roundResults);

      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      if (TFModFortRiseWinCountersModule.ReloadNecessary) //TODO test !message not displayed 
      {
        Logger.Info("ReloadNecessary stat");
        TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
        TFModFortRiseWinCountersModule.ReloadNecessary = false;
      }

      if (TFModFortRiseWinCountersModule.Settings.resetTodayCounter)
      {
        Logger.Info("resetTodayCounter stat");
        TFModFortRiseWinCountersModule.Settings.resetTodayCounter = false;
        //MyVersusMatchResults.PlayerWinsByColors.Clear();
        winCounter.resetToday();
      }

      for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++)
      {
        if (!TFGame.Players[playerIndex]) continue;

        //if (!MyVersusMatchResults.PlayerWinsByColors.ContainsKey(MySession.playerColorForLevel[playerIndex]))
        //{
        //  MyVersusMatchResults.PlayerWinsByColors.Add(MySession.playerColorForLevel[playerIndex], 0);
        //}

        //if (!MyVersusMatchResults.PlayerTotalWinsByColors.ContainsKey(MySession.playerColorForLevel[playerIndex]))
        //{
        //  MyVersusMatchResults.PlayerTotalWinsByColors.Add(MySession.playerColorForLevel[playerIndex], 0);
        //}

        if (session.MatchStats[playerIndex].Won)
        {
          //MyVersusMatchResults.PlayerWinsByColors[MySession.playerColorForLevel[playerIndex]]++;
          //MyVersusMatchResults.PlayerTotalWinsByColors[MySession.playerColorForLevel[playerIndex]]++;
          winCounter.increment(MySession.playerColorForLevel[playerIndex]);
        }
      }

      //need to save each time
      //TFModFortRiseWinCountersModule.SaveCurrentResult(); //todo async
      Task.Factory.StartNew(() => TFModFortRiseWinCountersModule.SaveCurrentResult());
    }
  }
}
