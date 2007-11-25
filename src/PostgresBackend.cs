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
  public class PostgresTrackDatabase: ITrackDatabase
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
      ret.UserId = Guid.Empty;

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserId\" FROM \"{0}\" WHERE \"TrackId\" = @TrackId", m_tracksTable);

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
                ret.Title = (string)reader.GetValue(2);
                ret.Artist = (string)reader.GetValue(3);
                ret.Album = (string)reader.GetValue(4);
                ret.DiscNumber = (uint)(int)reader.GetValue(5);
                ret.TrackNumber = (uint)(int)reader.GetValue(6);
                ret.Length = new TimeSpan(0, 0, (int)reader.GetValue(7));
                ret.UserId = new Guid((string)reader.GetValue(8));
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

    public Track[] GetAll()
    {
      Stack ret = new Stack();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserId\" FROM \"{0}\"", m_tracksTable);

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
                t.Artist = (string)reader.GetValue(2);
                t.Title = (string)reader.GetValue(3);
                t.Album = (string)reader.GetValue(4);
                t.DiscNumber = (uint)(int)reader.GetValue(5);
                t.TrackNumber = (uint)(int)reader.GetValue(6);
                t.Length = new TimeSpan(0, 0, (int)reader.GetValue(7));
                t.UserId = new Guid((string)reader.GetValue(8));

                ret.Push(t);
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

    public DataView GetViewofAll()
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("SELECT \"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserId\" FROM \"{0}\"", m_tracksTable);

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
          dbCommand.CommandText = string.Format("INSERT INTO \"{0}\" (\"TrackId\", \"Filename\", \"Artist\", \"Title\", \"Album\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"UserId\") VALUES(@TrackId, @Filename, @Artist, @Title, @Album, @DiscNumber, @TrackNumber, @Length, @UserId)", m_tracksTable);

          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = newTrack.TrackId.ToString();
          dbCommand.Parameters.Add("@Filename", NpgsqlDbType.Varchar, 255).Value = newTrack.Filename;
          dbCommand.Parameters.Add("@Artist", NpgsqlDbType.Varchar, 255).Value = newTrack.Artist;
          dbCommand.Parameters.Add("@Title", NpgsqlDbType.Varchar, 255).Value = newTrack.Title;
          dbCommand.Parameters.Add("@Album", NpgsqlDbType.Varchar, 255).Value = newTrack.Album;
          dbCommand.Parameters.Add("@DiscNumber", NpgsqlDbType.Integer, 0).Value = newTrack.DiscNumber;
          dbCommand.Parameters.Add("@TrackNumber", NpgsqlDbType.Integer, 0).Value = newTrack.TrackNumber;
          dbCommand.Parameters.Add("@Length", NpgsqlDbType.Integer, 0).Value = (uint)newTrack.Length.TotalSeconds;
          dbCommand.Parameters.Add("@UserId", NpgsqlDbType.Varchar, 36).Value = newTrack.UserId.ToString();

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

  public class PostgresVoteDatabase : IVoteDatabase
  {
    private const string m_votesTable = "Votes";
    private string m_connectionString = string.Empty;

    public PostgresVoteDatabase()
    {
      // Get connection string.
      string connStrName = ConfigurationManager.AppSettings["VoteDatabaseConnectionStringName"];

      if (string.IsNullOrEmpty(connStrName))
      {
        /*throw new ArgumentOutOfRangeException("TrackDatabaseConnectionStringName",
            Properties.Resources.ErrArgumentNullOrEmpty);*/
        throw new ArgumentOutOfRangeException("VoteDatabaseConnectionStringName");
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

    public Vote[] GetVotesByUser(Guid UserId)
    {
      return null;
    }

    public Vote[] GetAll()
    {
      Stack ret = new Stack();

      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("SELECT \"UserId\", \"TrackId\", \"Timestamp\" FROM \"{0}\"",
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
                v.UserId = new Guid((string)reader.GetValue(0));
                v.TrackId = new Guid((string)reader.GetValue(1));
                v.Timestamp = (DateTime)reader.GetValue(2);

                ret.Push(v);
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

    public void AddVote(Guid userId, Guid trackId)
    {
      using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
      {
        using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
        {
          dbCommand.CommandText = string.Format("INSERT INTO \"{0}\" (\"UserId\", \"TrackId\", \"Timestamp\") VALUES(@UserId, @TrackId, @Timestamp)", m_votesTable);

          dbCommand.Parameters.Add("@UserId", NpgsqlDbType.Varchar, 36).Value = userId;
          dbCommand.Parameters.Add("@TrackId", NpgsqlDbType.Varchar, 36).Value = trackId;
          dbCommand.Parameters.Add("@LastActivityDate", NpgsqlDbType.Timestamp, 255).Value = DateTime.Now;

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
}

