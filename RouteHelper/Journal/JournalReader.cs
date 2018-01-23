using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Caliburn.Micro;
using Newtonsoft.Json;
using RouteHelper.Models.Journal;

namespace RouteHelper.Journal
{
  public class JournalReader
  {
    private readonly JsonSerializerSettings jsonSerializerSettings;
    private FileSystemWatcher fileSystemWatcher;
    private FileInfo journalFile;
    private string journalPath;

    private long lastPosition;
    private Timer timer;

    public JournalReader()
    {
      jsonSerializerSettings = new JsonSerializerSettings
      {
        Converters = new List<JsonConverter>
        {
          new JournalEntryToSubTypeConverter()
        }
      };
    }

    public event EventHandler<Location> LocationChanged;
    public event EventHandler<Scan> BodyScanned;

    public void Initialize()
    {
      if (SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out var path) >= 0)
      {
        var journalParentDirectory = Marshal.PtrToStringUni(path);
        if (journalParentDirectory != null)
        {
          journalPath = Path.Combine(journalParentDirectory, "Frontier Developments", "Elite Dangerous");
        }
      }

      if (string.IsNullOrWhiteSpace(journalPath) || !Directory.Exists(journalPath))
      {
        return;
      }

      if (fileSystemWatcher != null)
      {
        fileSystemWatcher.Created -= FileSystemWatcherOnCreated;
      }

      fileSystemWatcher = new FileSystemWatcher
      {
        Path = journalPath,
        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size,
        Filter = "Journal.*.log"
      };

      fileSystemWatcher.Created += FileSystemWatcherOnCreated;

      timer = new Timer(1000);
      timer.Elapsed += TimerOnElapsed;
      timer.Start();
      fileSystemWatcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
      if (fileSystemWatcher != null)
      {
        fileSystemWatcher.EnableRaisingEvents = false;
      }

      timer?.Dispose();
    }

    private async void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
    {
      timer.Stop();

      await Task.Run(() =>
      {
        journalFile = GetLastEditedJournalFile(journalPath, fileSystemWatcher.Filter);
        if (journalFile == null) { return; }

        var fileLength = journalFile.Length;
        var readLength = (int) (fileLength - lastPosition);
        if (readLength < 0)
        {
          readLength = 0;
        }

        using (var fileStream = journalFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          fileStream.Seek(lastPosition, SeekOrigin.Begin);
          var bytes = new byte[readLength];
          var haveRead = 0;
          while (haveRead < readLength)
          {
            haveRead += fileStream.Read(bytes, haveRead, readLength - haveRead);
            fileStream.Seek(lastPosition + haveRead, SeekOrigin.Begin);
          }

          // Convert bytes to string
          var s = Encoding.UTF8.GetString(bytes);
          var lines = s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
          foreach (var line in lines)
          {
            Parse(line);
          }
        }

        lastPosition = fileLength;
      });

      timer.Start();
    }

    private FileInfo GetLastEditedJournalFile(string filePath, string filter)
    {
      return new DirectoryInfo(filePath).GetFiles(filter).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
    }

    private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs args)
    {
      lastPosition = 0;
    }

    private void Parse(string line)
    {
      var journalEntry = JsonConvert.DeserializeObject<JournalEntryBase>(line, jsonSerializerSettings);
      switch (journalEntry)
      {
        case FSDJump jump:
          Invoke(LocationChanged, jump);
          break;
        case Location location:
          Invoke(LocationChanged, location);
          break;
        case Scan scan:
          Invoke(BodyScanned, scan);
          break;
        case null:
          break;
        default:
          throw new Exception($"Unmapped journal event '{journalEntry.GetType().Name}'");
      }
    }

    private void Invoke<T>(EventHandler<T> handler, T fsdJump)
    {
      Execute.OnUIThreadAsync(() => handler?.Invoke(this, fsdJump));

    }

    //turns out that this is how Elite gets the file path. Neat.
    [DllImport("Shell32.dll")]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
  }
}