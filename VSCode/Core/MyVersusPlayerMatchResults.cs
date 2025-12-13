using System;
using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MyVersusPlayerMatchResults : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
                AccessTools.DeclaredConstructor(typeof(VersusPlayerMatchResults), [
                                                                  typeof(Session) ,
                                                                  typeof(VersusMatchResults) , 
                                                                  typeof(int) , 
                                                                  typeof(Vector2) , 
                                                                  typeof(Vector2) , 
                                                                  typeof(List<AwardInfo>)
                                                                          ]),
                postfix: new HarmonyMethod(ctor_patch)
            );

      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(VersusPlayerMatchResults), nameof(VersusPlayerMatchResults.Render)),
          postfix: new HarmonyMethod(Render_patch)
      );
    }


    private static void ctor_patch(VersusPlayerMatchResults __instance, Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
    {
      
      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      var dynData = DynamicData.For(__instance);
      var gem = dynData.Get<Sprite<string>>("gem");
      var winText = new OutlineText(TFGame.Font, "-", gem.Position);
      winText.Color = Color.White;
      winText.OutlineColor = Color.Black;
      __instance.Add(winText);
      dynData.Set("winText", winText);
    }

    private static void Render_patch(VersusPlayerMatchResults __instance)
    {

      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      var playerIndex = DynamicData.For(__instance).Get<int>("playerIndex");
      if (DynamicData.For(__instance).TryGet<OutlineText>("winText", out var text)){

        text.DrawText = MyVersusMatchResults.winCounter.getTodayWin(CustomNameImport.GetPlayerName(playerIndex)) 
                    + (TFModFortRiseWinCountersModule.Settings.displayTotalWin
                        ? " (" + MyVersusMatchResults.winCounter.getTotalWin(CustomNameImport.GetPlayerName(playerIndex)) + ")"
                        : ""
                    );
        text.Render();
      }
      //var dynData = DynamicData.For(self);
      
      //if (TFGame.PlayerInputs[playerIndex] == null) {
      //    Logger.Info("PlayerInputs is null");
      //  return;
      //}

      //InputState inputState = TFGame.PlayerInputs[playerIndex].GetState();
          //Logger.Info(inputState.ToString());
      //if (input == null) {
      //  return;
      //}
      //InputState inputState = input.Invoke<InputState>("GetState");
      //if (inputState.ArrowsPressed)
      //{
      //    Logger.Info("inputState.ArrowsPressed");
      //  self.Scene.Add(new Test());
      //}
    }
  }

  //class Test: Entity {
  //   public Test()
  //    {
  //        Add(new Text(TFGame.Font, "test", new Vector2(60, 60)));
  //    }
  //  }
}
