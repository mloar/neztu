using System;
using System.Configuration;
using System.Diagnostics;

namespace Neztu
{
  public class Daemon
  {
    public static int Main(string[] args)
    {
      string mediaPlayerCommand = ConfigurationManager.AppSettings["MediaPlayerCommand"];
      if (string.IsNullOrEmpty(mediaPlayerCommand))
      {
        Console.Error.WriteLine("MediaPlayerCommand not defined in .config file.  Aborting.");
        return 1;
      }

      ITrackDatabase trackDb = new PostgresTrackDatabase();
      IStateDatabase stateDb = new PostgresStateDatabase();
      stateDb.Initialize(trackDb);

      IRandomSelector randSel = new FullyRandomSelector();
      randSel.Initialize(trackDb);

      IScheduler s = new FIFOScheduler();
      s.Initialize(randSel, stateDb);

      while (true)
      {
        Track t = s.PlayNext();
        Process.Start(mediaPlayerCommand, "\"" + t.Filename + "\"").WaitForExit();
      }
    }
  }
}
