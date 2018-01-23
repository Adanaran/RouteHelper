using Newtonsoft.Json;

namespace RouteHelper.Models.Journal
{
  public class Scan : JournalEntryBase
  {
    [JsonProperty("BodyName")]
    public string BodyName { get; set; }
  }
}