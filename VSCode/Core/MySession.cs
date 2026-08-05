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
      // Dernier moment ou le mode de jeu est encore modifiable : voir
      // TFModFortRiseWinCountersModule.ReloadIfKeyChanged.
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Session), nameof(Session.StartGame)),
          prefix: new HarmonyMethod(StartGame_prefix)
      );
    }

    public static void StartGame_prefix(Session __instance)
    {
      TFModFortRiseWinCountersModule.ReloadIfKeyChanged(__instance);
    }

    public static void OnPlayerDeath_patch(Session __instance, Player player, PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex) {
      MyVersusMatchResults.winCounter.addStat(playerIndex, deathType, killerIndex);
    }
  }
}
