using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{

  internal class MyTFGame
  {

    internal static void Load()
    {
      On.TowerFall.TFGame.Load += Load_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.TFGame.Load -= Load_patch;
    }

    private static void Load_patch(On.TowerFall.TFGame.orig_Load orig)
    {
      MyVersusPlayerMatchResults.PlayerWins = new int[8];
      orig();
    }
  }
}
