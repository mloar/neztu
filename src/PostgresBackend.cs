//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements a PostgreSQL database backend.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace Neztu
{
  public class PostgresBackendException : Exception
  {
    public PostgresBackendException(string what) : base(what)
    {
    }
  }

  public class PostgresDatabase: INeztuDatabase, IRandomizableTrackDatabase, IPlayTimeTrackingDatabase
  {
    private const string m_votesTable = "Votes";
    private const string m_historyTable = "History";
    private const string m_pendingTable = "Pending";
    private const string m_tracksTable = "Tracks";
    private string m_connectionString = string.Empty;

    private Track GetTrackData(NpgsqlDataReader reader, int firstColumn)
    {
      Track ret = new Track();

      if (!reader.IsDBNull(firstColumn))
        ret.TrackId = (uint)(int)reader.GetValue(firstColumn);
      if (!reader.IsDBNull(firstColumn + 1))
        ret.Filename = (string)reader.GetValue(firstColumn + 1);
      if (!reader.IsDBNull(firstColumn + 2))
        ret.Artist = (string)reader.GetValue(firstColumn + 2);
      if (!reader.IsDBNull(firstColumn + 3))
        ret.Title = (string)reader.GetValue(firstColumn + 3);
      if (!reader.IsDBNull(firstColumn + 4))
        ret.Album = (string)reader.GetValue(firstColumn + 4);
      if (!reader.IsDBNull(firstColumn + 5))
        ret.Genre = (string)reader.GetValue(firstColumn + 5);
      if (!reader.IsDBNull(firstColumn + 6))
        ret.DiscNumber = (uint)(int)reader.GetValue(firstColumn + 6);
      if (!reader.IsDBNull(firstColumn + 7))
        ret.TrackNumber = (uint)(int)reader.GetValue(firstColumn + 7);
      if (!reader.IsDBNull(firstColumn + 8))
        ret.Length = new TimeSpan(0, 0, (int)reader.GetValue(firstColumn + 8));
      if (!reader.IsDBNull(firstColumn + 9))
        ret.Uploader = (string)reader.GetValue(firstColumn + 9);

      return ret;
    }

    public PostgresDatabase()
    {
      // Get connection string.
      string connStrName = ConfigurationManager.AppSettings["DatabaseConnectionStringName"];

      if (string.IsNullOrEmpty(connStrName))
      {
        throw new ArgumentOutOfRangeException("DatabaseConnectionStringName");
      }
      else
      {
        ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connStrName];

        if (ConnectionStringSettings == null || string.IsNullOrEmpty(ConnectionStringSettings.ConnectionString.Trim()))
        {
          throw new Exception("Connection String Empty");
        }

        m_connectionString = ConnectionStringSettings.ConnectionString;
      }
    }

    public Track GetTrack(uint trackId)
    {
      Track ret = null;

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" WHERE \"TrackId\" = @TrackId",
              m_tracksTable);

          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret = GetTrackData(reader, 0);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      return ret;
    }

    public Track GetTrack(string filename)
    {
      Track ret = null;

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" WHERE \"Filename\" = @Filename",
              m_tracksTable);

          dbCommand.Parameters.Add("@Filename", NpgsqlDbType.Varchar, 255).Value = filename;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret = GetTrackData(reader, 0);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      return ret;
    }

    public Track[] GetTracks()
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\"",
              m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret.Enqueue(GetTrackData(reader, 0));
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Track[] tracks = new Track[ret.Count];
      ret.CopyTo(tracks, 0);
      return tracks;
    }

    public Track[] GetRandomTracks(uint count)
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" ORDER BY random() LIMIT @Count",
              m_tracksTable);
          dbCommand.Parameters.Add("@Count", NpgsqlDbType.Integer, 0).Value = count;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret.Enqueue(GetTrackData(reader, 0));
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Track[] tracks = new Track[ret.Count];
      ret.CopyTo(tracks, 0);
      return tracks;
    }

    public Track[] GetTracks(string title, string artist, string album)
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" WHERE \"Title\" ~* @Title AND \"Artist\" ~* @Artist AND \"Album\" ~* @Album",
              m_tracksTable);

          dbCommand.Parameters.Add("@Title", NpgsqlDbType.Varchar, 255).Value = title;
          dbCommand.Parameters.Add("@Artist", NpgsqlDbType.Varchar, 255).Value = artist;
          dbCommand.Parameters.Add("@Album", NpgsqlDbType.Varchar, 255).Value = album;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret.Enqueue(GetTrackData(reader, 0));
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Track[] tracks = new Track[ret.Count];
      ret.CopyTo(tracks, 0);
      return tracks;
    }

    public uint AddTrack(Track newTrack)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand1 = dbConn.CreateCommand())
        {
          using (NpgsqlCommand dbCommand2 = dbConn.CreateCommand())
          {
            dbCommand1.CommandText = string.Format(
                "INSERT INTO \"{0}\" (\"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\") VALUES(@Filename, @Artist, @Title, @Album, @Genre, @DiscNumber, @TrackNumber, @Length, @Uploader)",
                m_tracksTable);

            dbCommand1.Parameters.Add("@Filename", NpgsqlDbType.Varchar, 255).Value = newTrack.Filename;
            dbCommand1.Parameters.Add("@Artist", NpgsqlDbType.Varchar, 255).Value = newTrack.Artist;
            dbCommand1.Parameters.Add("@Title", NpgsqlDbType.Varchar, 255).Value = newTrack.Title;
            dbCommand1.Parameters.Add("@Album", NpgsqlDbType.Varchar, 255).Value = newTrack.Album;
            dbCommand1.Parameters.Add("@Genre", NpgsqlDbType.Varchar, 255).Value = newTrack.Genre;
            dbCommand1.Parameters.Add("@DiscNumber", NpgsqlDbType.Integer, 0).Value = newTrack.DiscNumber;
            dbCommand1.Parameters.Add("@TrackNumber", NpgsqlDbType.Integer, 0).Value = newTrack.TrackNumber;
            dbCommand1.Parameters.Add("@Length", NpgsqlDbType.Integer, 0).Value = (uint)newTrack.Length.TotalSeconds;
            dbCommand1.Parameters.Add("@Uploader", NpgsqlDbType.Varchar, 255).Value = newTrack.Uploader;

            dbCommand2.CommandText = string.Format(
                "SELECT last_value FROM \"{0}_TrackId_seq\";",
                m_tracksTable);

            try
            {
              dbConn.Open();
              dbCommand1.Prepare();
              dbCommand2.Prepare();
              using (NpgsqlTransaction dbTrans = dbConn.BeginTransaction())
              {
                try
                {
                  dbCommand1.ExecuteNonQuery();
                  newTrack.TrackId = (uint)(Int64)dbCommand2.ExecuteScalar();
                  dbTrans.Commit();
                }
                catch (NpgsqlException e)
                {
                  dbTrans.Rollback();
                  Console.Error.WriteLine(e.ToString());
                  throw new PostgresBackendException("The database operation was aborted (see error log for details).");
                }
              }
            }
            catch (NpgsqlException e)
            {
              Console.Error.WriteLine(e.ToString());
              throw new PostgresBackendException("The database operation was aborted (see error log for details).");
            }
          }
        }
      }

      return newTrack.TrackId;
    }

    public void RemoveTrack(uint trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("DELETE FROM \"{0}\" WHERE \"TrackId\"=@TrackId)", m_tracksTable);

          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId.ToString();

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }
    }

    public Vote[] GetVotes(string userName)
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"Timestamp\", \"{0}\".\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" LEFT JOIN \"{1}\" ON \"{0}\".\"TrackId\"=\"{1}\".\"TrackId\" WHERE \"UserName\"=@UserName ORDER BY \"Timestamp\"",
              m_votesTable, m_tracksTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v = new Vote();
                v.UserName = (string)reader.GetValue(0);
                v.Timestamp = (DateTime)reader.GetValue(1);
                v.ReqTrack = GetTrackData(reader, 2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Vote[] votes = new Vote[ret.Count];
      ret.CopyTo(votes, 0);
      return votes;
    }

    public Vote[] GetVotes()
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"Timestamp\", \"{0}\".\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" LEFT JOIN \"{1}\" ON \"{0}\".\"TrackId\"=\"{1}\".\"TrackId\" ORDER BY \"Timestamp\"",
              m_votesTable, m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v = new Vote();
                v.UserName = (string)reader.GetValue(0);
                v.Timestamp = (DateTime)reader.GetValue(1);
                v.ReqTrack = GetTrackData(reader, 2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Vote[] votes = new Vote[ret.Count];
      ret.CopyTo(votes, 0);
      return votes;
    }

    public void AddVote(string userName, uint trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "INSERT INTO \"{0}\" (\"UserName\", \"TrackId\", \"Timestamp\") VALUES(@UserName, @TrackId, now())",
              m_votesTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }
    }

    public void RemoveVote(string userName, uint trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "DELETE FROM \"{0}\" WHERE \"UserName\"=@UserName AND \"TrackId\"=@TrackId",
              m_votesTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }
    }

    public void SwapVotes(Vote vote1, Vote vote2)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        try
        {
          dbConn.Open();
          using (NpgsqlCommand dbCommand1 = dbConn.CreateCommand())
          {
            using (NpgsqlCommand dbCommand2 = dbConn.CreateCommand())
            {
              dbCommand1.CommandText = string.Format(
                  "UPDATE \"{0}\" SET \"Timestamp\"=@Timestamp WHERE \"UserName\"=@UserName AND \"TrackId\"=@TrackId",
                  m_votesTable);
              dbCommand2.CommandText = string.Format(
                  "UPDATE \"{0}\" SET \"Timestamp\"=@Timestamp WHERE \"UserName\"=@UserName AND \"TrackId\"=@TrackId",
                  m_votesTable);

              dbCommand1.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = vote2.Timestamp;
              dbCommand1.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = vote1.UserName;
              dbCommand1.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = vote1.ReqTrack.TrackId;
              dbCommand2.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = vote1.Timestamp;
              dbCommand2.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = vote2.UserName;
              dbCommand2.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = vote2.ReqTrack.TrackId;

              dbCommand1.Prepare();
              dbCommand2.Prepare();

              using (NpgsqlTransaction dbTrans = dbConn.BeginTransaction())
              {
                try
                {
                  dbCommand1.ExecuteNonQuery();
                  dbCommand2.ExecuteNonQuery();
                  dbTrans.Commit();
                }
                catch (NpgsqlException e)
                {
                  dbTrans.Rollback();
                  Console.Error.WriteLine(e.ToString());
                  throw new PostgresBackendException("The database operation was aborted (see error log for details).");
                }
              }
            }
          }
        }
        catch (NpgsqlException e)
        {
          Console.Error.WriteLine(e.ToString());
          throw new PostgresBackendException("The database operation was aborted (see error log for details).");
        }
      }
    }

    public Vote[] GetPending()
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"Timestamp\", \"{0}\".\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" LEFT JOIN \"{1}\" ON \"{0}\".\"TrackId\"=\"{1}\".\"TrackId\" ORDER BY \"Timestamp\"",
              m_pendingTable, m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v = new Vote();
                v.UserName = (string)reader.GetValue(0);
                v.Timestamp = (DateTime)reader.GetValue(1);
                v.ReqTrack = GetTrackData(reader, 2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Vote[] votes = new Vote[ret.Count];
      ret.CopyTo(votes, 0);
      return votes;
    }

    public void AddPending(string userName, uint trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "INSERT INTO \"{0}\" (\"UserName\", \"TrackId\", \"Timestamp\") VALUES(@UserName, @TrackId, @Timestamp)",
              m_pendingTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }
    }

    public void RemovePending(string userName, uint trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "DELETE FROM \"{0}\" WHERE \"UserName\"=@UserName AND \"TrackId\"=@TrackId",
              m_pendingTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }
    }

    public Vote[] GetHistory()
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"Timestamp\", \"{0}\".\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" LEFT JOIN \"{1}\" ON \"{0}\".\"TrackId\"=\"{1}\".\"TrackId\" ORDER BY \"Timestamp\"",
              m_historyTable, m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v = new Vote();
                v.UserName = (string)reader.GetValue(0);
                v.Timestamp = (DateTime)reader.GetValue(1);
                v.ReqTrack = GetTrackData(reader, 2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      Vote[] votes = new Vote[ret.Count];
      ret.CopyTo(votes, 0);
      return votes;
    }


    public void AddHistory(string userName, uint trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "INSERT INTO \"{0}\" (\"UserName\", \"TrackId\", \"Timestamp\") VALUES(@UserName, @TrackId, @Timestamp)",
              m_historyTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Integer, 0).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }
    }

    public Vote GetCurrent()
    {
      Vote ret = null;

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"Timestamp\", \"{0}\".\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\" FROM \"{0}\" LEFT JOIN \"{1}\" ON \"{0}\".\"TrackId\"=\"{1}\".\"TrackId\" ORDER BY \"Timestamp\" DESC LIMIT 1",
              m_historyTable, m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret = new Vote();
                ret.UserName = (string)reader.GetValue(0);
                ret.Timestamp = (DateTime)reader.GetValue(1);
                ret.ReqTrack = GetTrackData(reader, 2);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      return ret;
    }

    public PlayTimeInfo[] GetPlayTime()
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", SUM(\"Length\") AS \"PlayTime\" FROM \"{0}\" LEFT JOIN \"{1}\" ON \"{0}\".\"TrackId\"=\"{1}\".\"TrackId\" GROUP BY (\"UserName\") ORDER BY \"PlayTime\"",
              m_historyTable, m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                PlayTimeInfo pti = new PlayTimeInfo();
                pti.UserName = (string)reader.GetValue(0);
                pti.PlayTime = new TimeSpan(0, 0, (int)reader.GetValue(1));
                ret.Enqueue(pti);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Console.Error.WriteLine(e.ToString());
            throw new PostgresBackendException("The database operation was aborted (see error log for details).");
          }
        }
      }

      PlayTimeInfo[] playtime = new PlayTimeInfo[ret.Count];
      ret.CopyTo(playtime, 0);
      return playtime;
    }
  }
}
