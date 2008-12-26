//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Defines the TagReader class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_TAGREADER_H
#define NEZTU_TAGREADER_H

class TagReader
{
  public:
    static Track ReadFileTags(const std::string &file);
};

#endif