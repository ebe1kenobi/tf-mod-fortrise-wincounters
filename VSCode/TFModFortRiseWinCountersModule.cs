using System;
using FortRise;

using System.IO;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace TFModFortRiseWinCounters
{
  [Fort("com.ebe1.kenobi.tfmodfortrisewincounters", "TFModFortRiseWinCountersModule")]
  public class TFModFortRiseWinCountersModule : FortModule
  {
    public static TFModFortRiseWinCountersModule Instance;

    public TFModFortRiseWinCountersModule() 
    {
      Instance = this;
      Logger.Init("TFModFortRiseWinCounters");
    }

    public override void LoadContent()
    {
    }

    public override void Load()
    {
      MyTFGame.Load();
      MyVersusPlayerMatchResults.Load();
      MySession.Load();
    }

    public override void Unload()
    {
      MyTFGame.Unload();
      MyVersusPlayerMatchResults.Unload();
      MySession.Unload();
    }

    public static void LoadFromFile(string filePath, bool loadOnlyTotal)
    {
      try
      {
        string json = File.ReadAllText(filePath);
        WinCounterData data = JsonConvert.DeserializeObject<WinCounterData>(json);
        Logger.Info(json);

        if (data != null)
        {
          if (!loadOnlyTotal)
          {
            MyVersusPlayerMatchResults.PlayerWins = data.wins;
            MyVersusPlayerMatchResults.PlayerWinsByColors = data.todayWins;
          }
          MyVersusPlayerMatchResults.PlayerTotalWinsByColors = data.totalWins;
          Logger.Info("Fichier stat chargé.");
        }
      }
      catch (Exception ex)
      {
        Logger.Info("Erreur lors de la lecture du fichier : " + ex.Message);
      }
    }

    private class WinCounterData
    {
      public String info{ get; set; }
      public String version { get; set; }
      public DateTime date { get; set; }
      public int[] wins { get; set; }
      public Dictionary<Color, int> todayWins { get; set; }
      public Dictionary<Color, int> totalWins { get; set; }
    }

    public static void SaveCurrentResult()
    {
      string today = DateTime.Now.ToString("yyyy-MM-dd");
      string fileName = today + "-v-wincounters.json";

      var data = new WinCounterData
      {
        version = "v1",
        info =  "248, 120, 248, 255 = pink\n" +
                "0, 184, 0, 255 = green\n" +
                "239, 140, 33, 255 = orange\n" +
                "60, 159, 252, 255 = blue\n" +
                "211, 0, 0, 255 = red\n" +
                "242, 255, 0, 255 = yellow\n" +
                "122, 66, 255, 255 = purple\n" +
                "0, 255, 246, 255 = blue soft\n" +
                "229, 229, 229, 255 = white",
        date = DateTime.Now,
        wins = MyVersusPlayerMatchResults.PlayerWins,
        todayWins = MyVersusPlayerMatchResults.PlayerWinsByColors,
        totalWins = MyVersusPlayerMatchResults.PlayerTotalWinsByColors
      };

      try
      {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(fileName, json);
        Console.WriteLine("Fichier stat sauvegardé : " + fileName);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Erreur lors de la sauvegarde du fichier : " + ex.Message);
      }
    }

    public static void loadPreviousResultIfExists()
    {
      string today = DateTime.Now.ToString("yyyy-MM-dd");
      //string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

      string todayFile = today + "-v-wincounters.json";
      //string yesterdayFile = yesterday + "-wincounters.json";

      // if not exists all counter are set to 0
      if (File.Exists(todayFile))
      {
        TFModFortRiseWinCountersModule.LoadFromFile(todayFile, false);
        return;
      }

      //load totalWins from last file found
      var files = Directory
          .EnumerateFiles(Directory.GetCurrentDirectory(), "*-v-wincounters.json")
          .Select(path => new
          {
            Path = path,
            // On essaie de parser les 10 premiers caractères en date
            Date = DateTime.TryParseExact(
                  Path.GetFileName(path).Substring(0, 10),
                  "yyyy-MM-dd",
                  CultureInfo.InvariantCulture,
                  DateTimeStyles.None,
                  out var dt)
                  ? dt
                  : (DateTime?)null
          })
          // Garde ceux dont la date est valide et avant aujourd'hui
          .Where(x => x.Date.HasValue && x.Date.Value < DateTime.Today)
          // Tri par date descendante
          .OrderByDescending(x => x.Date.Value)
          .ToList();

      if (files.Any())
      {
        // On prend le plus récent
        string lastFile = files.First().Path;
        TFModFortRiseWinCountersModule.LoadFromFile(lastFile, true);
      }
    }
  }
}
