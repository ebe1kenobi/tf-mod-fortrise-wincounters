using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MyPlayerIndicator : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredConstructor(typeof(PlayerIndicator), [
                                                                        typeof(Vector2),
                                                                        typeof(int),
                                                                        typeof(bool)
                                                                    ]),
          postfix: new HarmonyMethod(ctor_patch)
      );
    }

    public MyPlayerIndicator() { }

    public static void ctor_patch(PlayerIndicator __instance, Vector2 offset, int playerIndex, bool crown)
    {
      var dynData = DynamicData.For(__instance);
      dynData.Set("text", CustomNameImport.GetPlayerName(playerIndex));
      dynData.Dispose();
    }
  }
}
