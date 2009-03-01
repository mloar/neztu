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
#include <string.h>

#include "Database.h"
#include "Scheduler.h"

#include <AL/alut.h>
#include <iostream>

#include "streamer.h"
#include "ogg.h"
#include "mp3.h"

using namespace neztu;

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
            throw string("Ogg refused to play.");

        while(stream->update())
        {
            if(!stream->playing())
            {
                if(!stream->playback())
                    throw string("Ogg abruptly stopped.");
                else
                    cout << "Ogg stream was interrupted.\n";
            }
        }

        cout << "Program normal termination.";
    }
    catch(string error)
    {
        cout << error;
        cin.get();
    }

    delete stream;

    alutExit();
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
