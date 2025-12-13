using System;
using System.Collections.Generic;
//using TFModFortRiseLoaderAI;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using FortRise;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;
//using Newtonsoft.Json;
//using IL.TowerFall;
using TowerFall;
using static TFModFortRiseWinCounters.APIStat;

namespace TFModFortRiseWinCounters
{
  public class TFModFortRiseWinCountersModule : Mod
  {
    public static TFModFortRiseWinCountersModule Instance;
    public static string settingsFilePath = @".\FortRise\Mods\tf-mod-fortrise-wincounters\settings.json";

    internal Type[] Hookables = [
        typeof(MyPlayerIndicator),
        typeof(MyRollcallElement),
        typeof(MySession),
        typeof(MyVersusMatchResults),
        typeof(MyVersusPlayerMatchResults),
    ];

    //public override Type SettingsType => typeof(TFModFortRiseWinCountersSettings);
    //public static TFModFortRiseWinCountersSettings Settings => (TFModFortRiseWinCountersSettings)Instance.InternalSettings;
    public static TFModFortRiseWinCountersSettings Settings => Instance.GetSettings<TFModFortRiseWinCountersSettings>()!;

    public static APIStat ApiStat;
    //public static bool ReloadNecessary = false;

    public TFModFortRiseWinCountersModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
      if (!Debugger.IsAttached)
      {
        //Debugger.Launch(); // Proposera d’attacher Visual Studio
      }
      System.Net.ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Tls12;
      Instance = this;
      //TFModFortRiseWinCounters.Logger.Init("ModWinCounters");
      ApiStat = new APIStat(settingsFilePath);
      typeof(CustomNameImport).ModInterop();

      foreach (var hookable in Hookables)
      {
        hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
      }
    }

    public override ModuleSettings CreateSettings()
    {
      return new TFModFortRiseWinCountersSettings();
    }

    public static void LoadFromFile(string filePath, bool loadOnlyTotal)
    {
      try
      {
        string json = File.ReadAllText(filePath);
        //MyVersusMatchResults.winCounter = JsonConvert.DeserializeObject<WinCounterData>(json);
        MyVersusMatchResults.winCounter =  JsonSerializer.Deserialize<WinCounterData>(json);

        if (MyVersusMatchResults.winCounter != null) {
          if (loadOnlyTotal) {
            MyVersusMatchResults.winCounter.resetToday();
          }
          moveResultForV3Format();

        }
      }
      catch (Exception ex)
      {
        ;
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
      //TFModFortRiseWinCounters.Logger.Info($"SaveCurrentResult");
      string today = DateTime.Now.ToString("yyyy-MM-dd");
      string fileName = today + "-" + getFileSuffix() + "-wincounters.json";

      MyVersusMatchResults.winCounter.date = DateTime.Now.ToString("yyyy-MM-dd-HH");

      try
      {
        var data = JsonSerializer.Serialize(MyVersusMatchResults.winCounter, new JsonSerializerOptions
        {
          WriteIndented = true
        });
        if (Settings.useOnlineStat)
        {
          ApiStat.PostStat(getTeamName(), today, data);
          //return; //always save online AND local
        }
        //string json = data;
        File.WriteAllText(fileName, data);
      }
      catch (Exception ex)
      {
      }
    }

    public static void initPlayerData() {
      //set all player name 
      //TFModFortRiseWinCounters.Logger.Info($"initPlayerData");
      for (int i = 0; i < TFGame.Players.Length; i++)
      {

        if (TFGame.Players[i])
        {
          string playerName = CustomNameImport.GetPlayerName(i);
          //TFModFortRiseWinCounters.Logger.Info($"playerName {playerName}");
          if (!MyVersusMatchResults.winCounter.total.ContainsKey(playerName))
          {
            //TFModFortRiseWinCounters.Logger.Info($"playerName {playerName}  create total[playerName]");
            MyVersusMatchResults.winCounter.total[playerName] = new PlayerStatData();
          }

          if (!MyVersusMatchResults.winCounter.today.ContainsKey(playerName))
          {
            //TFModFortRiseWinCounters.Logger.Info($"playerName {playerName}  create today[playerName]");
            MyVersusMatchResults.winCounter.today[playerName] = new PlayerStatData();
          }

          if (!MyVersusMatchResults.winCounter.todayWin.ContainsKey(playerName))
          {
            //TFModFortRiseWinCounters.Logger.Info($"playerName {playerName}  create todayWin[playerName]");
            MyVersusMatchResults.winCounter.todayWin[playerName] = 0;
          }

          if (!MyVersusMatchResults.winCounter.totalWin.ContainsKey(playerName))
          {
            //TFModFortRiseWinCounters.Logger.Info($"playerName {playerName}  create totalWin[playerName]");
            MyVersusMatchResults.winCounter.totalWin[playerName] = 0;
          }
        }
      }
    }
    public static void loadPreviousResultIfExists()
    {
      //TFModFortRiseWinCounters.Logger.Info($"loadPreviousResultIfExists");
      MyVersusMatchResults.winCounter.clear();

      string today = DateTime.Now.ToString("yyyy-MM-dd");
      //ONLINE STAT
      if (Settings.useOnlineStat) {
        APIStat.Sheet sheet = ApiStat.GetStat(getTeamName(), today);
        if (sheet.error != null)
        {
          initPlayerData();
          return;
        }
        
        //WinCounterData data = JsonConvert.DeserializeObject<WinCounterData>(sheet.value);
        //MyVersusMatchResults.winCounter = JsonConvert.DeserializeObject<WinCounterData>(sheet.value);
        MyVersusMatchResults.winCounter = JsonSerializer.Deserialize<WinCounterData>(sheet.value);

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
        // v3 : move totla result in new format
        moveResultForV3Format();
        initPlayerData();
        
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

    public static void moveResultForV3Format() {
      foreach (var name in MyVersusMatchResults.winCounter.totalWin) {
        if (MyVersusMatchResults.winCounter.totalWin.ContainsKey(name.Key)
              && MyVersusMatchResults.winCounter.totalWin[name.Key] > 0){
          if (!MyVersusMatchResults.winCounter.total.ContainsKey(name.Key))
          {
            MyVersusMatchResults.winCounter.total[name.Key] = new PlayerStatData();
            MyVersusMatchResults.winCounter.total[name.Key].win = MyVersusMatchResults.winCounter.totalWin[name.Key];
          } else {
            if (MyVersusMatchResults.winCounter.total[name.Key].win == 0) {
              MyVersusMatchResults.winCounter.total[name.Key].win = MyVersusMatchResults.winCounter.totalWin[name.Key];
            }
          }
        } 
      }
    }
  }
}
