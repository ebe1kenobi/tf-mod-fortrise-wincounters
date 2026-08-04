using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;
using System.Net.Http;
using FortRise;
namespace TFModFortRiseWinCounters
{
  public class APIStat
  {
    private string urlTemplate;

    /// <summary>
    /// settings.json est livre avec le mod (ModFile/). FortRise 4 le lisait via un
    /// chemin relatif au repertoire courant (.\FortRise\Mods\...), ce qui ne resout
    /// plus correctement en FortRise 5. On passe par IModContent : fiable, et
    /// fonctionne aussi bien en dossier qu'en zip.
    /// </summary>
    public APIStat(IModContent content, string fileName)
    {
      try
      {
        IResourceInfo resource;
        if (content != null && content.TryGetResource(fileName, out resource))
        {
          var config = JsonSerializer.Deserialize<Config>(resource.Text);
          urlTemplate = config?.appliWebUrl ?? "";
        }
        else
        {
          TFModFortRiseWinCounters.Logger.Info($"[APIStat] config introuvable : {fileName}");
          urlTemplate = "";
        }
      }
      catch (Exception ex)
      {
        TFModFortRiseWinCounters.Logger.Info($"[APIStat] config illisible : {ex.Message}");
        urlTemplate = "";
      }
    }

    public APIStat(string configPath)
    {
      // Charger le JSON de config avec Newtonsoft
      //TFModFortRiseWinCounters.Logger.Info($"APIStat {configPath}");
      //try
      //{
      //  ;
      //  var json = File.ReadAllText(configPath);
      //  TFModFortRiseWinCounters.Logger.Info($"json {json}");
      //  var config = JsonConvert.DeserializeObject<Config>(json);
      //  urlTemplate = config.appliWebUrl;
      //}
      //catch (Exception ex)
      //{
      //  urlTemplate = "";
      //}

      try
      {
        var json = File.ReadAllText(configPath);
        //TFModFortRiseWinCounters.Logger.Info($"json {json}");

        var config = JsonSerializer.Deserialize<Config>(json);

        urlTemplate = config?.appliWebUrl ?? "";
      }
      catch (Exception ex)
      {
        //TFModFortRiseWinCounters.Logger.Info(ex.ToString());
        urlTemplate = "";
      }
    }

    // Appel synchrone declenche depuis le lancement d'un versus (RollcallElement) :
    // tant qu'il n'a pas rendu, le jeu est fige. Le timeout par defaut de
    // HttpWebRequest etant de 100 s, un serveur injoignable bloquait tout ce temps.
    public const int TimeoutMs = 15000;

    // Rend null si les stats n'ont pas pu etre recuperees (timeout, serveur muet,
    // erreur HTTP, JSON invalide). L'appelant previent alors le joueur plutot que
    // de faire tomber le mod ou le jeu.
    public Sheet GetStat(string id, string date)
    {
      if (string.IsNullOrEmpty(urlTemplate))
      {
        TFModFortRiseWinCounters.Logger.Info("[APIStat] pas d'URL configuree, stats en ligne ignorees");
        return null;
      }

      string finalUrl = urlTemplate.Replace("[#ID#]", Uri.EscapeDataString(id));
      finalUrl = finalUrl.Replace("[#DATE#]", Uri.EscapeDataString(date));
      TFModFortRiseWinCounters.Logger.Info($"finalUrl {finalUrl}");

      try
      {
        var request = (HttpWebRequest)WebRequest.Create(finalUrl);
        request.Method = "GET";
        request.Timeout = TimeoutMs;
        request.ReadWriteTimeout = TimeoutMs;

        using (var response = (HttpWebResponse)request.GetResponse())
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          string result = reader.ReadToEnd();
          return JsonSerializer.Deserialize<Sheet>(result);
        }
      }
      catch (WebException ex)
      {
        string cause = ex.Status == WebExceptionStatus.Timeout
            ? $"pas de reponse en {TimeoutMs / 1000} s"
            : ex.Status.ToString();
        TFModFortRiseWinCounters.Logger.Info($"[APIStat] GetStat echoue ({cause}) : {ex.Message}");
        return null;
      }
      catch (Exception ex)
      {
        TFModFortRiseWinCounters.Logger.Info($"[APIStat] GetStat : reponse inexploitable : {ex.Message}");
        return null;
      }
    }

    // Meme contrainte que GetStat : appel synchrone sur le thread de jeu, en fin
    // de match. Rend false si l'envoi n'a pas abouti ; la sauvegarde locale doit
    // se faire quoi qu'il arrive (voir SaveCurrentResult).
    public bool PostStat(string id, string date, string json)
    {
      if (string.IsNullOrEmpty(urlTemplate))
        return false;

      string finalUrl = urlTemplate.Replace("[#ID#]", Uri.EscapeDataString(id));
      finalUrl = finalUrl.Replace("[#DATE#]", Uri.EscapeDataString(date));
        //TFModFortRiseWinCounters.Logger.Info($"PostStat {finalUrl}");
      Sheet sheet = new Sheet()
      {
        id = id,
        date = date,
        value = json,
      };

      //var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sheet, Formatting.Indented));
      var data = JsonSerializer.Serialize(sheet, new JsonSerializerOptions
      {
        WriteIndented = true
      });
      //TFModFortRiseWinCounters.Logger.Info($"data {data}");

      var data2 = Encoding.UTF8.GetBytes(data);

      try
      {
        var request = (HttpWebRequest)WebRequest.Create(finalUrl);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = data2.Length;
        request.Timeout = TimeoutMs;
        request.ReadWriteTimeout = TimeoutMs;

        using (var stream = request.GetRequestStream())
        {
          stream.Write(data2, 0, data2.Length);
        }

        using (var response = (HttpWebResponse)request.GetResponse())
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          reader.ReadToEnd();
        }
        return true;
      }
      catch (Exception ex)
      {
        TFModFortRiseWinCounters.Logger.Info($"[APIStat] PostStat echoue : {ex.Message}");
        return false;
      }
    }



    private class Config
    {
      [JsonPropertyName("appliWebUrl")]
      public string appliWebUrl { get; set; }
    }

    public class Sheet
    {
      [JsonPropertyName("error")]
      public string error { get; set; }
      [JsonPropertyName("status")]
      public string status { get; set; }
      [JsonPropertyName("id")]
      public string id { get; set; }
      [JsonPropertyName("value")]
      public string value { get; set; }
      [JsonPropertyName("date")]
      public string date { get; set; }
    }
  }
}
