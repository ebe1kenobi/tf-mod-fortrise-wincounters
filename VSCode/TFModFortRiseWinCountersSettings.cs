using FortRise;

namespace TFModFortRiseWinCounters
{
  public class TFModFortRiseWinCountersSettings: ModuleSettings
  {
    [SettingsName("Enable")]
    public bool enable = false;

    public const int MyTeam = 0;
    public const int TeamEricDavidLouis = 1;
    public const int TeamCGI = 2;
    public const int TeamOther = 3;
    [SettingsName("Team")]
    [SettingsOptions("MyTeam", "TeamEricDavidLouis", "TeamCGI", "TeamOther")]
    public int team = 0;


    [SettingsName("Reset today counter")]
    public bool resetTodayCounter = false;

    [SettingsName("Use Online stat")]
    public bool useOnlineStat = false;

    public string getTeamName() {
      switch (team) {
        case MyTeam: return "MyTeam";
        case TeamEricDavidLouis: return "TeamEricDavidLouis";
        case TeamCGI: return "TeamCGI";
        case TeamOther: return "TeamOther";
        default: return "TeamDefault";
      }
    }
  }
}
