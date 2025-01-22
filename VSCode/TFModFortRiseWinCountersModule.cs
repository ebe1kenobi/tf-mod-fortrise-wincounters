using System;
using System.IO;
using System.Reflection;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  [Fort("com.ebe1.kenobi.tfmodfortrisewincounters", "TFModFortRiseWinCountersModule")]
  public class TFModFortRiseWinCountersModule : FortModule
  {
    public static TFModFortRiseWinCountersModule Instance;

    public TFModFortRiseWinCountersModule() 
    {
      Instance = this;
    }

    public override void LoadContent()
    {
      
    }

    public override void Load()
    {
      MyTFGame.Load();
      MyVersusPlayerMatchResults.Load();
    }


    public override void Unload()
    {
      MyTFGame.Unload();
      MyVersusPlayerMatchResults.Unload();
    }
  }
}
