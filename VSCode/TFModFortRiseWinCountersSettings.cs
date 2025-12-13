using FortRise;

namespace TFModFortRiseWinCounters
{
  public class TFModFortRiseWinCountersSettings: ModuleSettings
  {
    public override void Create(ISettingsCreate settings)
    {
      settings.CreateOnOff("Enable", enable, (x) => enable = x);
      settings.CreateOnOff("Display total win after today win", displayTotalWin, (x) => displayTotalWin = x);
      settings.CreateOnOff("Reset today counter\n\n(if a new player begin later)", resetTodayCounter, (x) => resetTodayCounter = x);
      settings.CreateOnOff("Use Online stat (need a config file)", useOnlineStat, (x) => useOnlineStat = x);

    }

    //[SettingsName("Enable")]
    public bool enable { get; set; } = false;

    //[SettingsName("Display total win after today win")]
    public bool displayTotalWin { get; set; } = false;

    //[SettingsName("Reset today counter\n\n(if a new player begin later)")]
    public bool resetTodayCounter { get; set; } = false;

    //[SettingsName("Use Online stat (need a config file)")]
    public bool useOnlineStat { get; set; } = false;

  }
}
