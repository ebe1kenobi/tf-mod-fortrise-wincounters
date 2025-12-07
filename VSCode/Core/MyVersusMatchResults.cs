using System.Threading.Tasks;
using MonoMod.Utils;
using TowerFall;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using Monocle;
using TowerFall;
using Microsoft.Xna.Framework;
using System.Text;

//todo order name in popup by name
// decrease font size to fit more data

namespace TFModFortRiseWinCounters
{
  internal class MyVersusMatchResults
  {
    public static WinCounterData winCounter = new WinCounterData();

    internal static void Load()
    {
      On.TowerFall.VersusMatchResults.ctor += ctor_patch;
      //On.TowerFall.VersusMatchResults.PopupSequence += PopupSequence_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.VersusMatchResults.ctor -= ctor_patch;
      //On.TowerFall.VersusMatchResults.PopupSequence -= PopupSequence_patch;
    }

    public static void ctor_patch(On.TowerFall.VersusMatchResults.orig_ctor orig, global::TowerFall.VersusMatchResults self, global::TowerFall.Session session, global::TowerFall.VersusRoundResults roundResults)
    {
      orig(self, session, roundResults);

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
          //winCounter.increment(CustomNameImport.GetPlayerName(playerIndex));
          winCounter.addWinner(CustomNameImport.GetPlayerName(playerIndex));
        }
      }

      //need to save each time
      Task.Factory.StartNew(() => TFModFortRiseWinCountersModule.SaveCurrentResult());

