//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements reading of audio file meta data.
//
//-----------------------------------------------------------------------------

#include <stdlib.h>
#include <boost/filesystem.hpp>
#include <taglib/fileref.h>
#include <taglib/tag.h>

#include "config.h"
#include "Database.h"
#include "TagReader.h"

namespace neztu
{
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
}

void ProcessPath(neztu::Database &db, const boost::filesystem::path &path)
{
    try
    {
        using namespace boost::filesystem;
        using namespace neztu;

        if (is_directory(path))
        {
            for (directory_iterator itr(path); itr!=directory_iterator(); ++itr)
            {
                ProcessPath(db, itr->path());
            }
        }
        else if (is_regular(path) &&
                (
                 path.string().find(".mp3") == path.string().length() - 4
                 || path.string().find(".ogg") == path.string().length() - 4)
                )
        {
            Track t;
            t = db.GetTrack(path.string().c_str());
            if (!t.TrackId)
            {
                t = TagReader::ReadFileTags(path.string().c_str());
                if (t.Title.empty())
                {
                    std::cerr << path << " has no title, not adding" << std::endl;
                }
                else
                {
                    t.Uploader = "neztu";
                    db.AddTrack(t);
                    std::cout << "Added " << path << std::endl;
                }
            }
        }
    }
    catch (boost::filesystem::filesystem_error &e)
    {
        fprintf(
                stderr,
                "Filesystem error in %s: %d\n",
                e.what(),
                e.system_error()
               );
    }
    catch (std::exception &e)
    {
        fprintf(
                stderr,
                "Unknown error: %s\n",
                e.what()
               );
    }
}

// For the TagReader standalone app.
int main(int argc, char* argv[])
{
    using namespace neztu;

    if (argc < 2)
    {
        fprintf(stderr, "Usage: %s file...\n", argv[0]);
        return 1;
    }

    try
    {
        Configuration config(SYSCONFDIR "/neztu.conf");
        Database db(config);

        for (int i = 1; i < argc; i++)
        {
            // XXX realpath() is a compatibility nightmare
            char *path = realpath(argv[i], NULL);
            if (path)
            {
              ProcessPath(db, path);
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
