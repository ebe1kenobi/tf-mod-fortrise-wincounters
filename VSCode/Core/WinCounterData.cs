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
    public const String version = "v3";
    public String date { get; set; }

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
    }

    public void clear() {
      todayWin.Clear();
      totalWin.Clear();

      today.Clear();
      total.Clear();
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
