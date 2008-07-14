using System;
using System.Data;

namespace Neztu
{
  public struct Track
  {
    public uint TrackId;
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
    Track GetTrack(uint trackId);
    Track[] GetTracks(string title, string artist, string album);
    Track[] GetTracks();
    DataView GetTrackView();

    uint AddTrack(Track newTrack);
    void RemoveTrack(uint trackId);
  }

  public interface IRandomizableTrackDatabase : ITrackDatabase
  {
    Track GetRandom();
  }

  public interface IStateDatabase
  {
    Vote[] GetVotes();
    Vote[] GetVotes(string userName);
    void AddVote(string userName, uint trackId);
    void RemoveVote(string userName, uint trackId);
    void SwapVotes(Vote vote1, Vote vote2);

    Vote[] GetHistory();
    void AddHistory(string userName, uint trackId);
    Vote GetCurrent();
  }
}
