//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements the player daemon.
//
//-----------------------------------------------------------------------------

#include "Scheduler.h"
#include "Database.h"

struct LameScheduler
{
  bool operator()(const Vote &vote1, const Vote &vote2)
  {
    return vote1.Timestamp.compare(vote2.Timestamp);
  }
};

struct ShortestFirstScheduler
{
  bool operator()(const Vote &vote1, const Vote &vote2)
  {
    return vote1.ReqTrack.Length < vote2.ReqTrack.Length;
  }
};

bool Scheduler::operator()(const Vote &vote1, const Vote &vote2)
{
  return ShortestFirstScheduler()(vote1, vote2);
}
