using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Xml.Linq;
using TowerFall;


namespace TFModFortRiseWinCounters
{
  public class MyRollcallElement
  {
    internal static void Load()
    {
      On.TowerFall.RollcallElement.ForceStart += ForceStart_patch;
      On.TowerFall.RollcallElement.StartVersus += StartVersus_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.RollcallElement.ForceStart -= ForceStart_patch;
      On.TowerFall.RollcallElement.StartVersus -= StartVersus_patch;
    }

    public static void ForceStart_patch(On.TowerFall.RollcallElement.orig_ForceStart orig, global::TowerFall.RollcallElement self){
      orig(self);
      TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
    }

    public static void StartVersus_patch(On.TowerFall.RollcallElement.orig_StartVersus orig, global::TowerFall.RollcallElement self){
      orig(self);
      TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
    }
  }
}
