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

namespace neztu
{
  // Not actually needed
  //struct FifoScheduler
  //{
  //  bool operator()(std::vector<Vote>::iterator vote1, std::vector<Vote>::iterator vote2)
  //  {
  //    return vote1->Timestamp.compare(vote2->Timestamp);
  //  }
  //};

  struct ShortestFirstScheduler
  {
    bool operator()(std::vector<Vote>::iterator vote1, std::vector<Vote>::iterator vote2)
    {
      return vote1->ReqTrack.Length < vote2->ReqTrack.Length;
    }
  };

  Scheduler::Scheduler(const Configuration &config, Database &db) :
    m_schedulingMethod(Fifo),
    m_db(db)
  {
    const std::string &scheduler = config.GetSchedulingMethod();

    if (!scheduler.compare("shortestfirst"))
    {
      m_schedulingMethod = ShortestFirst;
    }
  }

  const Vote& Scheduler::GetNext()
  {
    m_db.GetVotes(&m_votes);
    m_schedule.clear();
    if (!m_votes.empty())
    {
      // assign() doesn't work for this
      for (std::vector<Vote>::iterator iter = m_votes.begin(); iter != m_votes.end(); iter++)
      {
        m_schedule.push_back(iter);
      }
      switch (m_schedulingMethod)
      {
        case Fifo:
          // The database guarantees FIFO order, so do nothing.
          break;
        case ShortestFirst:
          m_schedule.sort(ShortestFirstScheduler());
          break;
        default:
          throw std::logic_error("invalid scheduling method");
      }
      return **m_schedule.begin();
    }
    else
    {
      m_votes.push_back(Vote());
      return m_votes[0];
    }
  }

  const std::list<std::vector<Vote>::iterator>& Scheduler::GetSchedule()
  {
    m_db.GetVotes(&m_votes);
    m_schedule.clear();
    // assign() doesn't work for this
    for (std::vector<Vote>::iterator iter = m_votes.begin(); iter != m_votes.end(); iter++)
    {
      m_schedule.push_back(iter);
    }
    switch (m_schedulingMethod)
    {
      case Fifo:
        // The database guarantees FIFO order, so do nothing.
        break;
      case ShortestFirst:
        m_schedule.sort(ShortestFirstScheduler());
        break;
      default:
        throw std::logic_error("invalid scheduling method");
    }

    return m_schedule;
  }
}
