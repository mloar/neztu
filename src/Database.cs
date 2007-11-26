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
    public Guid UserId;
  }

  public struct Vote
  {
    public Guid UserId;
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
    Vote[] GetVotes(Guid userId);
    void AddVote(Guid userId, Guid trackId);
    void RemoveVote(Guid userId, Guid trackId);

    Vote[] GetHistory();
    void AddHistory(Guid userId, Guid trackId);
  }
}
