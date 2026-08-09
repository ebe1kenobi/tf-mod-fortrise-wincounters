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
//using Newtonsoft.Json;
//using IL.TowerFall;
using TowerFall;
using static TFModFortRiseWinCounters.APIStat;

namespace TFModFortRiseWinCounters
{
  public class TFModFortRiseWinCountersModule : Mod
  {
    public static TFModFortRiseWinCountersModule Instance;
    // settings.json est livre avec le mod : lu via IModContent (voir APIStat).
    public const string SettingsFileName = "settings.json";

    // Les fichiers de stats *-wincounters.json sont des DONNEES generees : elles
    // vont dans l'espace de sauvegarde du mod. FortRise 4 les ecrivait dans le
    // repertoire courant, c'est-a-dire le dossier du jeu.
    public static string SavePath => Path.Combine(ModIO.GetRootPath(), "Saves", Instance.Meta.Name);

    internal Type[] Hookables = [
        typeof(MyPlayerIndicator),
        typeof(MySession),
        typeof(MyStatWarning),
        typeof(MyVersusMatchResults),
        typeof(MyVersusPlayerMatchResults),
    ];

    //public override Type SettingsType => typeof(TFModFortRiseWinCountersSettings);
    //public static TFModFortRiseWinCountersSettings Settings => (TFModFortRiseWinCountersSettings)Instance.InternalSettings;
    public static TFModFortRiseWinCountersSettings Settings => Instance.GetSettings<TFModFortRiseWinCountersSettings>()!;

    public static APIStat ApiStat;

    // Leve quand les stats en ligne n'ont pas pu etre chargees : les compteurs
    // affiches repartent alors de zero au lieu de refleter l'historique du jour.
    // MyStatWarning en fait un bandeau en bas de l'ecran, affiche quelques
    // secondes. Passer a true relance ce compte a rebours meme si le drapeau
    // etait deja leve, pour qu'un second echec dans la soiree previenne a nouveau.
    private static bool statsUnavailable;
    public static bool StatsUnavailable
    {
      get { return statsUnavailable; }
      set
      {
        if (value) MyStatWarning.ResetDisplayTimer();
        statsUnavailable = value;
      }
    }
    //public static bool ReloadNecessary = false;

    public TFModFortRiseWinCountersModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
      if (!Debugger.IsAttached)
      {
        //Debugger.Launch(); // Proposera dâ€™attacher Visual Studio
      }
      System.Net.ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Tls12;
      Instance = this;
      // Les logs vont dans l'espace de sauvegarde du mod. Le chemin relatif
      // precedent les ecrivait dans un dossier ModWinCounters cree a la racine
      // du repertoire d'installation de TowerFall.
      TFModFortRiseWinCounters.Logger.Init(Meta.Name);
      ApiStat = new APIStat(content, SettingsFileName);

