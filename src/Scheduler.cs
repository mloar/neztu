using System;

namespace Neztu
{
  public interface IScheduler
  {
    void Initialize(IRandomSelector randomSelector, ITrackDatabase trackDb, IVoteDatabase voteDb,
        IHistoryDatabase historyDb);

    Track PlayNext();
    Vote[] GetSchedule()
  }

  public interface IRandomSelector
  {
    void Initialize(ITrackDatabase trackDb);
    Track GetRandom();
  }

  public class FIFOScheduler : IScheduler
  {
    private IRandomSelector m_randomSelector;
    private ITrackDatabase m_trackDatabase;
    private IVoteDatabase m_voteDatabase;
    private IHistoryDatabase m_historyDatabase;

    public void Initialize(IRandomSelector randomSelector, ITrackDatabase trackDb, IVoteDatabase voteDb,
        IHistoryDatabase historyDb)
    {
      m_randomSelector = randomSelector;
      m_trackDatabase = trackDb;
      m_voteDatabase = voteDb;
      m_historyDatabase = historyDb;
    }

    public Track PlayNext()
    {
      Track selected = m_randomSelector.GetRandom();

      Vote[] votes = m_voteDatabase.GetAll();
      if (votes.Length > 0)
      {
        selected = m_trackDatabase.GetTrack(votes[0].TrackId);
        m_voteDatabase.RemoveVote(votes[0].UserId, votes[0].TrackId);
        m_historyDatabase.AddPlay(votes[0].UserId, votes[0].TrackId);
      }
      else
      {
        m_historyDatabase.AddPlay(Guid.Empty, selected.TrackId);
      }

      return selected;
    }

    public Vote[] GetSchedule()
    {
      return m_voteDatabase.GetAll();
    }
  }

  public class FullyRandomSelector : IRandomSelector
  {
    private ITrackDatabase m_trackDatabase;
    private Random m_rng;

    public void Initialize(ITrackDatabase trackDb)
    {
      m_rng = new Random();
      m_trackDatabase = trackDb;
    }

    public Track GetRandom()
    {
      // FIXME: what if there are no tracks?
      Track[] tracks = m_trackDatabase.GetAll();
      int r = m_rng.Next(tracks.Length);
      return tracks[r];
    }
  }
}
