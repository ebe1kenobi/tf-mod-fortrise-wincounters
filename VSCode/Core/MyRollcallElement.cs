using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using TowerFall;


namespace TFModFortRiseWinCounters
{
  public class MyRollcallElement
  {
    public static Dictionary<int, Text> playerName = new Dictionary<int, Text>(8);
    public static List<string> playerNamesAvailable = new List<string>();

    internal static void Load()
    {
      On.TowerFall.RollcallElement.ctor += ctor_patch;
      On.TowerFall.RollcallElement.NotJoinedUpdate += NotJoinedUpdate_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.RollcallElement.ctor -= ctor_patch;
      On.TowerFall.RollcallElement.NotJoinedUpdate -= NotJoinedUpdate_patch;
    }

    public MyRollcallElement() { }

    public static void ctor_patch(On.TowerFall.RollcallElement.orig_ctor orig, global::TowerFall.RollcallElement self, int playerIndex)
    {
      orig(self, playerIndex);
      var dynData = DynamicData.For(self);

      Color color = Color.White;
      Vector2 positionText;
      if (TFGame.Players.Length > 4)
      {
        positionText = new Vector2(-10, -60);
        positionText = new Vector2(0, 0);
      }
      else
      {
        positionText = new Vector2(-30, -60);
      }

      //to do once for the game
      if (!playerName.ContainsKey(playerIndex)) {
        String name = playerNamesAvailable[0] + (playerIndex + 1);
        playerName[playerIndex] = new Text(TFGame.Font, name, positionText, color, Text.HorizontalAlign.Left, Text.VerticalAlign.Bottom);
      }

      self.Add((Component)playerName[playerIndex]);

      dynData.Dispose();
    }

    public static void SetPlayerName(int playerIndex, String newName)
    {
      var dynData = DynamicData.For(playerName[playerIndex]);
      dynData.Set("text", newName);
      dynData.Dispose();
    }

    public static string getNextName(int playerIndex)
    {
      int currentNameIndex = getCurrentNameIndex(playerIndex);
      string nextName = "";

      // Construire la liste des noms déjà utilisés
      var usedNames = new HashSet<string>();
      foreach (var kvp in playerName)
      {
        if (kvp.Value != null)
        {
          var dynData = DynamicData.For(kvp.Value);
          string txt = (string)dynData.Get("text");
          if (!string.IsNullOrEmpty(txt))
            usedNames.Add(txt);
          dynData.Dispose();
        }
      }

      // Rechercher le prochain nom disponible
      int totalNames = playerNamesAvailable.Count;
      for (int i = 1; i <= totalNames; i++)
      {
        int nextIndex = (currentNameIndex + i) % totalNames;
        string candidate = playerNamesAvailable[nextIndex];

        if (!usedNames.Contains(candidate))
        {
          nextName = candidate;
          break;
        }
      }

      // Si rien trouvé (très peu probable), on prend le premier nom
      if (string.IsNullOrEmpty(nextName))
        nextName = playerNamesAvailable[0];

      // Si c’est le premier nom ("P" par exemple), on ajoute le numéro du joueur
      if (nextName == playerNamesAvailable[0])
      {
        return nextName + (playerIndex + 1);
      }

      return nextName;
    }

    static public int getCurrentNameIndex(int playerIndex)
    {
      var dynData = DynamicData.For(playerName[playerIndex]);
      String currentName = (String)dynData.Get("text");
      int index = 0;

      // Si le nom commence par 'P' et a une longueur de 2 → on renvoie 0
      if (!string.IsNullOrEmpty(currentName) &&
          currentName.Length == 2 &&
          currentName.StartsWith("P"))
      {
        index = 0;
      }
      else
      {
        // Sinon, on cherche l'indice dans la liste des noms disponibles
        index = playerNamesAvailable.IndexOf(currentName);

        // Si le nom n’existe pas dans la liste, on renvoie 0 par défaut
        if (index < 0)
          index = 0;
      }

      dynData.Dispose();
      return index;
    }

    public static int NotJoinedUpdate_patch(On.TowerFall.RollcallElement.orig_NotJoinedUpdate orig, global::TowerFall.RollcallElement self)
    {
      var dynData = DynamicData.For(self);

      int playerIndex = (int)dynData.Get("playerIndex");

      if (dynData.Get("input") == null)
        return orig(self);

      var input = DynamicData.For(dynData.Get("input"));
      if (input == null)
        return orig(self);
      //move to next name
      if ((bool)input.Get("MenuAlt2")){
        SetPlayerName(playerIndex, getNextName(playerIndex));
      }
      dynData.Dispose();

      return orig(self);
    }
  }
}
