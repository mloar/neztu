//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Defines the TagReader class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_SCHEDULER_H
#define NEZTU_SCHEDULER_H

#include "Database.h"

struct Scheduler
{
    bool operator()(const Vote&, const Vote&);
};

#endif
