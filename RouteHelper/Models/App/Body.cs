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
      Distance = $"{body.Distance:N0} ls";
      Type = body.Type;
      EstimatedScan = body.EstimatedScanValue.ToString("N0");
      EstimatedScanValue = body.EstimatedScanValue;
    }

    public int Jumps { get; }

    public string Type { get; }

    public string Distance { get; }

    public string BodyName { get; }

    public string SystemName { get; }

    public string EstimatedScan { get; }

    public long EstimatedScanValue { get; }
  }
}