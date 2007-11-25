using System;
using System.Diagnostics;

namespace Neztu
{
  public class Daemon
  {
    public static void Main(string[] args)
    {
      ITrackDatabase trackDb = new PostgresTrackDatabase();
      IVoteDatabase voteDb = new PostgresVoteDatabase();
      IHistoryDatabase historyDb = new PostgresHistoryDatabase();
      IRandomSelector randSel = new FullyRandomSelector();
      randSel.Initialize(trackDb);

      IScheduler s = new FIFOScheduler();
      s.Initialize(randSel, trackDb, voteDb, historyDb);

      while (true)
      {
        Track t = s.PlayNext();
        Process.Start("mplayer", "\"" + t.Filename + "\"").WaitForExit();
      }
    }
  }
}
