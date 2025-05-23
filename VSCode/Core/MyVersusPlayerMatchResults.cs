using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;



using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MyVersusPlayerMatchResults
  {
    public static int[] PlayerWins;
    public static Dictionary<Color, int> PlayerWinsByColors = new Dictionary<Color, int>();
    public static Dictionary<Color, int> PlayerTotalWinsByColors = new Dictionary<Color, int>();


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
      var dynData = DynamicData.For(self);
      var gem = dynData.Get<Sprite<string>>("gem");

      if (!PlayerWinsByColors.ContainsKey(MySession.playerColorForLevel[playerIndex]))
      {
        PlayerWinsByColors.Add(MySession.playerColorForLevel[playerIndex], 0);
      }

      if (!PlayerTotalWinsByColors.ContainsKey(MySession.playerColorForLevel[playerIndex]))
      {
        PlayerTotalWinsByColors.Add(MySession.playerColorForLevel[playerIndex], 0);
      }

      if (session.MatchStats[playerIndex].Won)
      {
        PlayerWins[playerIndex]++;
        PlayerWinsByColors[MySession.playerColorForLevel[playerIndex]]++;
        PlayerTotalWinsByColors[MySession.playerColorForLevel[playerIndex]]++;
      }
      TFModFortRiseWinCountersModule.SaveCurrentResult();
      //var winText = new OutlineText(TFGame.Font, PlayerWins[playerIndex].ToString(), gem.Position);
      var winText = new OutlineText(TFGame.Font, PlayerWinsByColors[MySession.playerColorForLevel[playerIndex]].ToString() + " (" + PlayerTotalWinsByColors[MySession.playerColorForLevel[playerIndex]].ToString() + ")", gem.Position);
      winText.Color = Color.White;
      winText.OutlineColor = Color.Black;
      self.Add(winText);
      dynData.Set("winText", winText);
    }

    private static void Render_patch(On.TowerFall.VersusPlayerMatchResults.orig_Render orig, TowerFall.VersusPlayerMatchResults self)
    {
      orig(self);
      var playerIndex = DynamicData.For(self).Get<int>("playerIndex");
      if (DynamicData.For(self).TryGet<OutlineText>("winText", out var text))
        text.Render();
    }
  }
}
