using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;
using System.Net.Http;
namespace TFModFortRiseWinCounters
{
  public class APIStat
  {
    private string urlTemplate;

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

    public Sheet GetStat(string id, string date)
    {

      string finalUrl = urlTemplate.Replace("[#ID#]", Uri.EscapeDataString(id));
      finalUrl = finalUrl.Replace("[#DATE#]", Uri.EscapeDataString(date));
      //TFModFortRiseWinCounters.Logger.Info($"finalUrl {finalUrl}");

      var request = (HttpWebRequest)WebRequest.Create(finalUrl);
      request.Method = "GET";

      using (var response = (HttpWebResponse)request.GetResponse())
      using (var reader = new StreamReader(response.GetResponseStream()))
      {
        //TFModFortRiseWinCounters.Logger.Info($"GetResponseStream");

        string result = reader.ReadToEnd();
        //Sheet sheet = JsonConvert.DeserializeObject<Sheet>(result);
        //TFModFortRiseWinCounters.Logger.Info($"result {result}");

        Sheet sheet = JsonSerializer.Deserialize<Sheet>(result);
        return sheet;
      }
    }

    public void PostStat(string id, string date, string json)
    {
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

      var request = (HttpWebRequest)WebRequest.Create(finalUrl);
      request.Method = "POST";
      request.ContentType = "application/json";
      request.ContentLength = data2.Length;

      using (var stream = request.GetRequestStream())
      {
        stream.Write(data2, 0, data2.Length);
      }

      using (var response = (HttpWebResponse)request.GetResponse())
      using (var reader = new StreamReader(response.GetResponseStream()))
      {
        string result = reader.ReadToEnd();
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
