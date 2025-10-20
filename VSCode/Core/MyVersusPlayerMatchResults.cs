using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MyVersusPlayerMatchResults
  {
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

      var dynData = DynamicData.For(self);
      var gem = dynData.Get<Sprite<string>>("gem");
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

        text.DrawText = MyVersusMatchResults.winCounter.getTodayWin(CustomNameImport.GetPlayerName(playerIndex)) 
                    + (TFModFortRiseWinCountersModule.Settings.displayTotalWin
                        ? " (" + MyVersusMatchResults.winCounter.getTotalWin(CustomNameImport.GetPlayerName(playerIndex)) + ")"
                        : ""
                    );
        text.Render();
      }
    }
  }
}