      // Ajouter un Entity pour surveiller les inputs du bouton Y
      // Utiliser un Alarm car self.Scene peut être null dans le constructeur
      Alarm.Set(self, 1, delegate
      {
        if (self.Scene != null)
        {
          StatsInputWatcher watcher = new StatsInputWatcher();
          self.Scene.Add(watcher);
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

  // Classe popup pour afficher les statistiques
  public class StatsPopup : Entity
  {
    private bool focused;
    private bool finished;
    private const float FontScale = 0.50f; // Réduction de 25% de la taille de la police

    public StatsPopup() : base(new Vector2(160f, -120f), 3)
    {
      Sounds.sfx_multiStartLevelControlFlyin.Play(160f, 1f);
      Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
      tween.OnUpdate = delegate(Tween t)
      {
        this.Position = Vector2.Lerp(new Vector2(160f, -120f), new Vector2(160f, 120f), t.Eased);
      };
      tween.OnComplete = delegate(Tween t)
      {
        this.focused = true;
      };
      base.Add(tween);
    }

    public override void Update()
    {
      base.Update();
      if (this.focused)
      {
        if (MenuInput.ConfirmOrStart || MenuInput.Back)
        {
          Sounds.ui_click.Play(160f, 1f);
          this.TweenOut();
        }
      }
    }

    public override void Render()
    {
      // Parcourir tous les joueurs dans winCounter.today
      var todayStats = MyVersusMatchResults.winCounter.today;
      float panelHeight = 0f;
      if (todayStats.Count == 0)
      {
        panelHeight = 240f;
        MenuPanel.DrawPanel(base.X - 100f, base.Y - panelHeight / 2f, 200f, panelHeight);
        Draw.TextCentered(TFGame.Font, "STATISTIQUES", this.Position + new Vector2(0f, -panelHeight / 2f + 20f), Color.White);
        Draw.TextCentered(TFGame.Font, "AUCUNE STATISTIQUE", this.Position + new Vector2(0f, 0f), Color.Gray);
        //Draw.TextCentered(TFGame.Font, "APPUYEZ SUR A OU B POUR FERMER", this.Position + new Vector2(0f, panelHeight / 2f - 20f), Color.Gray);
        base.Render();
        return;
      }

      // Créer la liste des colonnes : WIN, KILL, DEATH, SELF, KILL BY, KILL FROM
      var columns = new System.Collections.Generic.List<string>();
      columns.Add("WIN");
      columns.Add("KILL");
      columns.Add("DEATH");
      columns.Add("SELF");
      columns.Add("KILL BY");
      columns.Add("KILL FROM");

      // Calculer la largeur nécessaire (nom joueur + colonnes)
      float columnWidth = 30f;
      float playerNameWidth = 30f;
      float killByWidth = 50f; // Colonne plus large pour le texte
      float killFromWidth = 50f; // Colonne plus large pour le texte
      float totalWidth = playerNameWidth + (4 * columnWidth) + killByWidth + killFromWidth;
      float startX = this.Position.X - totalWidth / 2f + playerNameWidth / 2f;
      //float startX = 0;

      // Calculer la hauteur avec le scale réduit
      float lineHeight = 10f * FontScale;
      //panelHeight = 50f + (todayStats.Count + 1) * lineHeight + 30f; // titre + en-tête + lignes joueurs + footer
      panelHeight = 240f;
      MenuPanel.DrawPanel(base.X - totalWidth / 2f - 10f, base.Y - panelHeight / 2f, totalWidth + 20f, panelHeight);
      Draw.TextCentered(TFGame.Font, "STATISTIQUES", this.Position + new Vector2(0f, -panelHeight / 2f + 15f), Color.White);

      // Afficher l'en-tête
      float yOffset = -panelHeight / 2f + 35f;
      float xOffset = startX;

      // Colonne nom joueur
      DrawTextScaled("JOUEUR", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.Yellow);
      xOffset += playerNameWidth;

      // Colonnes de stats
      foreach (var col in columns)
      {
        DrawTextScaled(col, this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.Cyan);
        xOffset += columnWidth;
      }
      yOffset += lineHeight;

      // Afficher les lignes de données pour chaque joueur
      foreach (var kvp in todayStats)
      {
        string playerName = kvp.Key;
        PlayerStatData todayData = kvp.Value;
        PlayerStatData totalData = MyVersusMatchResults.winCounter.total.ContainsKey(playerName)
          ? MyVersusMatchResults.winCounter.total[playerName]
          : new PlayerStatData();

        xOffset = startX;

        // Nom du joueur
        DrawTextScaled(playerName, this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.Yellow);
        xOffset += playerNameWidth;

        // WIN
        DrawTextScaled($"{todayData.win}({totalData.win})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        xOffset += columnWidth;

        // KILL
        DrawTextScaled($"{todayData.kill}({totalData.kill})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        xOffset += columnWidth;

        // DEATH
        DrawTextScaled($"{todayData.death}({totalData.death})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        xOffset += columnWidth;

        // SELF
        DrawTextScaled($"{todayData.self}({totalData.self})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        xOffset += columnWidth;

        // KILL BY - regrouper toutes les valeurs
        var killByText = new StringBuilder();
        var sortedKillByKeys = new System.Collections.Generic.List<string>(todayData.killBy.Keys);
        foreach (var key in totalData.killBy.Keys)
        {
          if (!sortedKillByKeys.Contains(key)) sortedKillByKeys.Add(key);
        }
        sortedKillByKeys.Sort();

        float yOffsetSave = yOffset;
        foreach (var key in sortedKillByKeys)
        {
          int todayValue = todayData.killBy.ContainsKey(key) ? todayData.killBy[key] : 0;
          int totalValue = totalData.killBy.ContainsKey(key) ? totalData.killBy[key] : 0;
          //if (killByText.Length > 0) killByText.Append(" ");
          //killByText.Append($"{key}:{todayValue}({totalValue})");
          DrawTextScaled($"{key}:{todayValue}({totalValue})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
          yOffset += lineHeight;
        }
        //if (killByText.Length == 0) killByText.Append("-");
        ////DrawTextScaled(killByText.ToString(), this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        //DrawTextScaled($"{todayData.self}({totalData.self})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        xOffset += killByWidth;

        // KILL FROM - regrouper toutes les valeurs (avec clés en majuscule)
        yOffset = yOffsetSave;
        var killFromText = new StringBuilder();
        var sortedKillFromKeys = new System.Collections.Generic.List<string>();
        foreach (var key in todayData.killFrom.Keys)
        {
          string keyUpper = key.ToUpper();
          if (!sortedKillFromKeys.Contains(keyUpper)) sortedKillFromKeys.Add(keyUpper);
        }
        foreach (var key in totalData.killFrom.Keys)
        {
          string keyUpper = key.ToUpper();
          if (!sortedKillFromKeys.Contains(keyUpper)) sortedKillFromKeys.Add(keyUpper);
        }
        sortedKillFromKeys.Sort();

        yOffsetSave = yOffset;
        foreach (var keyUpper in sortedKillFromKeys)
        {
          // Trouver la clé originale
          string originalKey = keyUpper;
          foreach (var orig in todayData.killFrom.Keys)
          {
            if (orig.ToUpper() == keyUpper)
            {
              originalKey = orig;
              break;
            }
          }
          foreach (var orig in totalData.killFrom.Keys)
          {
            if (orig.ToUpper() == keyUpper && !todayData.killFrom.ContainsKey(originalKey))
            {
              originalKey = orig;
              break;
            }
          }

          int todayValue = todayData.killFrom.ContainsKey(originalKey) ? todayData.killFrom[originalKey] : 0;
          int totalValue = totalData.killFrom.ContainsKey(originalKey) ? totalData.killFrom[originalKey] : 0;
          //if (killFromText.Length > 0) killFromText.Append(" ");
          //killFromText.Append($"{keyUpper}:{todayValue}({totalValue})");
          DrawTextScaled($"{keyUpper}:{todayValue}({totalValue})", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
          yOffset += lineHeight;
        }

        //if (killFromText.Length == 0) killFromText.Append("-");
        ////DrawTextScaled(killFromText.ToString(), this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        //DrawTextScaled($"{todayData.self}({totalData.self})\n\n", this.Position + new Vector2(xOffset - this.Position.X, yOffset), Color.White);
        //xOffset += killFromWidth;

        yOffset += lineHeight; //todo add more for each \n\n ajouter
      }

      //Draw.TextCentered(TFGame.Font, "APPUYEZ SUR A OU B POUR FERMER", this.Position + new Vector2(0f, panelHeight / 2f - 15f), Color.Gray);
      base.Render();
    }

    // Méthode helper pour dessiner du texte avec un scale réduit
    private void DrawTextScaled(string text, Vector2 position, Color color)
    {
      var origin = TFGame.Font.MeasureString(text) * 0.5f;
      Draw.SpriteBatch.DrawString(TFGame.Font, text, position, color, 0f, origin, FontScale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
    }

    private void TweenOut()
    {
      this.focused = false;
      Sounds.sfx_multiStartLevelControlFlyout.Play(160f, 1f);
      Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 20, true);
      tween.OnUpdate = delegate(Tween t)
      {
        this.Position = Vector2.Lerp(new Vector2(160f, 120f), new Vector2(160f, 360f), t.Eased);
      };
      tween.OnComplete = delegate(Tween t)
      {
        this.finished = true;
        StatsInputWatcher.popupIsShown = false; // Réinitialiser le flag quand la popup est fermée
        base.RemoveSelf();
      };
      base.Add(tween);
    }
  }
}
