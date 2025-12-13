using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Xml.Linq;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using TowerFall;


namespace TFModFortRiseWinCounters
{
  public class MyRollcallElement : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(RollcallElement), "ForceStart"),
          postfix: new HarmonyMethod(ForceStart_patch)
      );
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(RollcallElement), "StartVersus"),
          postfix: new HarmonyMethod(StartVersus_patch)
      );
    }

    public static void ForceStart_patch(RollcallElement __instance)
    {
      TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
    }

    public static void StartVersus_patch(RollcallElement __instance)
    {
      TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
    }
  }
}
