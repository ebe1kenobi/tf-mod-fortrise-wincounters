using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MySession
  {
    internal static void Load()
    {
      On.TowerFall.Session.OnPlayerDeath += OnPlayerDeath_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.Session.OnPlayerDeath -= OnPlayerDeath_patch;
    }
    public static void OnPlayerDeath_patch(On.TowerFall.Session.orig_OnPlayerDeath orig, global::TowerFall.Session self, global::TowerFall.Player player, global::TowerFall.PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex) {
      orig(self, player, corpse, playerIndex, deathType, position, killerIndex);
      MyVersusMatchResults.winCounter.addStat(playerIndex, deathType, killerIndex);
    }
  }
}
