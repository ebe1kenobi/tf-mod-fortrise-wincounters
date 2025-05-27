using System;
using Microsoft.Xna.Framework;

namespace TFModFortRiseWinCounters
{
  internal class WinCounterData
  {
    public String version = "v1";
    public String date { get; set; }

    public const string pinkColor = "{R:248 G:120 B:248 A:255}";
    public const string greenColor = "{R:0 G:184 B:0 A:255}";
    public const string orangeColor = "{R:239 G:140 B:33 A:255}";
    public const string blueColor = "{R:60 G:159 B:252 A:255}";
    public const string redColor = "{R:211 G:0 B:0 A:255}";
    public const string yellowColor = "{R:242 G:255 B:0 A:255}";
    public const string purpleColor = "{R:122 G:66 B:255 A:255}";
    public const string blue2Color = "{R:0 G:255 B:246 A:255}";
    public const string whiteColor = "{R:229 G:229 B:229 A:255}";

    public int pinkTodayWins = 0;
    public int greenTodayWins = 0;
    public int orangeTodayWins = 0;
    public int blueTodayWins = 0;
    public int redTodayWins = 0;
    public int yellowTodayWins = 0;
    public int purpleTodayWins = 0;
    public int blue2TodayWins = 0;
    public int whiteTodayWins = 0;

    public int pinkTotalWins = 0;
    public int greenTotalWins = 0;
    public int orangeTotalWins = 0;
    public int blueTotalWins = 0;
    public int redTotalWins = 0;
    public int yellowTotalWins = 0;
    public int purpleTotalWins = 0;
    public int blue2TotalWins = 0;
    public int whiteTotalWins = 0;
    public void resetToday()
    {
      pinkTodayWins = 0;
      greenTodayWins = 0;
      orangeTodayWins = 0;
      blueTodayWins = 0;
      redTodayWins = 0;
      yellowTodayWins = 0;
      purpleTodayWins = 0;
      blue2TodayWins = 0;
      whiteTodayWins = 0;
    }

    public void clear() {
      pinkTodayWins = 0;
      greenTodayWins = 0;
      orangeTodayWins = 0;
      blueTodayWins = 0;
      redTodayWins = 0;
      yellowTodayWins = 0;
      purpleTodayWins = 0;
      blue2TodayWins = 0;
      whiteTodayWins = 0;

      pinkTotalWins = 0;
      greenTotalWins = 0;
      orangeTotalWins = 0;
      blueTotalWins = 0;
      redTotalWins = 0;
      yellowTotalWins = 0;
      purpleTotalWins = 0;
      blue2TotalWins = 0;
      whiteTotalWins = 0;
    }

    public int getTodayWin(Color color) {
      switch (color.ToString())
      {
        case pinkColor:
          return pinkTodayWins;
        case greenColor:
          return greenTodayWins;
        case orangeColor:
          return orangeTodayWins;
        case blueColor:
          return blueTodayWins;
        case redColor:
          return redTodayWins;
        case yellowColor:
          return yellowTodayWins;
        case purpleColor:
          return purpleTodayWins;
        case blue2Color:
          return blue2TodayWins;
        case whiteColor:
          return whiteTodayWins;
      }
      return 0;
    }

    public int getTotalWin(Color color)
    {
      switch (color.ToString())
      {
        case pinkColor:
          return pinkTotalWins;
        case greenColor:
          return greenTotalWins;
        case orangeColor:
          return orangeTotalWins;
        case blueColor:
          return blueTotalWins;
        case redColor:
          return redTotalWins;
        case yellowColor:
          return yellowTotalWins;
        case purpleColor:
          return purpleTotalWins;
        case blue2Color:
          return blue2TotalWins;
        case whiteColor:
          return whiteTotalWins;
      }
      return 0;
    }

    public void increment(Color color)
    {
      switch (color.ToString())
      {
        case pinkColor:
          pinkTodayWins++;
          pinkTotalWins++;
          break;
        case greenColor:
          greenTodayWins++;
          greenTotalWins++;
          break;
        case orangeColor:
          orangeTodayWins++;
          orangeTotalWins++;
          break;
        case blueColor:
          blueTodayWins++;
          blueTotalWins++;
          break;
        case redColor:
          redTodayWins++;
          redTotalWins++;
          break;
        case yellowColor:
          yellowTodayWins++;
          yellowTotalWins++;
          break;
        case purpleColor:
          purpleTodayWins++;
          purpleTotalWins++;
          break;
        case blue2Color:
          blue2TodayWins++;
          blue2TotalWins++;
          break;
        case whiteColor:
          whiteTodayWins++;
          whiteTotalWins++;
          break;
      }
    }
  }
}
