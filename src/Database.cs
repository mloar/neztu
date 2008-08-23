//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Contains structures and interfaces for the generic database interface.
//
//-----------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data;
using System.Reflection;

namespace Neztu
{
  public class Track
  {
    public uint TrackId;
    public string Filename;
    public string Title;
    public string Artist;
    public string Album;
    public string Genre;
    public uint DiscNumber;
    public uint TrackNumber;
    public TimeSpan Length;
    public string Uploader;
  }

  public class Vote
  {
    public string UserName;
    public Track ReqTrack;
    public DateTime Timestamp;
  }

  public interface INeztuDatabase
  {
    // Track management
    Track GetTrack(uint trackId);
    Track GetTrack(string filename);
    Track[] GetTracks(string title, string artist, string album);
    Track[] GetTracks();
    DataView GetTrackView();

    uint AddTrack(Track newTrack);
    void RemoveTrack(uint trackId);

    // Vote management
    Vote[] GetVotes();
    Vote[] GetVotes(string userName);
    void AddVote(string userName, uint trackId);
    void RemoveVote(string userName, uint trackId);
    void SwapVotes(Vote vote1, Vote vote2);

    // Pending management
    Vote[] GetPending();
    void AddPending(string userName, uint trackId);
    void RemovePending(string userName, uint trackId);

    // History management
    Vote[] GetHistory();
    void AddHistory(string userName, uint trackId);
    Vote GetCurrent();
  }

  public interface IRandomizableTrackDatabase : INeztuDatabase
  {
    Track[] GetRandomTracks(uint count);
  }

  public abstract partial class DatabaseHelper
  {
    public static INeztuDatabase GetDatabase()
    {
      string databaseType = ConfigurationManager.AppSettings["DatabaseType"];
      if (databaseType == null)
      {
        throw new Exception("Database type not specified");
      }

      Type type = Type.GetType(databaseType);
      if (type == null)
      {
        throw new Exception("Could not find database type");
      }

      return (INeztuDatabase)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
    }
  }
}
