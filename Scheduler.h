//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the TagReader class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_SCHEDULER_H
#define NEZTU_SCHEDULER_H

#include <vector>
#include <list>

#include "Database.h"

namespace neztu
{
  class Scheduler
  {
    enum
    {
      Fifo,
      ShortestFirst
    } m_schedulingMethod;

    Database& m_db;
    std::vector<Vote> m_votes;
    std::list<std::vector<Vote>::iterator> m_schedule;

    public:
    Scheduler(const Configuration&, Database&);
    const Vote& GetNext();
    const std::list<std::vector<Vote>::iterator>& GetSchedule();
  };
}

#endif
