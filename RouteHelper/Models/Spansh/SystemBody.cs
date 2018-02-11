using Newtonsoft.Json;

namespace RouteHelper.Models.Spansh
{
  public class SystemBody
  {
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "distance_to_arrival")]
    public long Distance { get; set; }

    [JsonProperty(PropertyName = "estimated_scan_value")]
    public int EstimatedScanValue { get; set; }

  }
}