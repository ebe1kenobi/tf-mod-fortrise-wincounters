using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  public class MySession : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Session), nameof(Session.OnPlayerDeath)),
          postfix: new HarmonyMethod(OnPlayerDeath_patch)
      );
    }

    public static void OnPlayerDeath_patch(Session __instance, Player player, PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex) {
      MyVersusMatchResults.winCounter.addStat(playerIndex, deathType, killerIndex);
    }
  }
}
