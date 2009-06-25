//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      TagReader stand-alone app.
//
//-----------------------------------------------------------------------------

#include <stdlib.h>

#include <iostream>

#include "Database.h"
#include "TagReader.h"

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
                TagReader::ProcessPath(db, path, std::cout);
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
