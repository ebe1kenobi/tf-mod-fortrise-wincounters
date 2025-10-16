using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Monocle;
using FortRise;
using TowerFall;
using Newtonsoft.Json;

namespace TFModFortRiseWinCounters
{
  internal class MyTFGame
  {
    // Variable statique pour stocker les noms des joueurs
    public static List<string> PlayerNames = new List<string>();
    static String[] playerInputsName = new String[8];

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
      orig();

      TaskHelper.Run("LOAD CONFIG FILE WITH PLAYER NAME", () =>
      {
        try
        {
          string filePath = @".\Mods\tf-mod-fortrise-wincounters\playerName.json";

          Logger.Info("Loading all player names from file playerName.json...");

          if (!File.Exists(filePath))
          {
            Logger.Error("File not found: " + filePath);
            PlayerNames = new List<string>();
            return;
          }

          string jsonContent = File.ReadAllText(filePath);

          // Désérialisation avec Newtonsoft.Json
          var names = JsonConvert.DeserializeObject<List<string>>(jsonContent);

          if (names == null)
          {
            Logger.Error("No player names found in JSON file.");
            PlayerNames = new List<string>();
          }
          else
          {
            PlayerNames = names;
            Logger.Info("Loaded " + PlayerNames.Count + " player names successfully : " + string.Join(", ", PlayerNames.ToArray()));
          }
        }
        catch (Exception ex)
        {
          TFGame.Log(ex, true);
          TFGame.OpenLog();
          Engine.Instance.Exit();
        }
      });
    }
  }
}