using System;
using System.Data;

namespace Neztu
{
  public struct Track
  {
    public Guid TrackId;
    public string Filename;
    public string Title;
    public string Artist;
    public string Album;
    public uint DiscNumber;
    public uint TrackNumber;
    public TimeSpan Length;
    public string UserName;
  }

  public struct Vote
  {
    public string UserName;
    public Track ReqTrack;
    public DateTime Timestamp;
  }

  public interface ITrackDatabase
  {
    Track GetTrack(Guid trackId);
    Track[] GetTracks(string title, string artist, string album);
    Track[] GetTracks();
    DataView GetTrackView();

    Guid AddTrack(Track newTrack);
    void RemoveTrack(Guid trackId);
  }

  public interface IRandomizableTrackDatabase : ITrackDatabase
  {
    Track GetRandom();
  }

  public interface IStateDatabase
  {
    void Initialize(ITrackDatabase trackDb);

    Vote[] GetVotes();
    Vote[] GetVotes(string userName);
    void AddVote(string userName, Guid trackId);
    void RemoveVote(string userName, Guid trackId);
    void SwapVotes(Vote vote1, Vote vote2);

    Vote[] GetHistory();
    void AddHistory(string userName, Guid trackId);
    Vote GetCurrent();
  }
}
