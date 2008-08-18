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
      INeztuDatabase database = DatabaseHelper.GetDatabase();
      IRandomSelector randSel = new FullyRandomSelector(database);
      IScheduler s = new FIFOScheduler(database);

      if (args.Length > 0)
      {
        if(args[0] == "-u")
        {
          Vote v = s.GetNext();
          if (v != null)
          {
            database.RemoveVote(v.UserName, v.ReqTrack.TrackId);
            database.AddPending(v.UserName, v.ReqTrack.TrackId);
            Console.WriteLine(v.ReqTrack.Filename);
          }
        }
        else if (args[0] == "-r")
        {
          Track t = randSel.GetRandom();
          Console.WriteLine(t.Filename);
        }
        else if (args[0] == "-n")
        {
          string filename = string.Join(" ", args, 1, args.Length - 1);
          Vote[] votes = database.GetPending();
          foreach (Vote v in votes)
          {
            if (v.ReqTrack.Filename == filename)
            {
              database.RemovePending(v.UserName, v.ReqTrack.TrackId);
              database.AddHistory(v.UserName, v.ReqTrack.TrackId);
              return 0;
            }
          }

          Track t = database.GetTrack(filename);
          database.AddHistory(string.Empty, t.TrackId);
        }
      }
      else
      {
        string mediaPlayerCommand = ConfigurationManager.AppSettings["MediaPlayerCommand"];
        if (string.IsNullOrEmpty(mediaPlayerCommand))
        {
          Console.Error.WriteLine("MediaPlayerCommand not defined in .config file.  Aborting.");
          return 1;
        }

        while (true)
        {
          Track t;

          Vote v = s.GetNext();
          if (v != null)
          {
            t = v.ReqTrack;
            database.RemoveVote(v.UserName, t.TrackId);
            database.AddHistory(v.UserName, t.TrackId);
          }
          else
          {
            t = randSel.GetRandom();
            database.AddHistory(string.Empty, t.TrackId);
          }

          Process.Start(mediaPlayerCommand, "\"" + t.Filename + "\"").WaitForExit();
        }
      }

      return 0;
    }
  }
}
