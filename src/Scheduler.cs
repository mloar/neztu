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
    private IPlayTimeTrackingDatabase m_database;

    public FairTimeScheduler(IPlayTimeTrackingDatabase database)
    {
      m_database = database;
    }

    public Vote GetNext()
    {
      // TODO
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
    private IRandomizableTrackDatabase m_database;

    public FullyRandomSelector(IRandomizableTrackDatabase database)
    {
      m_database = database;
    }

    public Track GetRandom()
    {
      Track[] tracks = m_database.GetRandomTracks(1);
      if (tracks.Length > 0)
      {
        return tracks[0];
      }

      return null;
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

      object[] args = {GetDatabase()};
      try
      {
        return (IScheduler)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);
      }
      catch (MissingMethodException)
      {
        throw new Exception(
            string.Format(
              "Scheduler {0} is incompatible with database {1}",
              type.ToString(),
              args[0].GetType().ToString()
              )
            );
      }
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

      object[] args = {GetDatabase()};
      try
      {
        return (IRandomSelector)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);
      }
      catch (MissingMethodException)
      {
        throw new Exception(
            string.Format(
              "Scheduler {0} is incompatible with database {1}",
              type.ToString(),
              args[0].GetType().ToString()
              )
            );
      }
    }
  }
}
