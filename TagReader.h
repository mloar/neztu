//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the TagReader class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_TAGREADER_H
#define NEZTU_TAGREADER_H

#include <boost/filesystem.hpp>

namespace neztu
{
    class TagReader
    {
    public:
        static Track ReadFileTags(const std::string &file);
        static void ProcessPath(
            neztu::Database &db,
            const boost::filesystem::path &path,
            std::ostream &out
            );
    };
}

#endif
