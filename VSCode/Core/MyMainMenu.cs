using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFModFortRiseWinCounters
{
  internal class MyMainMenu
  {
    internal static void Load()
    {
      On.TowerFall.MainMenu.CreateRollcall += CreateRollcall_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.MainMenu.CreateRollcall -= CreateRollcall_patch;
    }

    public static void CreateRollcall_patch(On.TowerFall.MainMenu.orig_CreateRollcall orig, global::TowerFall.MainMenu self)
    {
      Logger.Info("CreateRollcall_patch program reload stat");
      TFModFortRiseWinCountersModule.ReloadNecessary = true;
      orig(self);
    }
  }
}
