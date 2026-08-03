using System;

namespace TFModFortRiseWinCounters
{
  /// <summary>
  /// Acces aux noms de joueurs fournis par le mod CustomName.
  ///
  /// FortRise 4 passait par MonoMod.ModInterop ([ModImportName] + delegues statiques).
  /// CustomName n'exporte plus par ce biais en FortRise 5 : il publie une interface
  /// via GetApi(). Les delegues restaient donc null et les ~10 sites d'appel de
  /// GetPlayerName levaient une NullReferenceException.
  ///
  /// Repli sur "P1".."P8" si CustomName est absent, pour que les compteurs restent
  /// fonctionnels (les cles de stats seront simplement P1, P2, ...).
  /// </summary>
  public static class CustomNameImport
  {
    internal static ICustomNameModApi Api;

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
          TFModFortRiseWinCounters.Logger.Info($"[CustomName] GetPlayerName({playerIndex}) a echoue : {ex.Message}");
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
        TFModFortRiseWinCounters.Logger.Info($"[CustomName] SetPlayerName({playerIndex}) a echoue : {ex.Message}");
      }
    }
  }
}
