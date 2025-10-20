using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace TFModFortRiseWinCounters
{
  public class APIStat
  {
    private string urlTemplate;

    public APIStat(string configPath)
    {
      // Charger le JSON de config avec Newtonsoft
      try
      {
        var json = File.ReadAllText(configPath);
        var config = JsonConvert.DeserializeObject<Config>(json);
        urlTemplate = config.appliWebUrl;
      } catch (Exception ex) {
        urlTemplate = "";
      }
    }

    public Sheet GetStat(string id, string date)
    {
      string finalUrl = urlTemplate.Replace("[#ID#]", Uri.EscapeDataString(id));
      finalUrl = finalUrl.Replace("[#DATE#]", Uri.EscapeDataString(date));

      var request = (HttpWebRequest)WebRequest.Create(finalUrl);
      request.Method = "GET";

      using (var response = (HttpWebResponse)request.GetResponse())
      using (var reader = new StreamReader(response.GetResponseStream()))
      {
        string result = reader.ReadToEnd();
        Sheet sheet = JsonConvert.DeserializeObject<Sheet>(result);
        return sheet;
      }
    }

    public void PostStat(string id, string date, string json)
    {
      string finalUrl = urlTemplate.Replace("[#ID#]", Uri.EscapeDataString(id));
      finalUrl = finalUrl.Replace("[#DATE#]", Uri.EscapeDataString(date));
      Sheet sheet = new Sheet()
      {
        id = id,
        date = date,
        value = json,
      };

      var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sheet, Formatting.Indented));

      var request = (HttpWebRequest)WebRequest.Create(finalUrl);
      request.Method = "POST";
      request.ContentType = "application/json";
      request.ContentLength = data.Length;

      using (var stream = request.GetRequestStream())
      {
        stream.Write(data, 0, data.Length);
      }

      using (var response = (HttpWebResponse)request.GetResponse())
      using (var reader = new StreamReader(response.GetResponseStream()))
      {
        string result = reader.ReadToEnd();
      }
    }


    private class Config
    {
      public string appliWebUrl { get; set; }
    }

    public class Sheet
    {
      public string error { get; set; }
      public string status { get; set; }
      public string id { get; set; }
      public string value { get; set; }
      public string date { get; set; }
    }
  }
}
