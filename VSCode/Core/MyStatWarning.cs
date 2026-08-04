using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace TFModFortRiseWinCounters
{
  // Bandeau d'avertissement affiche en bas de l'ecran quand les statistiques en
  // ligne n'ont pas pu etre chargees : sans lui, les compteurs repartent de zero
  // sans que personne ne s'en apercoive avant la fin de la soiree.
  //
  // Le rendu se greffe sur Monocle.Engine.Draw plutot que sur Level.PostScreen :
  // le chargement des stats est declenche depuis le rollcall, et les ecrans qui
  // suivent (reglages du match, carte) ne sont pas des Level. Un hook sur Level
  // n'affichait donc rien avant le debut du match, c'est-a-dire pas au moment ou
  // l'information compte. Engine.Draw couvre toutes les scenes.
  //
  // A ce stade Engine.Draw a deja fait SetRenderTarget(null) puis Screen.Render() :
  // on dessine sur le back buffer, a l'echelle de l'ecran, exactement comme le
  // jeu le fait pour son HUD dans Level.PostScreen.
  public class MyStatWarning : IHookable
  {
    private const string Message = "STATS NON CHARGEES - COMPTEURS A ZERO";

    // Ecran interne du jeu : 320x240. On garde une marge pour ne pas coller aux
    // bords, et on descend le texte au ras du bas.
    private const float ScreenWidth = 320f;
    private const float MaxWidth = 300f;
    private const float BaselineY = 232f;

    // Le bandeau s'efface au bout de ce delai : l'information est utile au moment
    // ou elle tombe, pas pendant toute la soiree. Le compteur est relance a chaque
    // nouvelle alerte (voir TFModFortRiseWinCountersModule.StatsUnavailable).
    private const double DisplaySeconds = 10.0;

    private static double shownFor;
    private static bool loggedOnce;

    public static void ResetDisplayTimer()
    {
      shownFor = 0.0;
    }

    public static void Load(IHarmony harmony)
    {
      // Engine.Draw est protegee : patch par nom.
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Engine), "Draw"),
          postfix: new HarmonyMethod(Draw_patch)
      );
    }

    private static void Draw_patch(GameTime gameTime)
    {
      if (!TFModFortRiseWinCountersModule.StatsUnavailable) return;
      if (shownFor >= DisplaySeconds) return;

      // Temps de rendu reel : le bandeau reste donc 10 s a l'ecran meme si le jeu
      // rame ou si on traverse plusieurs scenes pendant ce laps de temps.
      if (gameTime != null)
        shownFor += gameTime.ElapsedGameTime.TotalSeconds;

      try
      {
        Screen screen = Engine.Instance != null ? Engine.Instance.Screen : null;
        if (screen == null) return;

        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            screen.Matrix);

        // L'echelle est deduite de la largeur reelle du texte : le message tient
        // ainsi a l'ecran quelle que soit la police, sans valeur codee en dur.
        float width = TFGame.Font.MeasureString(Message).X;
        float scale = width > MaxWidth ? MaxWidth / width : 1f;

        // Contour noir : lisible aussi bien sur une tour claire que sombre.
        Draw.OutlineTextCentered(
            TFGame.Font, Message,
            new Vector2(ScreenWidth / 2f, BaselineY),
            Color.OrangeRed, scale);

        Draw.SpriteBatch.End();

        if (!loggedOnce)
        {
          loggedOnce = true;
          TFModFortRiseWinCounters.Logger.Info("[WinCounters] avertissement affiche a l'ecran");
        }
      }
      catch (System.Exception ex)
      {
        // Un avertissement ne doit jamais faire tomber le rendu du jeu : on le
        // desamorce plutot que de relancer l'erreur a chaque frame.
        TFModFortRiseWinCounters.Logger.Info($"[WinCounters] affichage de l'avertissement desactive : {ex.Message}");
        TFModFortRiseWinCountersModule.StatsUnavailable = false;
      }
    }
  }
}
