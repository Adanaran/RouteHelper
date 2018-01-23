using RouteHelper.Models.Spansh;

namespace RouteHelper.Models.App
{
  public class Body
  {
    public Body(StarSystem system, SystemBody body)
    {
      SystemName = system.Name;
      Jumps = system.Jumps;
      BodyName = body.Name;
      Distance = $"{body.Distance} ls";
      Type = body.Type;
    }

    public int Jumps { get; }

    public string Type { get; }

    public string Distance { get; }

    public string BodyName { get; }

    public string SystemName { get; }
  }
}