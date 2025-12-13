using System;
using FortRise;
using HarmonyLib;
using Monocle;
using MonoMod.Utils;
using TowerFall;
//using TFModFortRiseWinCounters;

namespace TFModFortRiseWinCounters
{
  public class MyVersusRoundResults : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(VersusRoundResults), nameof(VersusRoundResults.Update)),
          prefix: new HarmonyMethod(Update_patch)
      );
    }

    public MyVersusRoundResults() { }

    public static void Update_patch(VersusRoundResults __instance)
    {
      if (__instance.Components == null)
      {
        return;
      }
      for (var i = 0; i < __instance.Components.Count; i++)
      {
        if (__instance.Components[i].GetType().ToString() != "Monocle.Text") continue;
        Text text = (Text)__instance.Components[i];
        var dynData = DynamicData.For(text);
        String textText = (String)dynData.Get("text");
        if (textText.Length == 0) continue;
        if (!textText[0].ToString().Equals("P")) continue;
        if (textText[1].ToString().Equals(" ")) continue; //second pass for NAI 1 AI 1 P 1
        int playerIndex = int.Parse(textText[1].ToString()) - 1;
        dynData.Set("text", CustomNameImport.GetPlayerName(playerIndex));
        text.Position.X = 20;
        dynData.Dispose();
      }
    }
  }
}
