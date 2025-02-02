using FortRise;

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
