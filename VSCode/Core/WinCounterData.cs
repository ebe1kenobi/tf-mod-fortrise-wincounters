using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace TFModFortRiseWinCounters
{
  internal class WinCounterData
  {
    public const String version = "v2";
    public String date { get; set; }

    public Dictionary<String, int> todayWin = new Dictionary<String, int>();
    public Dictionary<String, int> totalWin = new Dictionary<String, int>();
    public void resetToday()
    {
      todayWin.Clear();
    }

    public void clear() {
      todayWin.Clear();
      totalWin.Clear();
    }

    public int getTodayWin(String name) {
      if (!todayWin.ContainsKey(name)) {
        todayWin[name] = 0;
      }
      return todayWin[name];
    }

    public int getTotalWin(String name)
    {
      if (!totalWin.ContainsKey(name))
      {
        totalWin[name] = 0;
      }
      return totalWin[name];
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
  }
}
