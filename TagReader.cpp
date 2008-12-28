//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements reading of audio file meta data.
//
//-----------------------------------------------------------------------------

#include <stdlib.h>
#include <taglib/fileref.h>
#include <taglib/tag.h>

#include "Database.h"
#include "TagReader.h"

Track TagReader::ReadFileTags(const std::string &file)
{
  using namespace TagLib;

  Track t;
  FileRef f(file.c_str());
  t.Filename = file;
  t.Title = f.tag()->title().to8Bit(true);
  t.Artist = f.tag()->artist().to8Bit(true);
  t.Album = f.tag()->album().to8Bit(true);
  t.Genre = f.tag()->genre().to8Bit(true);
  t.DiscNumber = 0;
  t.TrackNumber = f.tag()->track();
  t.Length = f.audioProperties()->length();

  return t;
}

// For the TagReader standalone app.
int main(int argc, char* argv[])
{
  if (argc < 2)
  {
    fprintf(stderr, "Usage: %s file...\n", argv[0]);
    return 1;
  }

  try
  {
    Configuration config("/etc/neztu.conf");
    Database db(config);

    for (int i = 1; i < argc; i++)
    {
      // XXX realpath() is a compatibility nightmare
      // Though at least POSIX has a realpath().  Ask a Windows filesystem
      // developer what the Windows equivalent of realpath() is sometime.  Their
      // answer will sadden you greatly.

      char *path = realpath(argv[i], NULL);
      if (path)
      {
        try
        {
          Track t;
          t = db.GetTrack(path);
          if (t.TrackId)
          {
            fprintf(stderr, "Already have %s\n", path);
          }
          else
          {
            t = TagReader::ReadFileTags(path);
            if (t.Title.empty())
            {
              fprintf(stderr, "%s has no title, not adding\n", path);
            }
            else
            {
              t.Uploader = "neztu";
              db.AddTrack(t);
              printf("Added %s\n", path);
            }
          }
        }
        catch (std::exception &e)
        {
          fprintf(stderr, "Something is wrong with %s: %s\n", path, e.what());
        }

        free(path);
      }
      else
      {
        fprintf(stderr, "%s does not appear to exist\n", argv[i]);
      }
    }
  }
  catch (std::exception &e)
  {
    fprintf(stderr, "could not connect to database: %s\n", e.what());
    return 1;
  }

  return 0;
}
