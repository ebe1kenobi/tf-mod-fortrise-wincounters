using System;
using System.IO;

namespace TFModFortRiseWinCounters
{
  // NB : Init() n'est volontairement pas appele par le module (journalisation
  // desactivee). Les methodes ci-dessous sont donc tolerantes a un logger null :
  // sans ce garde, le moindre appel a Logger.Info leverait une
  // NullReferenceException. Appeler Init(dossier) suffit a reactiver l'ecriture.
  public static class Logger {
    static CustomLogger logger;

    public static void Init(string folder) {
      string path = Path.Combine(folder, "logs", $"{DateTime.UtcNow.Ticks / 1000000}.log");
      logger = new CustomLogger(path);
    }

    public static void WriteLine(string message) {
      logger?.WriteLine(message);
    }

    public static void Log(string message, string level) {
      logger?.Log(message, level);
    }

    public static void Error(string message) {
      logger?.Error(message);
    }

    public static void Info(string message) {
      logger?.Info(message);
    }
  }
}
