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

  public interface IDatabaseFactory
  {
    ITrackDatabase GetTrackDatabase();
    IStateDatabase GetStateDatabase();
  }

  public class DatabaseHelper
  {
    private static IDatabaseFactory GetFactory()
    {
      string factoryType = ConfigurationManager.AppSettings["DatabaseFactoryType"];
      if (factoryType == null)
      {
        throw new Exception("Database factory type not specified");
      }

      Type type = Type.GetType(factoryType);
      if (type == null)
      {
        throw new Exception("Could not find database factory type");
      }

      return (IDatabaseFactory)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
    }

    public static ITrackDatabase GetTrackDatabase()
    {
      return GetFactory().GetTrackDatabase();
    }

    public static IStateDatabase GetStateDatabase()
    {
      return GetFactory().GetStateDatabase();
    }
  }
}
