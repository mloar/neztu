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
    public Guid TrackId;
    public DateTime Timestamp;
  }

  interface ITrackDatabase
  {
    Track GetTrack(Guid trackId);
    Track[] GetTracks(string title, string artist, string album);
    Track[] GetAll();
    DataView GetViewofAll();

    Guid AddTrack(Track newTrack);
    void RemoveTrack(Guid trackId);
  }

  interface IVoteDatabase
  {
    Vote[] GetVotesByUser(Guid userId);
    Vote[] GetAll();

    void AddVote(Guid userId, Guid trackId);
  }
}
