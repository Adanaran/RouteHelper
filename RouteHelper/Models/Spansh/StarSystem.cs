using System.Collections.Generic;
using Newtonsoft.Json;

namespace RouteHelper.Models.Spansh
{
  public class StarSystem
  {
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "jumps")]
    public int Jumps { get; set; }

    [JsonProperty(PropertyName = "bodies")]
    public IList<SystemBody> Bodies { get; set; }
  }
}