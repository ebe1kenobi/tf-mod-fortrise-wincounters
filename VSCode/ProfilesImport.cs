using System;

namespace TFModFortRiseWinCounters
{
  /// <summary>
  /// Acces aux noms de joueurs fournis par le mod Profiles.
  ///
  /// Profiles a repris ce role a CustomName : il publie une interface via
  /// GetApi(), que l'interop de FortRise proxifie sur la forme des membres.
  ///
  /// Repli sur "P1".."P8" si Profiles est absent, pour que les compteurs restent
  /// fonctionnels (les cles de stats seront simplement P1, P2, ...).
  /// </summary>
  public static class ProfilesImport
  {
    internal static IProfilesModApi Api;

    public static bool IsAvailable => Api != null;

    public static String GetPlayerName(int playerIndex)
    {
      if (Api != null)
      {
        try
        {
          string name = Api.GetPlayerName(playerIndex);
          if (!string.IsNullOrEmpty(name))
            return name;
        }
        catch (Exception ex)
        {
          TFModFortRiseWinCounters.Logger.Info($"[Profiles] GetPlayerName({playerIndex}) a echoue : {ex.Message}");
        }
      }

      return "P" + (playerIndex + 1);
    }

    public static void SetPlayerName(int playerIndex, String playerName)
    {
      if (Api == null)
        return;

      try
      {
        Api.SetPlayerName(playerIndex, playerName);
      }
      catch (Exception ex)
      {
        TFModFortRiseWinCounters.Logger.Info($"[Profiles] SetPlayerName({playerIndex}) a echoue : {ex.Message}");
      }
    }
  }
}
