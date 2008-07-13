using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace Neztu
{
  public class PostgresTrackDatabase: IRandomizableTrackDatabase
  {
    private const string m_tracksTable = "Tracks";
    private string m_connectionString = string.Empty;

    public PostgresTrackDatabase()
    {
      // Get connection string.
      string connStrName = ConfigurationManager.AppSettings["TrackDatabaseConnectionStringName"];

      if (string.IsNullOrEmpty(connStrName))
      {
        /*throw new ArgumentOutOfRangeException("TrackDatabaseConnectionStringName",
          Properties.Resources.ErrArgumentNullOrEmpty);*/
        throw new ArgumentOutOfRangeException("TrackDatabaseConnectionStringName");
      }
      else
      {
        ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connStrName];

        if (ConnectionStringSettings == null || string.IsNullOrEmpty(ConnectionStringSettings.ConnectionString.Trim()))
        {
          //throw new NeztuException(Properties.Resources.ErrConnectionStringNullOrEmpty);
          throw new Exception("Connection String Empty");
        }

        m_connectionString = ConnectionStringSettings.ConnectionString;
      }
    }

    public Track GetTrack(Guid trackId)
    {
      Track ret;
      ret.TrackId = Guid.Empty;
      ret.Filename = string.Empty;
      ret.Title = string.Empty;
      ret.Artist = string.Empty;
      ret.Album = string.Empty;
      ret.DiscNumber = 0;
      ret.TrackNumber = 0;
      ret.Length = TimeSpan.Zero;
      ret.UserName = string.Empty;

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserName\" FROM \"{0}\" WHERE \"TrackId\" = @TrackId",
              m_tracksTable);

          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = trackId;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret.TrackId = new Guid((string)reader.GetValue(0));
                ret.Filename = (string)reader.GetValue(1);
                ret.Artist = (string)reader.GetValue(2);
                ret.Title = (string)reader.GetValue(3);
                ret.Album = (string)reader.GetValue(4);
                ret.DiscNumber = (uint)(int)reader.GetValue(5);
                ret.TrackNumber = (uint)(int)reader.GetValue(6);
                ret.Length = new TimeSpan(0, 0, (int)reader.GetValue(7));
                ret.UserName = (string)reader.GetValue(8);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          catch (System.InvalidCastException)
          {
            // Sigh
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
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
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserName\" FROM \"{0}\"",
              m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Track t;
                t.TrackId = new Guid((string)reader.GetValue(0));
                t.Filename = (string)reader.GetValue(1);
                if (!reader.IsDBNull(2))
                  t.Artist = (string)reader.GetValue(2);
                else
                  t.Artist = string.Empty;
                if (!reader.IsDBNull(3))
                  t.Title = (string)reader.GetValue(3);
                else
                  t.Title = string.Empty;
                if (!reader.IsDBNull(4))
                  t.Album = (string)reader.GetValue(4);
                else
                  t.Album = string.Empty;
                t.DiscNumber = (uint)(int)reader.GetValue(5);
                t.TrackNumber = (uint)(int)reader.GetValue(6);
                t.Length = new TimeSpan(0, 0, (int)reader.GetValue(7));
                t.UserName = (string)reader.GetValue(8);

                ret.Enqueue(t);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }

      Track[] tracks = new Track[ret.Count];
      ret.CopyTo(tracks, 0);
      return tracks;
    }

    public Track GetRandom()
    {
      Track ret;
      ret.TrackId = Guid.Empty;
      ret.Filename = string.Empty;
      ret.Title = string.Empty;
      ret.Artist = string.Empty;
      ret.Album = string.Empty;
      ret.DiscNumber = 0;
      ret.TrackNumber = 0;
      ret.Length = TimeSpan.Zero;
      ret.UserName = string.Empty;
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserName\" FROM \"{0}\" ORDER BY random() LIMIT 1",
              m_tracksTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret.TrackId = new Guid((string)reader.GetValue(0));
                ret.Filename = (string)reader.GetValue(1);
                if (!reader.IsDBNull(2))
                  ret.Artist = (string)reader.GetValue(2);
                else
                  ret.Artist = string.Empty;
                if (!reader.IsDBNull(3))
                  ret.Title = (string)reader.GetValue(3);
                else
                  ret.Title = string.Empty;
                if (!reader.IsDBNull(4))
                  ret.Album = (string)reader.GetValue(4);
                else
                  ret.Album = string.Empty;
                ret.DiscNumber = (uint)(int)reader.GetValue(5);
                ret.TrackNumber = (uint)(int)reader.GetValue(6);
                ret.Length = new TimeSpan(0, 0, (int)reader.GetValue(7));
                ret.UserName = (string)reader.GetValue(8);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }

      return ret;
    }

    public DataView GetTrackView()
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserName\" FROM \"{0}\"",
              m_tracksTable);

          using(NpgsqlDataAdapter dbAdapter = new NpgsqlDataAdapter(dbCommand))
          {

            try
            {
              dbConn.Open();
              DataSet dataSet = new DataSet();
              dbAdapter.Fill(dataSet, "Tracks");
              return new DataView(dataSet.Tables["Tracks"]);

            }
            catch (NpgsqlException e)
            {
              Trace.WriteLine(e.ToString());
              throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
            }
            finally
            {
              if (dbConn != null)
                dbConn.Close();
            }
          }
        }
      }
    }


    public Track[] GetTracks(string title, string artist, string album)
    {
      return null;
    }

    public Guid AddTrack(Track newTrack)
    {
      newTrack.TrackId = Guid.NewGuid();
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "INSERT INTO \"{0}\" (\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserName\") VALUES(@TrackId, @Filename, @Artist, @Title, @Album, @DiscNumber, @TrackNumber, @Length, @UserName)",
              m_tracksTable);

          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = newTrack.TrackId.ToString();
          dbCommand.Parameters.Add("@Filename", NpgsqlDbType.Varchar, 255).Value = newTrack.Filename;
          dbCommand.Parameters.Add("@Artist", NpgsqlDbType.Varchar, 255).Value = newTrack.Artist;
          dbCommand.Parameters.Add("@Title", NpgsqlDbType.Varchar, 255).Value = newTrack.Title;
          dbCommand.Parameters.Add("@Album", NpgsqlDbType.Varchar, 255).Value = newTrack.Album;
          dbCommand.Parameters.Add("@DiscNumber", NpgsqlDbType.Integer, 0).Value = newTrack.DiscNumber;
          dbCommand.Parameters.Add("@TrackNumber", NpgsqlDbType.Integer, 0).Value = newTrack.TrackNumber;
          dbCommand.Parameters.Add("@Length", NpgsqlDbType.Integer, 0).Value = (uint)newTrack.Length.TotalSeconds;
          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = newTrack.UserName;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }

      return newTrack.TrackId;
    }

    public void RemoveTrack(Guid trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("DELETE FROM \"{0}\" WHERE \"TrackId\"=@TrackId)", m_tracksTable);

          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = trackId.ToString();

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }
    }
  }

  public class PostgresStateDatabase : IStateDatabase
  {
    private const string m_votesTable = "Votes";
    private const string m_historyTable = "History";
    private string m_connectionString = string.Empty;
    private ITrackDatabase m_trackDatabase;

    public PostgresStateDatabase()
    {
      // Get connection string.
      string connStrName = ConfigurationManager.AppSettings["StateDatabaseConnectionStringName"];

      if (string.IsNullOrEmpty(connStrName))
      {
        /*throw new ArgumentOutOfRangeException("TrackDatabaseConnectionStringName",
          Properties.Resources.ErrArgumentNullOrEmpty);*/
        throw new ArgumentOutOfRangeException("StateDatabaseConnectionStringName");
      }
      else
      {
        ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connStrName];

        if (ConnectionStringSettings == null || string.IsNullOrEmpty(ConnectionStringSettings.ConnectionString.Trim()))
        {
          //throw new NeztuException(Properties.Resources.ErrConnectionStringNullOrEmpty);
          throw new Exception("Connection String Empty");
        }

        m_connectionString = ConnectionStringSettings.ConnectionString;
      }
    }

    public void Initialize(ITrackDatabase trackDb)
    {
      m_trackDatabase = trackDb;
    }

    public Vote[] GetVotes(string userName)
    {
      Queue ret = new Queue();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"TrackId\", \"Timestamp\" FROM \"{0}\" WHERE \"UserName\"=@UserName ORDER BY \"Timestamp\"",
              m_votesTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v;
                v.UserName = (string)reader.GetValue(0);
                v.ReqTrack = m_trackDatabase.GetTrack(new Guid((string)reader.GetValue(1)));
                v.Timestamp = (DateTime)reader.GetValue(2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
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
              "SELECT \"UserName\", \"TrackId\", \"Timestamp\" FROM \"{0}\" ORDER BY \"Timestamp\"",
              m_votesTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v;
                v.UserName = (string)reader.GetValue(0);
                v.ReqTrack = m_trackDatabase.GetTrack(new Guid((string)reader.GetValue(1)));
                v.Timestamp = (DateTime)reader.GetValue(2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }

      Vote[] votes = new Vote[ret.Count];
      ret.CopyTo(votes, 0);
      return votes;
    }

    public void AddVote(string userName, Guid trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "INSERT INTO \"{0}\" (\"UserName\", \"TrackId\", \"Timestamp\") VALUES(@UserName, @TrackId, @Timestamp)",
              m_votesTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }
    }

    public void RemoveVote(string userName, Guid trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "DELETE FROM \"{0}\" WHERE \"UserName\"=@UserName AND \"TrackId\"=@TrackId",
              m_votesTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
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
              dbCommand1.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = vote1.ReqTrack.TrackId;
              dbCommand2.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = vote1.Timestamp;
              dbCommand2.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 255).Value = vote2.UserName;
              dbCommand2.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = vote2.ReqTrack.TrackId;

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
                  Trace.WriteLine(e.ToString());
                  throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
                }
              }
            }
          }
        }
        catch (NpgsqlException e)
        {
          Trace.WriteLine(e.ToString());
          throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
        }
        finally
        {
          if (dbConn != null)
            dbConn.Close();
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
              "SELECT \"UserName\", \"TrackId\", \"Timestamp\" FROM \"{0}\" ORDER BY \"Timestamp\"",
              m_historyTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                Vote v;
                v.UserName = (string)reader.GetValue(0);
                v.ReqTrack = m_trackDatabase.GetTrack(new Guid((string)reader.GetValue(1)));
                v.Timestamp = (DateTime)reader.GetValue(2);

                ret.Enqueue(v);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }

      Vote[] votes = new Vote[ret.Count];
      ret.CopyTo(votes, 0);
      return votes;
    }

    public void AddHistory(string userName, Guid trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "INSERT INTO \"{0}\" (\"UserName\", \"TrackId\", \"Timestamp\") VALUES(@UserName, @TrackId, @Timestamp)",
              m_historyTable);

          dbCommand.Parameters.Add("@UserName", NpgsqlDbType.Varchar, 36).Value = userName;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = trackId;
          dbCommand.Parameters.Add("@Timestamp", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

          try
          {
            dbConn.Open();
            dbCommand.ExecuteNonQuery();
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }
    }

    public Vote GetCurrent()
    {
      Vote ret;
      ret.UserName = string.Empty;
      ret.ReqTrack.TrackId = Guid.Empty;
      ret.ReqTrack.Filename = string.Empty;
      ret.ReqTrack.Title = string.Empty;
      ret.ReqTrack.Artist = string.Empty;
      ret.ReqTrack.Album = string.Empty;
      ret.ReqTrack.DiscNumber = 0;
      ret.ReqTrack.TrackNumber = 0;
      ret.ReqTrack.Length = TimeSpan.Zero;
      ret.ReqTrack.UserName = string.Empty;
      ret.Timestamp = DateTime.MinValue;

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format(
              "SELECT \"UserName\", \"TrackId\", \"Timestamp\" FROM \"{0}\" ORDER BY \"Timestamp\" DESC LIMIT 1",
              m_historyTable);

          try
          {
            dbConn.Open();
            dbCommand.Prepare();

            using (NpgsqlDataReader reader = dbCommand.ExecuteReader())
            {
              while (reader.Read())
              {
                ret.UserName = (string)reader.GetValue(0);
                ret.ReqTrack = m_trackDatabase.GetTrack(new Guid((string)reader.GetValue(1)));
                ret.Timestamp = (DateTime)reader.GetValue(2);
              }
            }
          }
          catch (NpgsqlException e)
          {
            Trace.WriteLine(e.ToString());
            throw new Exception("borken");//ProviderException(Properties.Resources.ErrOperationAborted);
          }
          finally
          {
            if (dbConn != null)
              dbConn.Close();
          }
        }
      }

      return ret;
    }
  }
}
