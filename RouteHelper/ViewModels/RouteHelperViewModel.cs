using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using RouteHelper.Journal;
using RouteHelper.Models.App;
using RouteHelper.Models.Journal;
using RouteHelper.Models.Spansh;

namespace RouteHelper.ViewModels
{
  public class RouteHelperViewModel : Screen
  {
    private const string RouteFileName = "route.json";
    private const string VisitedSystemsFileName = "ImportStars.txt";
    private const string Elite = "Elite";
    private readonly JournalReader journalReader;
    private readonly IList<string> visitedSystems;
    private Body selectedBody;
    private int systemCount;
    private int bodyCount;

    public RouteHelperViewModel(JournalReader journalReader)
    {
      this.journalReader = journalReader;
      journalReader.LocationChanged += JournalReaderOnLocationChanged;
      journalReader.BodyScanned += JournalReaderOnBodyScanned;
      Route = new ObservableCollection<Body>();
      visitedSystems = new List<string>();
    }

    public ObservableCollection<Body> Route { get; }

    public Body SelectedBody
    {
      get => selectedBody;
      set
      {
        selectedBody = value;
        NotifyOfPropertyChange();
      }
    }

    public int SystemCount
    {
      get => systemCount;
      set
      {
        systemCount = value; 
        NotifyOfPropertyChange();
      }
    }

    public int BodyCount
    {
      get => bodyCount;
      set { bodyCount = value;
        NotifyOfPropertyChange();
      }
    }

    public void CopyToClipBoard()
    {
      CopyToClipBoard(SelectedBody);
    }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      journalReader.Initialize();
      LoadRoute();
    }

    protected override void OnDeactivate(bool close)
    {
      if (close)
      {
        journalReader.Stop();
      }

      base.OnDeactivate(close);
    }

    private void JournalReaderOnBodyScanned(object sender, Scan scan)
    {
      var scannedBody = Route.FirstOrDefault(x => x.BodyName == scan.BodyName);
      if (scannedBody != null)
      {
        Route.Remove(scannedBody);
        SelectedBody = Route.FirstOrDefault();
        if (scannedBody.SystemName != SelectedBody?.SystemName)
        {
          WriteSystemToDisk(scannedBody.SystemName);
        }

        Recalculate();
      }
    }

    private void Recalculate()
    {
      BodyCount = Route.Count;
      SystemCount = Route.Select(x => x.SystemName).Distinct().Count();
    }

    private void JournalReaderOnLocationChanged(object sender, Location location)
    {
      visitedSystems.Add(location.SystemName);
      var body = Route.FirstOrDefault(x => x.SystemName != location.SystemName);
      CopyToClipBoard(body);
    }

    private void CopyToClipBoard(Body body)
    {
      if (body == null) { return; }

      Clipboard.SetText(body.SystemName);
    }

    private async void LoadRoute()
    {
      var routeTask = LoadRouteFromDisk();
      var visitedSystemsTask = LoadVisitedSystems();

      await Task.WhenAll(visitedSystemsTask, routeTask);

      var route = routeTask.Result;
      foreach (var system in route.Systems)
      {
        if (!visitedSystems.Contains(system.Name))
        {
          foreach (var body in system.Bodies)
          {
            Route.Add(new Body(system, body));
          }
        }
      }

      SelectedBody = Route.FirstOrDefault();
      Recalculate();
      CopyToClipBoard();
    }

    private static async Task<Route> LoadRouteFromDisk()
    {
      var routePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Elite, RouteFileName);
      using (var reader = File.OpenText(routePath))
      {
        var content = await reader.ReadToEndAsync();
        var route = JsonConvert.DeserializeObject<Route>(content);
        return route;
      }
    }

    private async Task LoadVisitedSystems()
    {
      var visitedSystemsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Elite, VisitedSystemsFileName);
      using (var reader = File.OpenText(visitedSystemsPath))
      {
        while (!reader.EndOfStream)
        {
          var line = await reader.ReadLineAsync();
          visitedSystems.Add(line);
        }
      }
    }

    private void WriteSystemToDisk(string system)
    {
      var visitedSystemsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Elite, VisitedSystemsFileName);
      using (var fileStream = File.Open(visitedSystemsPath, FileMode.Append, FileAccess.Write))
      {
        using (var streamWriter = new StreamWriter(fileStream))
        {
          streamWriter.WriteLine(system);
        }
      }
    }
  }
}