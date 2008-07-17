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
    Track PlayNext();
    Vote[] GetSchedule();
  }

  public interface IRandomSelector
  {
    Track GetRandom();
  }

  public class FIFOScheduler : IScheduler
  {
    private IRandomSelector m_randomSelector;
    private INeztuDatabase m_database;

    public FIFOScheduler(IRandomSelector randomSelector, INeztuDatabase database)
    {
      m_randomSelector = randomSelector;
      m_database = database;
    }

    public Track PlayNext()
    {
      Track selected = m_randomSelector.GetRandom();

      Vote[] votes = m_database.GetVotes();
      if (votes.Length > 0)
      {
        selected = votes[0].ReqTrack;
        m_database.RemoveVote(votes[0].UserName, votes[0].ReqTrack.TrackId);
        m_database.AddHistory(votes[0].UserName, votes[0].ReqTrack.TrackId);
      }
      else
      {
        m_database.AddHistory(string.Empty, selected.TrackId);
      }

      return selected;
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
        return ((IRandomizableTrackDatabase)m_database).GetRandomTrack();
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
