using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MyVersusPlayerMatchResults
  {
    //public static Dictionary<Color, int> PlayerWinsByColors = new Dictionary<Color, int>();
    //public static Dictionary<Color, int> PlayerTotalWinsByColors = new Dictionary<Color, int>();


    internal static void Load()
    {
      On.TowerFall.VersusPlayerMatchResults.ctor += ctor_patch;
      On.TowerFall.VersusPlayerMatchResults.Render += Render_patch;
    }



    internal static void Unload()
    {
      On.TowerFall.VersusPlayerMatchResults.ctor -= ctor_patch;
      On.TowerFall.VersusPlayerMatchResults.Render -= Render_patch;
    }

    private static void ctor_patch(On.TowerFall.VersusPlayerMatchResults.orig_ctor orig, TowerFall.VersusPlayerMatchResults self, Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
    {
      orig(self, session, matchResults, playerIndex, tweenFrom, tweenTo, awards);
      
      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      //TFModFortRiseWinCountersModule.loadPreviousResultIfExists();

      //if (TFModFortRiseWinCountersModule.Settings.resetTodayCounter) {
      //  TFModFortRiseWinCountersModule.Settings.resetTodayCounter = false; //todo test
      //  MyVersusPlayerMatchResults.PlayerWins = new int[8];
      //  MyVersusPlayerMatchResults.PlayerWinsByColors.Clear();
      //}

      var dynData = DynamicData.For(self);
      var gem = dynData.Get<Sprite<string>>("gem");

      //if (!MyVersusMatchResults.PlayerWinsByColors.ContainsKey(MySession.playerColorForLevel[playerIndex]))
      //{
      //  MyVersusMatchResults.PlayerWinsByColors.Add(MySession.playerColorForLevel[playerIndex], 0);
      //}

      //if (!MyVersusMatchResults.PlayerTotalWinsByColors.ContainsKey(MySession.playerColorForLevel[playerIndex]))
      //{
      //  MyVersusMatchResults.PlayerTotalWinsByColors.Add(MySession.playerColorForLevel[playerIndex], 0);
      //}

      //if (session.MatchStats[playerIndex].Won)
      //{
      //  MyVersusMatchResults.PlayerWinsByColors[MySession.playerColorForLevel[playerIndex]]++;
      //  MyVersusMatchResults.PlayerTotalWinsByColors[MySession.playerColorForLevel[playerIndex]]++;
      //}

      //TFModFortRiseWinCountersModule.SaveCurrentResult();

      //var winText = new OutlineText(TFGame.Font, PlayerWins[playerIndex].ToString(), gem.Position);
      //var winText = new OutlineText(TFGame.Font, MyVersusMatchResults.PlayerWinsByColors[MySession.playerColorForLevel[playerIndex]].ToString() + " (" + MyVersusMatchResults.PlayerTotalWinsByColors[MySession.playerColorForLevel[playerIndex]].ToString() + ")", gem.Position);
      var winText = new OutlineText(TFGame.Font, "-", gem.Position);
      winText.Color = Color.White;
      winText.OutlineColor = Color.Black;
      self.Add(winText);
      dynData.Set("winText", winText);
    }

    private static void Render_patch(On.TowerFall.VersusPlayerMatchResults.orig_Render orig, TowerFall.VersusPlayerMatchResults self)
    {
      orig(self);

      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      var playerIndex = DynamicData.For(self).Get<int>("playerIndex");
      if (DynamicData.For(self).TryGet<OutlineText>("winText", out var text)){
        //text.DrawText = MyVersusMatchResults.PlayerWinsByColors[MySession.playerColorForLevel[playerIndex]].ToString() + " (" + MyVersusMatchResults.PlayerTotalWinsByColors[MySession.playerColorForLevel[playerIndex]].ToString() + ")";
        text.DrawText = MyVersusMatchResults.winCounter.getTodayWin(MySession.playerColorForLevel[playerIndex]) 
                    + " (" + MyVersusMatchResults.winCounter.getTotalWin(MySession.playerColorForLevel[playerIndex]) + ")";
        text.Render();
      }
    }
  }
}
