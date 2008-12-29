//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements the player daemon.
//
//-----------------------------------------------------------------------------

#include <list>
#include <iostream>
#include <stdlib.h>
#include <unistd.h>

#include "Database.h"
#include "Scheduler.h"

int main(int argc, char* argv[])
{
  Configuration config("/etc/neztu.conf");
  Database db(config);
  Scheduler scheduler(config, db);

  if (daemon(0, 0))
  {
    std::cerr << "Could not fork" << std::endl;
    return 1;
  }

  for (;;)
  {
    const Vote &v = scheduler.GetNext();
    if (!v.Timestamp.empty())
    {
      printf("Playing %s\n", v.ReqTrack.Title.c_str());
      db.RemoveVote(v.UserName, v.ReqTrack.TrackId);
      db.AddHistory(v.UserName, v.ReqTrack.TrackId);

      std::stringstream str;
      str << "mpg123 \"" << v.ReqTrack.Filename << "\"";
      printf("%s\n", str.str().c_str());
      if (system(str.str().c_str()))
      {
        // XXX Report this error to the database
      }
    }
    else
    {
      std::vector<Track> tracks;
      printf("No votes...playing random song\n");
      db.GetRandomTracks(&tracks, 1);
      db.AddHistory("neztu", tracks[0].TrackId);
      std::stringstream str;
      str << "mpg123 \"" << tracks[0].Filename << "\"";
      printf("%s\n", str.str().c_str());
      if (system(str.str().c_str()))
      {
        // XXX Report this error to the database
      }
    }
  }

  return 0;
}
