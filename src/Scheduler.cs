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
using System.Configuration;
using System.Reflection;

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

  public class FairTimeScheduler : IScheduler
  {
    private INeztuDatabase m_database;

    public FairTimeScheduler(INeztuDatabase database)
    {
      m_database = database;
    }

    public Vote GetNext()
    {
      // TODO

      Vote[] votes = m_database.GetVotes();
      if (votes.Length > 0)
      {
        return votes[0];
      }

      return null;
    }

    public Vote[] GetSchedule()
    {
      // TODO

      return m_database.GetVotes();
    }
  }

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

  public abstract partial class DatabaseHelper
  {
    public static IScheduler GetScheduler()
    {
      string schedulerType = ConfigurationManager.AppSettings["SchedulerType"];
      if (schedulerType == null)
      {
        throw new Exception("Scheduler type not specified");
      }

      Type type = Type.GetType(schedulerType);
      if (type == null)
      {
        throw new Exception("Could not find scheduler type");
      }

      object[] args = {(INeztuDatabase)GetDatabase()};
      return (IScheduler)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);
    }

    public static IRandomSelector GetRandomSelector()
    {
      string randomSelectorType = ConfigurationManager.AppSettings["RandomSelectorType"];
      if (randomSelectorType == null)
      {
        throw new Exception("Random selector type not specified");
      }

      Type type = Type.GetType(randomSelectorType);
      if (type == null)
      {
        throw new Exception("Could not find random selector type");
      }

      object[] args = {(INeztuDatabase)GetDatabase()};
      return (IRandomSelector)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);
    }
  }
}
