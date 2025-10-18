using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Monocle;
using MonoMod.Utils;
using FortRise;
using TowerFall;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
//using System.Drawing;

namespace TFModFortRiseWinCounters
{
  internal class MyTFGame
  {
    internal static void Load()
    {
      On.TowerFall.TFGame.Load += Load_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.TFGame.Load -= Load_patch;
    }

    private static void Load_patch(On.TowerFall.TFGame.orig_Load orig)
    {
      orig();



      TaskHelper.Run("LOAD CONFIG FILE WITH PLAYER NAME", () =>
      {
        try
        {
          string filePath = @".\Mods\tf-mod-fortrise-wincounters\playerName.json";

          if (!File.Exists(filePath))
          {
            MyRollcallElement.playerNamesAvailable = new List<string>();
            return;
          }

          string jsonContent = File.ReadAllText(filePath);

          // Désérialisation avec Newtonsoft.Json
          var names = JsonConvert.DeserializeObject<List<string>>(jsonContent);

          if (names == null)
          {
            MyRollcallElement.playerNamesAvailable = new List<string>();
          }
          else
          {
            MyRollcallElement.playerNamesAvailable = names;
            names.Insert(0, "P"); // Add default name which will be display like P1 P2.. P8
          }
        }
        catch (Exception ex)
        {
          TFGame.Log(ex, true);
          TFGame.OpenLog();
          Engine.Instance.Exit();
        }
      });
    }
  }
}