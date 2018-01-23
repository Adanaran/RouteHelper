using System.Collections.Generic;
using Newtonsoft.Json;

namespace RouteHelper.Models.Spansh
{
  public class Route
  {
    [JsonProperty(PropertyName = "result")]
    public IList<StarSystem> Systems { get; set; }
  }
}