using System.Threading.Tasks;
using MonoMod.Utils;
using TowerFall;
using System;

namespace TFModFortRiseWinCounters
{
  internal class MyVersusMatchResults
  {
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
      orig(self, session, roundResults);

      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      if (TFModFortRiseWinCountersModule.ReloadNecessary) //TODO test !message not displayed 
      {
        TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
        TFModFortRiseWinCountersModule.ReloadNecessary = false;
      }

      if (TFModFortRiseWinCountersModule.Settings.resetTodayCounter)
      {
        TFModFortRiseWinCountersModule.Settings.resetTodayCounter = false;
        winCounter.resetToday();
      }

      for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++)
      {
        if (!TFGame.Players[playerIndex]) continue;

        if (session.MatchStats[playerIndex].Won)
        {
          winCounter.increment(CustomNameImport.GetPlayerName(playerIndex));
        }
      }

      //need to save each time
      Task.Factory.StartNew(() => TFModFortRiseWinCountersModule.SaveCurrentResult());
    }
  }
}
