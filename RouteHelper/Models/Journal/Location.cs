using Newtonsoft.Json;

namespace RouteHelper.Models.Journal
{
  public class Location : JournalEntryBase
  {
    [JsonProperty("StarSystem")]
    public string SystemName { get; set; }
  }
}