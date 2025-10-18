using Monocle;
using MonoMod.Utils;
using System;
using TFModFortRiseWinCounters;

namespace TFModFortRiseLoaderAI
{
  public class MyVersusRoundResults
  {
    internal static void Load()
    {
      On.TowerFall.VersusRoundResults.Update += Update_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.VersusRoundResults.Update -= Update_patch;
    }
    public MyVersusRoundResults() { }

    public static void Update_patch(On.TowerFall.VersusRoundResults.orig_Update orig, global::TowerFall.VersusRoundResults self)
    {
      if (self.Components == null)
      {
        orig(self);
        return;
      }
      for (var i = 0; i < self.Components.Count; i++)
      {
        if (self.Components[i].GetType().ToString() != "Monocle.Text") continue;
        Text text = (Text)self.Components[i];
        var dynData = DynamicData.For(text);
        String textText = (String)dynData.Get("text");
        if (textText.Length == 0) continue;
        if (!textText[0].ToString().Equals("P")) continue;
        if (textText[1].ToString().Equals(" ")) continue; //second pass for NAI 1 AI 1 P 1
        int playerIndex = int.Parse(textText[1].ToString()) - 1;
        var dynDataName = DynamicData.For(MyRollcallElement.playerName[playerIndex]);
        dynData.Set("text", dynDataName.Get("text"));
        text.Position.X = 20;

        dynDataName.Dispose();
        dynData.Dispose();
      }
      orig(self);
    }
  }
}
