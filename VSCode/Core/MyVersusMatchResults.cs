using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;
using FortRise;

//todo order name in popup by name
// decrease font size to fit more data

namespace TFModFortRiseWinCounters
{
  internal class MyVersusMatchResults : IHookable
  {
    public static WinCounterData winCounter = new WinCounterData();

    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredConstructor(typeof(VersusMatchResults), [
                                                                        typeof(Session),
                                                                        typeof(VersusRoundResults),
                                                                    ]),
          postfix: new HarmonyMethod(ctor_patch)
      );
    }

    public static void ctor_patch(VersusMatchResults __instance, global::TowerFall.Session session, global::TowerFall.VersusRoundResults roundResults)
    {
      //TFModFortRiseWinCounters.Logger.Info($"MyVersusMatchResults ctor_patch ");
      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      //if (TFModFortRiseWinCountersModule.ReloadNecessary) //TODO test !message not displayed
      //{
      //  TFModFortRiseWinCountersModule.loadPreviousResultIfExists();
      //  TFModFortRiseWinCountersModule.ReloadNecessary = false;
      //}

      if (TFModFortRiseWinCountersModule.Settings.resetTodayCounter)
      {
        TFModFortRiseWinCountersModule.Settings.resetTodayCounter = false;
        winCounter.resetToday();
      }

      for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++)
      {
        if (!TFGame.Players[playerIndex]) continue;

        if (session.MatchStats[playerIndex].Won)
        {
          //winCounter.increment(ProfilesImport.GetPlayerName(playerIndex));
          winCounter.addWinner(ProfilesImport.GetPlayerName(playerIndex));
        }
      }

      // Score final du match, conserve dans matchsResults (format v4).
      winCounter.addMatchResult(session);

      //need to save each time
      //TFModFortRiseWinCounters.Logger.Info($"MyVersusMatchResults StartNew ");

      Task.Factory.StartNew(() => TFModFortRiseWinCountersModule.SaveCurrentResult());

      // Ajouter un Entity pour surveiller les inputs du bouton Y
      // Utiliser un Alarm car self.Scene peut être null dans le constructeur
      Alarm.Set(__instance, 1, delegate
      {
        if (__instance.Scene != null)
        {
          StatsInputWatcher watcher = new StatsInputWatcher();
          __instance.Scene.Add(watcher);
        }
      }, Alarm.AlarmMode.Oneshot);
    }

    //public static int NotJoinedUpdate_patch(On.TowerFall.RollcallElement.orig_NotJoinedUpdate orig, global::TowerFall.RollcallElement self)
    //{
    //  if (VirtualKeyboard.KeyboardActive)
    //  {
    //    return 0; // ignore l’input, le Rollcall ne réagit pas
    //  }
    //  var dynData = DynamicData.For(self);

    //  int playerIndex = (int)dynData.Get("playerIndex");

    //  if (dynData.Get("input") == null)
    //    return orig(self);

    //  var input = DynamicData.For(dynData.Get("input"));
    //  if (input == null)
    //    return orig(self);
    //  InputState inputState = input.Invoke<InputState>("GetState");
    //  if (inputState.ArrowsPressed)
    //  {
    //    self.Scene.Add(new VirtualKeyboard(playerIndex));
    //  }
    //  //move to next name
    //  if ((bool)input.Get("MenuAlt2"))
    //  {
    //    SetPlayerName(playerIndex, getNextName(playerIndex));
    //  }
    //  dynData.Dispose();

    //  return orig(self);
    //}
  }

  // Entity pour surveiller les inputs et afficher la popup
  public class StatsInputWatcher : Entity
  {
    private bool alt2PressedLastFrame = false;
    public static bool popupIsShown = false;

    public StatsInputWatcher() : base(3)
    {
      // Entity invisible qui surveille les inputs
      popupIsShown = false;
    }

    public override void Update()
    {
      base.Update();

      if (!TFModFortRiseWinCountersModule.Settings.enable) return;

      // Détecter le bouton Y (MenuAlt2) de la manette - détection "just pressed"
      bool alt2Pressed = MenuInput.Alt2;
      bool alt2JustPressed = alt2Pressed && !alt2PressedLastFrame;
      alt2PressedLastFrame = alt2Pressed;

      if (alt2JustPressed && !popupIsShown)
      {
        // Créer et afficher la popup
        StatsPopup popup = new StatsPopup();
        base.Scene.Add(popup);
        popupIsShown = true;
      }
    }
  }

}
