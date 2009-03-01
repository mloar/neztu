//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements the player daemon.
//
//-----------------------------------------------------------------------------

#include <list>
#include <iostream>
#include <string>
#include <stdlib.h>
#include <unistd.h>
#include <signal.h>
#include <string.h>

#include "Database.h"
#include "Scheduler.h"
#include "Player.h"

using namespace neztu;

static volatile bool skip;

bool should_cancel()
{
    return skip;
}

void playit(const char *file)
{
    skip = false;
    try
    {
        Player player(file, should_cancel);
        player.Play();
    }
    catch (std::string error)
    {
        std::cerr << error << std::endl;
    }
}

void sig_handler(int signal)
{
    if (signal == SIGUSR1)
        skip = true;
}

int main(int argc, char* argv[])
{
  Configuration config("/etc/neztu.conf");
  Database db(config);
  Scheduler scheduler(config, db);

  // XXX: use getopt or something
  if ((argc < 2 || strcmp(argv[1], "-d")) && daemon(0, 0))
  {
    std::cerr << "Could not fork" << std::endl;
    return 1;
  }

  struct sigaction act;
  memset(&act, 0, sizeof(struct sigaction));
  act.sa_handler = sig_handler;
  if (sigaction(SIGUSR1, &act, NULL))
  {
      std::cerr << "sigaction failed" << std::endl;
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

      printf("%s\n", v.ReqTrack.Filename.c_str());
      playit(v.ReqTrack.Filename.c_str());
    }
    else
    {
      std::vector<Track> tracks;
      printf("No votes...playing random song\n");
      db.GetRandomTracks(&tracks, 1);
      db.AddHistory("neztu", tracks[0].TrackId);
      printf("%s\n", tracks[0].Filename.c_str());
      playit(tracks[0].Filename.c_str());
    }
  }

  return 0;
}
