using System;
using FortRise;

using System.IO;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TFModFortRiseWinCounters
{
  [Fort("com.ebe1.kenobi.tfmodfortrisewincounters", "TFModFortRiseWinCountersModule")]
  public class TFModFortRiseWinCountersModule : FortModule
  {
    public static TFModFortRiseWinCountersModule Instance;

    public override Type SettingsType => typeof(TFModFortRiseWinCountersSettings);
    public static TFModFortRiseWinCountersSettings Settings => (TFModFortRiseWinCountersSettings)Instance.InternalSettings;

    public static APIStat ApiStat;
    public static bool ReloadNecessary = false;

    public TFModFortRiseWinCountersModule() 
    {
      Instance = this;
      Logger.Init("TFModFortRiseWinCounters");
      ApiStat = new APIStat(".\\Mods\\tf-mod-fortrise-wincounters\\settings.json");
    }

    public override void LoadContent()
    {
    }

    public override void Load()
    {
      MyMainMenu.Load();
      MyVersusPlayerMatchResults.Load();
      MySession.Load();
      MyVersusMatchResults.Load();
    }

    public override void Unload()
    {
      MyMainMenu.Unload();
      MyVersusPlayerMatchResults.Unload();
      MySession.Unload();
      MyVersusMatchResults.Unload();
    }

    //public static void LoadFromString(string json, bool loadOnlyTotal)
    //{
    //  Logger.Info("LoadFromString()");
    //  Logger.Info(json);
    //  try
    //  {
    //    WinCounterData data = JsonConvert.DeserializeObject<WinCounterData>(json);
    //    //Logger.Info(json);

    //    if (data != null)
    //    {
    //      if (!loadOnlyTotal)
    //      {
    //        MyVersusMatchResults.PlayerWinsByColors = data.todayWins;
    //      }
    //      MyVersusMatchResults.PlayerTotalWinsByColors = data.totalWins;
    //      Logger.Info("Fichier stat chargé.");
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    Logger.Info("Erreur lors de la lecture du json : " + ex.Message);
    //  }
    //}
    public static void LoadFromFile(string filePath, bool loadOnlyTotal)
    {
      Logger.Info("LoadFromFile()");
      Logger.Info(filePath);
      try
      {
        string json = File.ReadAllText(filePath);
        //LoadFromString(json, loadOnlyTotal);
        MyVersusMatchResults.winCounter = JsonConvert.DeserializeObject<WinCounterData>(json);
        if (MyVersusMatchResults.winCounter != null) {
          if (loadOnlyTotal) {
            MyVersusMatchResults.winCounter.resetToday();
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Info("Erreur lors de la lecture du fichier : " + ex.Message);
      }
    }


    public static string getFileSuffix() {

      return Settings.getTeamName();
    }

    public static void SaveCurrentResult()
    {
      string today = DateTime.Now.ToString("yyyy-MM-dd");
      string fileName = today + "-" + getFileSuffix() + "-wincounters.json";

      //var data = new WinCounterData
      //{
      //  version = "v1",
      //  date = DateTime.Now.ToString("yyyy-MM-dd-HH"),
      //  todayWins = MyVersusMatchResults.PlayerWinsByColors,
      //  totalWins = MyVersusMatchResults.PlayerTotalWinsByColors
      //};
      MyVersusMatchResults.winCounter.date = DateTime.Now.ToString("yyyy-MM-dd-HH");

      try
      {
        if (Settings.useOnlineStat)
        {
          //ApiStat.PostStat(Settings.getTeamName(), today, JsonConvert.SerializeObject(data, Formatting.Indented));
          ApiStat.PostStat(Settings.getTeamName(), today, JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented));
          Logger.Info("Fichier stat online sauvegardé : " + fileName);
          //return; //always save online AND local
        }
        string json = JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented);
        File.WriteAllText(fileName, json);
        Logger.Info("Fichier stat local sauvegardé : " + fileName);
      }
      catch (Exception ex)
      {
        Logger.Info("Erreur lors de la sauvegarde du fichier : " + ex.Message);
      }
    }

    //public static async Task SaveCurrentResultAsync()
    //{
    //  //string today = DateTime.Now.ToString("yyyy-MM-dd");
    //  //string fileName = today + "-" + getFileSuffix() + "-wincounters.json";

    //  //MyVersusMatchResults.winCounter.date = DateTime.Now.ToString("yyyy-MM-dd-HH");

    //  //try
    //  //{
    //  //  if (Settings.useOnlineStat)
    //  //  {
    //  //    string jsonData = JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented);
    //  //    await ApiStat.PostStatAsync(Settings.getTeamName(), today, jsonData); // Implement async version
    //  //    Logger.Info("Fichier stat online sauvegardé : " + fileName);
    //  //  }

    //    //string json = JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented);
    //    //await File.WriteAllTextAsync(fileName, json); // Available from .NET 4.6+, otherwise use StreamWriter
    //    //Logger.Info("Fichier stat local sauvegardé : " + fileName);
    //    Task.Factory.StartNew(() => TFModFortRiseWinCountersModule.SaveCurrentResult());
    //    //Task.Factory.StartNew(() =>
    //    //{
    //    //  try
    //    //  {
    //    //    string json = JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented);
    //    //    File.WriteAllText(fileName, json);
    //    //    Logger.Info("Fichier stat local sauvegardé : " + fileName);
    //    //  }
    //    //  catch (Exception ex)
    //    //  {
    //    //    Logger.Info("Erreur lors de la sauvegarde du fichier : " + ex.Message);
    //    //  }
    //    //});
    //  //}
    //  //catch (Exception ex)
    //  //{
    //  //  Logger.Info("Erreur lors de la sauvegarde du fichier : " + ex.Message);
    //  //}
    //}

    public static void loadPreviousResultIfExists()
    {
      Logger.Info("loadPreviousResultIfExists");
      //MyVersusMatchResults.PlayerWinsByColors.Clear();
      //MyVersusMatchResults.PlayerTotalWinsByColors.Clear();

      MyVersusMatchResults.winCounter.clear();

      string today = DateTime.Now.ToString("yyyy-MM-dd");
      //ONLINE STAT
      if (Settings.useOnlineStat) {
        APIStat.Sheet sheet = ApiStat.GetStat(Settings.getTeamName(), today);
        if (sheet.error != null)
        {
          Logger.Info("error GET,use 0 for each counter");
          return;
        }
        
        Logger.Info("date sheet = " + sheet.date + " date today = " + today);

        //WinCounterData data = JsonConvert.DeserializeObject<WinCounterData>(sheet.value);
        MyVersusMatchResults.winCounter = JsonConvert.DeserializeObject<WinCounterData>(sheet.value);

        if (today.Equals(sheet.date) || //problem if playing 2 following days
            //if we pass midnigth
            (
            // if date of saving data > 20h yesterday and date < today 8h
            string.Compare(MyVersusMatchResults.winCounter.date,DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd-20")) > 0
            &&
            string.Compare(MyVersusMatchResults.winCounter.date,DateTime.Now.ToString("yyyy-MM-dd-20")) < 0
            )
          )
        {
          Logger.Info("same date, ok nothing to do");
          //TFModFortRiseWinCountersModule.LoadFromString(sheet.value, false);
        }
        else {
          Logger.Info("not same date");
          MyVersusMatchResults.winCounter.resetToday();
          //TFModFortRiseWinCountersModule.LoadFromString(sheet.value, true);
        }
        return;
      }

      //LOCAL STAT
      string todayFile = today + "-" + getFileSuffix() + "-wincounters.json";

      // if not exists all counter are set to 0
      if (File.Exists(todayFile))
      {
        TFModFortRiseWinCountersModule.LoadFromFile(todayFile, false);
        return;
      }

      //load totalWins from last file found
      var files = Directory
          .EnumerateFiles(Directory.GetCurrentDirectory(), "*-" + getFileSuffix() + "-wincounters.json")
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
