using System.Threading.Tasks;
using MonoMod.Utils;
using TowerFall;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using Monocle;
using TowerFall;
using Microsoft.Xna.Framework;
using System.Text;

//todo order name in popup by name
// decrease font size to fit more data

namespace TFModFortRiseWinCounters
{
  internal class MyVersusBeginButton
  {

    internal static void Load()
    {
      On.TowerFall.VersusBeginButton.OnConfirm += OnConfirm_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.VersusBeginButton.OnConfirm -= OnConfirm_patch;
    }

    public static void OnConfirm_patch(On.TowerFall.VersusBeginButton.orig_OnConfirm orig, global::TowerFall.VersusBeginButton self)
    {
      TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
      orig(self);
    }
  }
}
