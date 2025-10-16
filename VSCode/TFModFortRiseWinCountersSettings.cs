using FortRise;

namespace TFModFortRiseWinCounters
{
  public class TFModFortRiseWinCountersSettings: ModuleSettings
  {
    [SettingsName("Enable")]
    public bool enable = false;

    [SettingsName("Display total win after today win")]
    public bool displayTotalWin = false;

    public const int MyTeam = 0;
    public const int TeamEricDavidLouis = 1;
    public const int TeamCGI = 2;
    public const int TeamOther = 3;
    public const int TeamEricLouis = 4;
    public const int TeamEricDavid = 5;
    public const int TeamLouisDavid = 6;
    [SettingsName("Team")]
    [SettingsOptions("MyTeam", "TeamEricDavidLouis", "TeamCGI", "TeamOther", "TeamEricLouis", "TeamEricDavid", "TeamLouisDavid")]
    public int team = 0;


    [SettingsName("Reset today counter\n\n(if a new player begin later)")]
    public bool resetTodayCounter = false;

    [SettingsName("Use Online stat\n\n(need a config file)")]
    public bool useOnlineStat = false;

    public string getTeamName() {
      switch (team) {
        case MyTeam: return "MyTeam";
        case TeamEricDavidLouis: return "TeamEricDavidLouis";
        case TeamCGI: return "TeamCGI";
        case TeamOther: return "TeamOther";
        case TeamEricLouis: return "TeamEricLouis";
        case TeamEricDavid: return "TeamEricDavid";
        case TeamLouisDavid: return "TeamLouisDavid";
        default: return "TeamDefault";
      }
    }
  }
}
