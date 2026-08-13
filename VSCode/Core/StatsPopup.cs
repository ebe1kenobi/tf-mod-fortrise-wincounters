using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  /// <summary>
  /// Le tableau des statistiques, ouvert sur l'ecran de fin de match.
  ///
  /// Refait entierement. L'ancienne version calculait sa mise en page PENDANT le
  /// rendu : les ordonnees s'empilaient au fil des joueurs et de leurs tueurs, et
  /// personne ne savait, avant d'avoir tout dessine, quelle hauteur le tableau ferait.
  /// C'est ce qui rendait le defilement impossible a poser proprement - on ne peut pas
  /// faire glisser une liste dont on ignore la longueur.
  ///
  /// Ici les lignes sont CONSTRUITES une fois, a l'ouverture, dans une liste. Le rendu
  /// n'est plus qu'une fenetre qui glisse dessus : il sait combien il y a de lignes,
  /// combien tiennent a l'ecran, et donc jusqu'ou l'on peut descendre. Tout ce qui
  /// suit - la barre, le clavier, le fait qu'une ligne hors cadre ne soit pas dessinee
  /// - en decoule.
  ///
  /// Une ligne n'est pas un joueur : un joueur qui s'est fait tuer par trois personnes
  /// en occupe quatre. C'est justement ce qui faisait deborder le panneau.
  /// </summary>
  public class StatsPopup : Entity
  {
    /// <summary>Taille du texte. A 0,5 les chiffres n'etaient lisibles que de pres.</summary>
    private const float FontScale = 0.7f;

    private const float LineHeight = 9f;

    private const float PanelWidth = 300f;
    private const float PanelHeight = 220f;

    /// <summary>Hauteur reservee au titre et a l'entete, en haut du panneau.</summary>
    private const float HeaderHeight = 32f;

    /// <summary>Hauteur reservee au rappel des touches, en bas.</summary>
    private const float FooterHeight = 14f;

    /// <summary>Les colonnes, et leur abscisse relative au bord gauche du tableau.</summary>
    private static readonly (string Title, float X)[] Columns =
    {
      ("PLAYER", 0f),
      ("WIN", 74f),
      ("KILL", 110f),
      ("DEATH", 148f),
      ("SELF", 192f),
      ("KILLED BY / FROM", 228f)
    };

    /// <summary>Une ligne prete a dessiner : ses cellules, leur teinte.</summary>
    private sealed class Row
    {
      public readonly List<(string Text, float X, Color Color)> Cells = new();

      /// <summary>Vrai pour la premiere ligne d'un joueur : elle porte un filet.</summary>
      public bool StartsPlayer;
    }

    private readonly List<Row> rows = new();

    private bool focused;
    private float scroll;

    public StatsPopup() : base(new Vector2(160f, -120f), 3)
    {
      Build();

      Sounds.sfx_multiStartLevelControlFlyin.Play(160f, 1f);

      Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
      tween.OnUpdate = t => Position = Vector2.Lerp(new Vector2(160f, -120f), new Vector2(160f, 120f), t.Eased);
      tween.OnComplete = t => focused = true;
      Add(tween);
    }

    /// <summary>Combien de lignes tiennent dans le panneau.</summary>
    private static int Visible => (int)((PanelHeight - HeaderHeight - FooterHeight) / LineHeight);

    private int MaxScroll => Math.Max(0, rows.Count - Visible);

    // ------------------------------------------------------------------
    // Construction
    // ------------------------------------------------------------------

    private void Build()
    {
      var today = MyVersusMatchResults.winCounter.today;
      var total = MyVersusMatchResults.winCounter.total;

      foreach (var entry in today)
      {
        string name = entry.Key;
        PlayerStatData day = entry.Value;
        PlayerStatData all = total.ContainsKey(name) ? total[name] : new PlayerStatData();

        // La premiere ligne du joueur porte son nom et ses quatre compteurs.
        var first = new Row { StartsPlayer = true };
        first.Cells.Add((Short(name), Columns[0].X, Color.Yellow));
        first.Cells.Add((Pair(day.win, all.win), Columns[1].X, Color.White));
        first.Cells.Add((Pair(day.kill, all.kill), Columns[2].X, Color.White));
        first.Cells.Add((Pair(day.death, all.death), Columns[3].X, Color.White));
        first.Cells.Add((Pair(day.self, all.self), Columns[4].X, Color.White));
        rows.Add(first);

        // Les tueurs et les victimes viennent ensuite, une par ligne, dans la
        // derniere colonne. La premiere se pose sur la ligne du nom, les autres
        // s'ajoutent en dessous - c'est ce qui fait qu'un joueur n'est pas une ligne.
        List<string> detail = Detail(day, all);

        for (int i = 0; i < detail.Count; i++)
        {
          Row row = i == 0 ? first : new Row();
          row.Cells.Add((detail[i], Columns[5].X, i % 2 == 0 ? Color.White : Color.Gray));

          if (i > 0)
          {
            rows.Add(row);
          }
        }
      }
    }

    /// <summary>
    /// Les lignes de detail d'un joueur : qui l'a tue, et qui il a tue.
    ///
    /// Les deux tables sont fusionnees en une seule colonne. Elles etaient cote a
    /// cote, chacune s'allongeant de son cote, et le tableau avait alors deux hauteurs
    /// differentes selon la colonne qu'on regardait - impossible a faire defiler d'une
    /// seule piece.
    /// </summary>
    private static List<string> Detail(PlayerStatData day, PlayerStatData all)
    {
      var lines = new List<string>();

      foreach (string key in Keys(day.killBy, all.killBy))
      {
        lines.Add("BY " + Short(key) + " " + Pair(Get(day.killBy, key), Get(all.killBy, key)));
      }

      foreach (string key in Keys(day.killFrom, all.killFrom))
      {
        lines.Add("VS " + Short(key) + " " + Pair(Get(day.killFrom, key), Get(all.killFrom, key)));
      }

      return lines;
    }

    private static List<string> Keys(Dictionary<string, int> a, Dictionary<string, int> b)
    {
      var keys = new List<string>();

      foreach (string key in a.Keys)
      {
        if (!keys.Contains(key, StringComparer.OrdinalIgnoreCase)) { keys.Add(key); }
      }

      foreach (string key in b.Keys)
      {
        if (!keys.Contains(key, StringComparer.OrdinalIgnoreCase)) { keys.Add(key); }
      }

      keys.Sort(StringComparer.OrdinalIgnoreCase);
      return keys;
    }

    private static int Get(Dictionary<string, int> table, string key)
    {
      foreach (var entry in table)
      {
        if (string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))
        {
          return entry.Value;
        }
      }

      return 0;
    }

    /// <summary>"3(21)" : la soiree, puis le total entre parentheses.</summary>
    private static string Pair(int today, int total)
    {
      return today + "(" + total + ")";
    }

    /// <summary>
    /// Un nom raccourci a ce que la colonne peut tenir.
    ///
    /// La police du jeu ne dessine pas tout : un nom venu du clavier virtuel peut
    /// porter un caractere qu'elle ignore, et MeasureString leve alors en plein rendu.
    /// On coupe donc sur le nombre de caracteres et non sur la largeur mesuree.
    /// </summary>
    private static string Short(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return "?";
      }

      name = Drawable(name.ToUpperInvariant());

      if (name.Length == 0)
      {
        return "?";
      }

      return name.Length <= 8 ? name : name.Substring(0, 8);
    }

    /// <summary>
    /// Ne garde que ce que la police du jeu sait dessiner.
    ///
    /// Un nom vient du clavier virtuel ou d'un fichier edite a la main : rien ne
    /// garantit qu'il tienne dans le jeu de caracteres de la police. Un seul caractere
    /// inconnu leve une exception au moment du RENDU, c'est-a-dire loin de sa cause et
    /// une fois par image - l'ecran est perdu. On coupe donc a la construction.
    /// </summary>
    private static string Drawable(string text)
    {
      var kept = new System.Text.StringBuilder(text.Length);

      foreach (char c in text)
      {
        if (TFGame.Font.Characters.Contains(c))
        {
          kept.Append(c);
        }
      }

      return kept.ToString();
    }

    // ------------------------------------------------------------------

    public override void Update()
    {
      base.Update();

      if (!focused)
      {
        return;
      }

      if (MenuInput.ConfirmOrStart || MenuInput.Back)
      {
        Sounds.ui_click.Play(160f, 1f);
        Close();
        return;
      }

      if (MenuInput.Down && scroll < MaxScroll)
      {
        scroll++;
        Sounds.ui_move1.Play(160f, 1f);
      }
      else if (MenuInput.Up && scroll > 0)
      {
        scroll--;
        Sounds.ui_move1.Play(160f, 1f);
      }
    }

    public override void Render()
    {
      base.Render();

      float left = X - PanelWidth / 2f;
      float top = Y - PanelHeight / 2f;

      MenuPanel.DrawPanel(left, top, PanelWidth, PanelHeight);

      Draw.TextCentered(TFGame.Font, "STATISTICS", new Vector2(X, top + 10f), Color.White);

      if (rows.Count == 0)
      {
        Empty();
        return;
      }

      // L'entete ne defile pas : sans lui on ne sait plus quelle colonne on lit.
      float tableLeft = left + 12f;

      foreach ((string title, float x) in Columns)
      {
        Text(title, new Vector2(tableLeft + x, top + 24f), Color.Cyan);
      }

      // La fenetre : on ne dessine QUE les lignes qui tiennent. Monocle ne decoupe
      // pas, une ligne au-dela sortirait du panneau et se promenerait sur l'ecran.
      int first = (int)scroll;
      int last = Math.Min(rows.Count, first + Visible);

      for (int i = first; i < last; i++)
      {
        float y = top + HeaderHeight + (i - first) * LineHeight;

        // Un filet au-dessus de chaque nouveau joueur : sans lui, les lignes de detail
        // du precedent semblent appartenir au suivant.
        if (rows[i].StartsPlayer && i > first)
        {
          Draw.Rect(tableLeft, y - 2f, PanelWidth - 24f, 1f, Color.Gray * 0.35f);
        }

        foreach ((string text, float x, Color color) in rows[i].Cells)
        {
          Text(text, new Vector2(tableLeft + x, y + 2f), color);
        }
      }

      ScrollBar(left, top);

      string hint = MaxScroll > 0 ? "UP/DOWN: SCROLL   CONFIRM: CLOSE" : "CONFIRM: CLOSE";
      Draw.TextCentered(TFGame.Font, hint, new Vector2(X, top + PanelHeight - 8f), Color.Gray);
    }

    private void Empty()
    {
      // Deux situations tres differentes aboutissaient au meme message : le chargement
      // a echoue, ou il a reussi mais cette combinaison de joueurs n'a rien enregistre.
      // Le second cas est normal en debut de soiree et ne doit pas faire croire a une
      // panne.
      bool broken = TFModFortRiseWinCountersModule.StatsUnavailable;

      Draw.TextCentered(TFGame.Font, broken ? "ERROR - STATS" : "NO STATS FOUND",
          new Vector2(X, Y - 7f), Color.Gray);

      Draw.TextCentered(TFGame.Font, broken ? "NOT LOADED" : "FOR THOSE PLAYERS",
          new Vector2(X, Y + 7f), Color.Gray);

      Draw.TextCentered(TFGame.Font, "CONFIRM: CLOSE",
          new Vector2(X, Y + PanelHeight / 2f - 8f), Color.Gray);
    }

    /// <summary>
    /// Le rail, a droite du tableau, seulement quand il y a quelque chose plus bas.
    /// Une barre toujours visible ferait croire qu'il reste toujours du contenu cache.
    /// </summary>
    private void ScrollBar(float left, float top)
    {
      if (MaxScroll <= 0)
      {
        return;
      }

      float x = left + PanelWidth - 8f;
      float railTop = top + HeaderHeight;
      float railHeight = Visible * LineHeight;

      Draw.Rect(x, railTop, 2f, railHeight, Color.Gray * 0.4f);

      float height = Math.Max(6f, railHeight * Visible / rows.Count);
      float offset = (railHeight - height) * (scroll / MaxScroll);

      Draw.Rect(x, railTop + offset, 2f, height, Color.White);
    }

    private static void Text(string text, Vector2 at, Color color)
    {
      // Ancre a GAUCHE et non au centre : les colonnes s'alignent sur leur bord, et
      // un texte plus long ne pousse plus la colonne voisine.
      //
      // Contour noir : le panneau n'est pas uni, et du texte nu de 0,7 s'y perdait
      // par endroits. C'est ce qui rendait les chiffres durs a lire, autant que la
      // taille.
      Draw.OutlineTextJustify(TFGame.Font, text, at, color, Color.Black,
          new Vector2(0f, 0f), FontScale);
    }

    private void Close()
    {
      focused = false;
      Sounds.sfx_multiStartLevelControlFlyout.Play(160f, 1f);

      Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 20, true);
      tween.OnUpdate = t => Position = Vector2.Lerp(new Vector2(160f, 120f), new Vector2(160f, 360f), t.Eased);
      tween.OnComplete = t =>
      {
        StatsInputWatcher.popupIsShown = false;
        RemoveSelf();
      };

      Add(tween);
    }
  }
}
