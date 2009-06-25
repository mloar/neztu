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

    void TagReader::ProcessPath(
        neztu::Database &db,
        const boost::filesystem::path &path,
        std::ostream &out
        )
    {
        try
        {
            using namespace boost::filesystem;
            using namespace neztu;

            if (is_directory(path))
            {
                for (directory_iterator itr(path); itr!=directory_iterator(); ++itr)
                {
                    TagReader::ProcessPath(db, itr->path(), out);
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
                        out << path << " has no title, not adding" << std::endl;
                    }
                    else
                    {
                        t.Uploader = "neztu";
                        db.AddTrack(t);
                        out << "Added " << path << std::endl;
                    }
                }
            }
        }
        catch (boost::filesystem::filesystem_error &e)
        {
            fprintf(
                stderr,
                "Filesystem error: %s\n",
                e.what()
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
}
