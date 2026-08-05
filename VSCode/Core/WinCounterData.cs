using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  internal class WinCounterData
  {
    /// <summary>
    /// Version du format de donnees. Etait declaree const en FortRise 5, donc
    /// absente du JSON : System.Text.Json ne serialise que les proprietes
    /// publiques, ni les constantes ni les champs. C'est une propriete pour que
    /// la version reparaisse dans les fichiers, comme en FortRise 4.
    ///
    /// v4 = ajout de "mode" et de "matchsResults".
    /// </summary>
    public String version { get; set; } = "v4";

    public String date { get; set; }

    /// <summary>
    /// Mode de jeu de la session, sous forme lisible (voir
    /// TFModFortRiseWinCountersModule.getModeName). Renseigne a la sauvegarde.
    /// </summary>
    public String mode { get; set; }

    /// <summary>
    /// Score final de chaque match joue, dans l'ordre : un dictionnaire
    /// joueur -> score par match.
    /// </summary>
    public List<Dictionary<string, int>> matchsResults { get; set; } = new List<Dictionary<string, int>>();

    public Dictionary<String, int> todayWin { get; set; } = new Dictionary<String, int>();
    public Dictionary<String, int> totalWin { get; set; } = new Dictionary<String, int>();

    public Dictionary<String, PlayerStatData> today { get; set; } = new Dictionary<String, PlayerStatData>();
    public Dictionary<String, PlayerStatData> total { get; set; } = new Dictionary<String, PlayerStatData>();

    public WinCounterData() {
    }
    public void resetToday()
    {
      todayWin.Clear();
      today.Clear();
      // matchsResults ne retient que les matchs du jour : sans ce Clear la liste
      // grossissait indefiniment, a contre-courant des autres compteurs "today".
      matchsResults.Clear();
    }

    public void clear() {
      todayWin.Clear();
      totalWin.Clear();

      today.Clear();
      total.Clear();
      matchsResults.Clear();
    }

    public int getTodayWin(String name) {
      if (!today.ContainsKey(name))
      {
        today[name] = new PlayerStatData();
      }
      return today[name].win;
    }

    public int getTotalWin(String name)
    {
      if (!total.ContainsKey(name))
      {
        total[name] = new PlayerStatData();
      }
      return total[name].win;
    }

    public void increment(String name)
    {
      if (!todayWin.ContainsKey(name))
      {
        todayWin[name] = 0;
      }

      if (!totalWin.ContainsKey(name))
      {
        totalWin[name] = 0;
      }

      todayWin[name]++;
      totalWin[name]++;
    }

    public void addWinner(String name)
    {
      increment(name);
      if (!today.ContainsKey(name))
      {
        today[name] = new PlayerStatData();
      }
      if (!total.ContainsKey(name))
      {
        total[name] = new PlayerStatData();
      }
      today[name].win++;
      total[name].win++;
    }

    /// <summary>
    /// Enregistre le score final d'un match : un dictionnaire joueur -> score,
    /// ajoute a la suite de matchsResults.
    ///
    /// Au passage, les joueurs du match sont declares dans todayWin/totalWin meme
    /// s'ils n'ont rien gagne, pour qu'ils apparaissent dans les compteurs avec
    /// zero plutot que d'en etre absents.
    /// </summary>
    public void addMatchResult(Session session)
    {
      if (session == null || session.Scores == null) return;

      Dictionary<String, int> matchResult = new Dictionary<String, int>();
      for (int i = 0; i < session.Scores.Length && i < TFGame.Players.Length; i++)
      {
        if (!TFGame.Players[i]) continue;

        string playerName = CustomNameImport.GetPlayerName(i);
        matchResult[playerName] = session.Scores[i];

        if (!todayWin.ContainsKey(playerName))
          todayWin[playerName] = 0;

        if (!totalWin.ContainsKey(playerName))
          totalWin[playerName] = 0;
      }

      matchsResults.Add(matchResult);
    }

    public void addStat(int playerIndex, DeathCause deathType, int killerIndex) {
      //look in this.RoundLogic.OnPlayerDeath
      //and Player.die
      string playerKilled = CustomNameImport.GetPlayerName(playerIndex);
      string killerPlayer = "";

      if (!today.ContainsKey(playerKilled))
      {
        today[playerKilled] = new PlayerStatData();
      }
      if (!total.ContainsKey(playerKilled))
      {
        total[playerKilled] = new PlayerStatData();
      }

      if (killerIndex > -1){
        killerPlayer = CustomNameImport.GetPlayerName(killerIndex);

        if (!today.ContainsKey(killerPlayer))
        {
          today[killerPlayer] = new PlayerStatData();
        }
        if (!total.ContainsKey(killerPlayer))
        {
          total[killerPlayer] = new PlayerStatData();
        }

        today[killerPlayer].kill++;
        total[killerPlayer].kill++;

      }

      today[playerKilled].death++;
      total[playerKilled].death++;


      if (playerIndex == killerIndex) {
        today[playerKilled].self++;
        total[playerKilled].self++;
      }
      if (killerPlayer != "") {
        if (today[playerKilled].killBy.ContainsKey(killerPlayer) == false) {
          today[playerKilled].killBy[killerPlayer] = 0;
        }
        if (total[playerKilled].killBy.ContainsKey(killerPlayer) == false)
        {
          total[playerKilled].killBy[killerPlayer] = 0;
        }
        today[playerKilled].killBy[killerPlayer]++;
        total[playerKilled].killBy[killerPlayer]++;
      }

      String deathCause = deathType.ToString();

      if (today[playerKilled].killFrom.ContainsKey(deathCause) == false)
      {
        today[playerKilled].killFrom[deathCause] = 0;
      }
      if (total[playerKilled].killFrom.ContainsKey(deathCause) == false)
      {
        total[playerKilled].killFrom[deathCause] = 0;
      }

      today[playerKilled].killFrom[deathCause]++;
      total[playerKilled].killFrom[deathCause]++;
    }
  }
}
