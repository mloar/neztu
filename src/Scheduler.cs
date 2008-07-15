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
    private IStateDatabase m_stateDatabase;

    public FIFOScheduler(IRandomSelector randomSelector, IStateDatabase stateDb)
    {
      m_randomSelector = randomSelector;
      m_stateDatabase = stateDb;
    }

    public Track PlayNext()
    {
      Track selected = m_randomSelector.GetRandom();

      Vote[] votes = m_stateDatabase.GetVotes();
      if (votes.Length > 0)
      {
        selected = votes[0].ReqTrack;
        m_stateDatabase.RemoveVote(votes[0].UserName, votes[0].ReqTrack.TrackId);
        m_stateDatabase.AddHistory(votes[0].UserName, votes[0].ReqTrack.TrackId);
      }
      else
      {
        m_stateDatabase.AddHistory(string.Empty, selected.TrackId);
      }

      return selected;
    }

    public Vote[] GetSchedule()
    {
      return m_stateDatabase.GetVotes();
    }
  }

  public class FullyRandomSelector : IRandomSelector
  {
    private ITrackDatabase m_trackDatabase;
    private bool m_dbIsRandomizable;
    private Random m_rng;

    public FullyRandomSelector(ITrackDatabase trackDb)
    {
      if (trackDb is IRandomizableTrackDatabase)
      {
        m_dbIsRandomizable = true;
      }
      else
      {
        m_rng = new Random();
      }

      m_trackDatabase = trackDb;
    }

    public Track GetRandom()
    {
      // FIXME: what if there are no tracks?
      if (m_dbIsRandomizable)
      {
        return ((IRandomizableTrackDatabase)m_trackDatabase).GetRandom();
      }
      else
      {
        Track[] tracks = m_trackDatabase.GetTracks();
        int r = m_rng.Next(tracks.Length);
        return tracks[r];
      }
    }
  }
}
