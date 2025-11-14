using FortRise;

namespace TFModFortRiseWinCounters
{
  public class TFModFortRiseWinCountersSettings: ModuleSettings
  {
    [SettingsName("Enable")]
    public bool enable = false;

    [SettingsName("Display total win after today win")]
    public bool displayTotalWin = false;

    [SettingsName("Reset today counter\n\n(if a new player begin later)")]
    public bool resetTodayCounter = false;

    [SettingsName("Use Online stat (need a config file)")]
    public bool useOnlineStat = false;

  }
}
