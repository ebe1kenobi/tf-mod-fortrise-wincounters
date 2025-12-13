using System;
using System.Collections.Generic;
using System.Linq;
namespace TFModFortRiseWinCounters
{
  public class PlayerStatData
  {
    public int win { get; set; } = 0;
    public int kill { get; set; } = 0;
    public int death { get; set; } = 0;
    public int self { get; set; } = 0;
    public Dictionary<String, int> killFrom { get; set; } = new Dictionary<String, int>();
    public Dictionary<String, int> killBy { get; set; } = new Dictionary<String, int>();
    public PlayerStatData() {
    }
  }
}
