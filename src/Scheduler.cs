//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Defines the IScheduler and IRandomSelector interfaces, and provides
//      several implementations.
//
//-----------------------------------------------------------------------------

using System;

namespace Neztu
{
  public interface IScheduler
  {
    Vote GetNext();
    Vote[] GetSchedule();
  }

  public interface IRandomSelector
  {
    Track GetRandom();
  }

  public class FIFOScheduler : IScheduler
  {
    private INeztuDatabase m_database;

    public FIFOScheduler(INeztuDatabase database)
    {
      m_database = database;
    }

    public Vote GetNext()
    {
      Vote[] votes = m_database.GetVotes();
      if (votes.Length > 0)
      {
        return votes[0];
      }

      return null;
    }

    public Vote[] GetSchedule()
    {
      return m_database.GetVotes();
    }
  }

  /*public class FairTimeScheduler : IScheduler
  {
    private IRandomSelector m_randomSelector;
    private IStateDatabase m_database;

    public FairTimeScheduler(IRandomSelector randomSelector, IStateDatabase stateDb)
    {
      m_randomSelector = randomSelector;
      m_database = stateDb;
    }
  }*/

  public class FullyRandomSelector : IRandomSelector
  {
    private INeztuDatabase m_database;
    private bool m_dbIsRandomizable;
    private Random m_rng;

    public FullyRandomSelector(INeztuDatabase database)
    {
      if (database is IRandomizableTrackDatabase)
      {
        m_dbIsRandomizable = true;
      }
      else
      {
        m_rng = new Random();
      }

      m_database = database;
    }

    public Track GetRandom()
    {
      // FIXME: what if there are no tracks?
      if (m_dbIsRandomizable)
      {
        return ((IRandomizableTrackDatabase)m_database).GetRandomTracks(1)[0];
      }
      else
      {
        Track[] tracks = m_database.GetTracks();
        int r = m_rng.Next(tracks.Length);
        return tracks[r];
      }
    }
  }
}
