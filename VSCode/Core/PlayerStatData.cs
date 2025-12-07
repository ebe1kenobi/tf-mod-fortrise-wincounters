using System;
using System.Collections.Generic;
using System.Linq;
namespace TFModFortRiseWinCounters
{
  public class PlayerStatData
  {
    public int win = 0;
    public int kill = 0;
    public int death = 0;
    public int self = 0;
    public Dictionary<String, int> killFrom = new Dictionary<String, int>();
    public Dictionary<String, int> killBy = new Dictionary<String, int>();
    public PlayerStatData() {
      //killFrom["Arrow"] = 0;
      //killFrom["Explosion"] = 0;
      //killFrom["Brambles"] = 0;
      //killFrom["JumpedOn"] = 0;
      //killFrom["Lava"] = 0;
      //killFrom["Shock"] = 0;
      //killFrom["SpikeBall"] = 0;
      //killFrom["FallingObject"] = 0;
      //killFrom["Squish"] = 0;
      //killFrom["Curse"] = 0;
      //killFrom["Miasma"] = 0;
      //killFrom["Enemy"] = 0;
      //killFrom["Chalice"] = 0;
      //killFrom["Other"] = 0;
    }
  }
}
