//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements the daemon which plays tracks.
//
//-----------------------------------------------------------------------------

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

      INeztuDatabase database = DatabaseHelper.GetDatabase();
      IRandomSelector randSel = new FullyRandomSelector(database);
      IScheduler s = new FIFOScheduler(randSel, database);

      while (true)
      {
        Track t = s.PlayNext();
        Process.Start(mediaPlayerCommand, "\"" + t.Filename + "\"").WaitForExit();
      }
    }
  }
}
