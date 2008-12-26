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
#include "Database.h"
#include "Scheduler.h"

int main(int argc, char* argv[])
{
  Database db;

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
      sleep(voteList.begin()->ReqTrack.Length);
    }
    else
    {
      printf("No votes...sleeping 5 seconds\n");
      sleep(5);
    }
  }

  return 0;
}
