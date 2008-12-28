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

  if (daemon(0, 0))
  {
    std::cerr << "Could not fork" << std::endl;
    return 1;
  }

  for (;;)
  {
    std::vector<Vote> votes;
    db.GetVotes(&votes);
    std::list<Vote> voteList(votes.begin(), votes.end());
    voteList.sort(Scheduler());
    if (!voteList.empty())
    {
      printf("Playing %s\n", voteList.begin()->ReqTrack.Title.c_str());
      db.RemoveVote(votes[0].UserName, voteList.begin()->ReqTrack.TrackId);
      db.AddHistory(votes[0].UserName, voteList.begin()->ReqTrack.TrackId);

      std::stringstream str;
      str << "mpg123 \"" << voteList.begin()->ReqTrack.Filename << "\"";
      printf("%s\n", str.str().c_str());
      if (system(str.str().c_str()))
      {
        // XXX Report this error to the database
      }
    }
    else
    {
      printf("No votes...sleeping 5 seconds\n");
      sleep(5);
    }
  }

  return 0;
}
