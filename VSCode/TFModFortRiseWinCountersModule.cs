using System;
using System.Collections.Generic;
//using TFModFortRiseLoaderAI;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;
using Newtonsoft.Json;
//using IL.TowerFall;
using TowerFall;

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
      if (!Debugger.IsAttached)
      {
        //Debugger.Launch(); // Proposera d’attacher Visual Studio
      }
      Instance = this;
      //Logger.Init("ModWinCounters");
      ApiStat = new APIStat(".\\Mods\\tf-mod-fortrise-wincounters\\settings.json");
    }

    public override void LoadContent()
    {
    }

    public override void Load()
    {
      MyMainMenu.Load();
      MyVersusPlayerMatchResults.Load();
      MyVersusMatchResults.Load();
      MyPlayerIndicator.Load();
      MyVersusRoundResults.Load();

      typeof(CustomNameImport).ModInterop();
    }

    public override void Unload()
    {
      MyMainMenu.Unload();
      MyVersusPlayerMatchResults.Unload();
      MyVersusMatchResults.Unload();
      MyPlayerIndicator.Unload();
      MyVersusRoundResults.Unload();
    }

    public static void LoadFromFile(string filePath, bool loadOnlyTotal)
    {
      try
      {
        string json = File.ReadAllText(filePath);
        MyVersusMatchResults.winCounter = JsonConvert.DeserializeObject<WinCounterData>(json);
        if (MyVersusMatchResults.winCounter != null) {
          if (loadOnlyTotal) {
            MyVersusMatchResults.winCounter.resetToday();
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    public static String getTeamName() {
      String TeamName = "";
      List<String> playerNames = new List<String>();
      for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++)
      {
        if (TFGame.Players[playerIndex])
        {
          playerNames.Add(CustomNameImport.GetPlayerName(playerIndex));
        }
      }
      playerNames.Sort();
      TeamName = String.Join("-", playerNames);
      return TeamName;
    }
    public static string getFileSuffix() {

      return getTeamName();

    }

    public static void SaveCurrentResult()
    {
      string today = DateTime.Now.ToString("yyyy-MM-dd");
      string fileName = today + "-" + getFileSuffix() + "-wincounters.json";

      MyVersusMatchResults.winCounter.date = DateTime.Now.ToString("yyyy-MM-dd-HH");

      try
      {
        if (Settings.useOnlineStat)
        {
          ApiStat.PostStat(getTeamName(), today, JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented));
          //return; //always save online AND local
        }
        string json = JsonConvert.SerializeObject(MyVersusMatchResults.winCounter, Formatting.Indented);
        File.WriteAllText(fileName, json);
      }
      catch (Exception ex)
      {
      }
    }

    public static void loadPreviousResultIfExists()
    {
      MyVersusMatchResults.winCounter.clear();

      string today = DateTime.Now.ToString("yyyy-MM-dd");
      //ONLINE STAT
      if (Settings.useOnlineStat) {
        APIStat.Sheet sheet = ApiStat.GetStat(getTeamName(), today);
        if (sheet.error != null)
        {
          return;
        }
        
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
        }
        else {
          MyVersusMatchResults.winCounter.resetToday();
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
