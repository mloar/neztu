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
#include <stdlib.h>
#include <unistd.h>
#include <signal.h>
#include <string.h>

#include "Database.h"
#include "Scheduler.h"

#include <AL/alut.h>
#include <iostream>

#include "streamer.h"
#include "ogg.h"
#include "mp3.h"

using namespace neztu;

int skip = 0;

void playit(const char *file)
{
    streamer* stream;
    alutInit(NULL, NULL);

    if (mp3_stream::try_open(file))
    {
        stream = new mp3_stream;
    }
    else
    {
        stream = new ogg_stream;
    }

    try
    {
        stream->open(file);

        stream->display();

        if(!stream->playback())
            throw string("File refused to play.");

        while(stream->update() && !skip)
        {
            if(!stream->playing())
            {
                if(!stream->playback())
                    throw string("Playback abruptly stopped.");
                else
                    cout << "Playback was interrupted.\n";
            }
        }

        skip = 0;
    }
    catch(string error)
    {
        cout << error;
    }

    delete stream;

    alutExit();
}

void sig_handler(int signal)
{
    if (signal == SIGUSR1)
        skip = 1;
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