      // Les noms de joueurs viennent du mod Profiles, qui a repris ce role a
      // CustomName. L'interop de FortRise construit son proxy sur la forme des
      // membres : il suffit que IProfilesModApi decrive ce que Profiles expose.
      ProfilesImport.Api = context.Interop.GetApi<IProfilesModApi>("Ebe1.Profiles");
      if (ProfilesImport.Api == null)
        TFModFortRiseWinCounters.Logger.Info("[Profiles] mod absent : repli sur les noms P1..P8");

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
          moveResultForV4Format();

        }
      }
      catch (Exception ex)
      {
        ;
      }
    }

    /// <summary>
    /// Nom lisible du mode de jeu courant.
    ///
    /// MatchSettings.Mode est un enum, mais un mode ajoute par un mod (Respawn,
    /// PlayTag...) recoit une valeur au-dela de celles declarees : ToString() rend
    /// alors le nombre brut ("12") au lieu d'un nom. FortRise range le vrai nom
    /// dans CustomVersusModeName des que IsCustom est vrai, c'est donc lui qu'on
    /// prend en premier.
    /// </summary>
    public static String getModeName()
    {
      var settings = MainMenu.VersusMatchSettings;
      if (settings == null)
        return "UNKNOWN";

      if (settings.IsCustom && !String.IsNullOrEmpty(settings.CustomVersusModeName))
        return settings.CustomVersusModeName;

      // Filet pour une valeur hors enum sans IsCustom : mieux vaut un libelle
      // reconnaissable qu'un nombre nu au milieu d'un nom de fichier.
      if (!Enum.IsDefined(typeof(Modes), settings.Mode))
        return "MODE" + (int)settings.Mode;

      return settings.Mode.ToString();
    }

    public static String getTeamName() {
      String TeamName = "";
      List<String> playerNames = new List<String>();
      for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++)
      {
        if (TFGame.Players[playerIndex])
        {
          playerNames.Add(ProfilesImport.GetPlayerName(playerIndex));
        }
      }
      playerNames.Sort();
      // Le mode ouvre le nom d'equipe : il sert de cle au fichier local comme a
      // l'enregistrement en ligne, et separe donc les stats par mode de jeu.
      playerNames.Insert(0, getModeName());
      TeamName = String.Join("-", playerNames);
      return TeamName;
    }
    public static string getFileSuffix() {

      return getTeamName();

    }

    /// <summary>
    /// Chemin complet d'un fichier de stats, dans l'espace de sauvegarde du mod.
    /// Le repertoire est cree au besoin.
    /// </summary>
    public static string GetStatFilePath(string fileName)
    {
      string folder = SavePath;
      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

      return Path.Combine(folder, fileName);
    }

    public static void SaveCurrentResult()
    {
      //TFModFortRiseWinCounters.Logger.Info($"SaveCurrentResult");
      string today = DateTime.Now.ToString("yyyy-MM-dd");
      string fileName = GetStatFilePath(today + "-" + getFileSuffix() + "-wincounters.json");

      MyVersusMatchResults.winCounter.date = DateTime.Now.ToString("yyyy-MM-dd-HH");
      MyVersusMatchResults.winCounter.mode = getModeName();

      try
      {
        var data = JsonSerializer.Serialize(MyVersusMatchResults.winCounter, new JsonSerializerOptions
        {
          WriteIndented = true
        });
        if (Settings.useOnlineStat)
        {
          // L'envoi en ligne ne doit pas empecher la sauvegarde locale : avant,
          // une exception ici sautait directement au catch et le fichier du jour
          // n'etait jamais ecrit.
          if (!ApiStat.PostStat(getTeamName(), today, data))
            StatsUnavailable = true;
        }
        //string json = data;
        File.WriteAllText(fileName, data);
      }
      catch (Exception ex)
      {
        TFModFortRiseWinCounters.Logger.Info($"[WinCounters] sauvegarde impossible : {ex.Message}");
      }
    }

    public static void initPlayerData() {
      //set all player name 
      //TFModFortRiseWinCounters.Logger.Info($"initPlayerData");
      for (int i = 0; i < TFGame.Players.Length; i++)
      {

        if (TFGame.Players[i])
        {
          string playerName = ProfilesImport.GetPlayerName(i);
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
    // Cle (mode + joueurs) pour laquelle les compteurs en memoire ont ete charges.
    // Sert a detecter un changement de mode entre le rollcall et le lancement.
    private static string loadedTeamName;

    /// <summary>
    /// Charge les compteurs du match qui demarre, si la cle a change.
    ///
    /// C'est le seul point de chargement. Le declencher au rollcall ne marchait
    /// pas : le mode de jeu se choisit APRES, RollcallElement.StartVersus basculant
    /// vers MenuState.VersusOptions, l'ecran du bouton de mode. Les compteurs
    /// etaient donc charges pour le mode precedent, puis reenregistres sous la cle
    /// du nouveau mode a la fin du match â€” les stats d'un mode se retrouvaient
    /// recopiees dans un autre.
    ///
    /// Session.StartGame est le dernier moment ou le mode est encore modifiable,
    /// et n'est appele qu'une fois par match. Le test sur la cle evite de recharger
    /// quand rien n'a change.
    /// </summary>
    public static void ReloadIfKeyChanged(Session session)
    {
      if (!Settings.enable) return;
      if (session == null || session.MatchSettings == null) return;
      if (!IsVersusMatch(session.MatchSettings)) return;

      string current = getTeamName();
      if (current == loadedTeamName) return;

      TFModFortRiseWinCounters.Logger.Info(
          $"[WinCounters] chargement des compteurs pour '{current}'"
          + (loadedTeamName != null ? $" (precedent : '{loadedTeamName}')" : ""));
      loadPreviousResultIfExists();
    }

    /// <summary>
    /// Le mod ne compte que les matchs versus : inutile d'aller interroger le
    /// serveur en Quest, Dark World ou Trials.
    ///
    /// On lit le mode de la session plutot que MainMenu.RollcallMode : cette
    /// statique n'est posee que par les boutons du menu principal, donc un match
    /// lance autrement â€” par le mod Tournament par exemple â€” garderait la valeur
    /// du mode joue precedemment.
    /// </summary>
    private static bool IsVersusMatch(MatchSettings settings)
    {
      if (settings.IsCustom) return true;   // les modes ajoutes par un mod sont des modes versus

      Modes mode = settings.Mode;
      return mode == Modes.LastManStanding
          || mode == Modes.HeadHunters
          || mode == Modes.TeamDeathmatch
          || mode == Modes.Warlord;
    }

    public static void loadPreviousResultIfExists()
    {
      //TFModFortRiseWinCounters.Logger.Info($"loadPreviousResultIfExists");
      loadedTeamName = getTeamName();
      MyVersusMatchResults.winCounter.clear();

      string today = DateTime.Now.ToString("yyyy-MM-dd");
      //ONLINE STAT
      if (Settings.useOnlineStat) {
        APIStat.Sheet sheet = ApiStat.GetStat(getTeamName(), today);

        // sheet == null : serveur injoignable ou reponse inexploitable (GetStat a
        // deja journalise la cause). C'est le SEUL cas qui merite d'alerter le
        // joueur : les scores affiches ne refletent alors pas l'historique.
        if (sheet == null)
        {
          TFModFortRiseWinCounters.Logger.Info("[WinCounters] statistiques en ligne NON chargees, compteurs remis a zero");
          StatsUnavailable = true;
          initPlayerData();
          return;
        }

        // Reponse valide mais vide : l'appli web repond error/value nul quand
        // l'identifiant n'a encore rien d'enregistre. C'est le cas normal d'une
        // premiere partie avec cette equipe â€” ou, depuis que le mode fait partie
        // de l'identifiant, d'un mode joue pour la premiere fois. Ce n'est pas une
        // panne, donc pas de bandeau d'alerte.
        if (sheet.error != null || sheet.value == null)
        {
          if (sheet.error != null)
            TFModFortRiseWinCounters.Logger.Info($"[WinCounters] rien d'enregistre pour '{getTeamName()}' : {sheet.error}");
          StatsUnavailable = false;
          initPlayerData();
          return;
        }

        StatsUnavailable = false;

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
        // v4 : mode + matchsResults, absents des enregistrements plus anciens.
        moveResultForV4Format();
        initPlayerData();
        
        return;
      }

      //LOCAL STAT
      // GetStatFilePath cree le repertoire au besoin : l'EnumerateFiles plus bas ne
      // peut donc pas lever sur un dossier inexistant.
      string todayFile = GetStatFilePath(today + "-" + getFileSuffix() + "-wincounters.json");

      // if not exists all counter are set to 0
      if (File.Exists(todayFile))
      {
        TFModFortRiseWinCountersModule.LoadFromFile(todayFile, false);
        return;
      }

      //load totalWins from last file found
      var files = Directory
          .EnumerateFiles(SavePath, "*-" + getFileSuffix() + "-wincounters.json")
          .Select(path => new
          {
            Path = path,
            // On essaie de parser les 10 premiers caractÃ¨res en date
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
        // On prend le plus rÃ©cent
        string lastFile = files.First().Path;
        TFModFortRiseWinCountersModule.LoadFromFile(lastFile, true);
      }
    }

    /// <summary>
    /// Migration vers le format v4 : les fichiers plus anciens n'ont ni mode ni
    /// liste de resultats de match. Le mode est inconnu retroactivement, on le
    /// marque comme tel plutot que d'inventer une valeur.
    /// </summary>
    public static void moveResultForV4Format()
    {
      var counter = MyVersusMatchResults.winCounter;
      if (counter == null) return;

      if (counter.matchsResults == null)
        counter.matchsResults = new List<Dictionary<string, int>>();

      if (String.IsNullOrEmpty(counter.mode))
        counter.mode = "UNKNOWN";

      counter.version = "v4";
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
