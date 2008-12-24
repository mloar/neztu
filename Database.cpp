#include <string.h>
#include "Database.h"

using namespace pqxx;

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetTrackFunctor : public pqxx::transactor<>
{
  Track *m_track;
  const char *m_trackId;

public:
  explicit GetTrackFunctor(Track *track, const char *trackId) :
    m_track(track),
    m_trackId(trackId)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.exec(std::string("SELECT * FROM \"Tracks\" WHERE \"TrackId\"=") + m_trackId);
    if (R.empty())
    {
      // XXX: throw exception?
      m_track->TrackId = 0;
    }
    else
    {
      // XXX lookup by column name does not appear to work
      for (size_t i = 0; i < R[0].size(); i++)
      {
        if (!strcmp(R[0][i].name(), "TrackId"))
        {
          R[0][i].to(m_track->TrackId);
        }
        else if (!strcmp(R[0][i].name(), "Filename"))
        {
          R[0][i].to(m_track->Filename);
        }
        else if (!strcmp(R[0][i].name(), "Title"))
        {
          R[0][i].to(m_track->Title);
        }
        else if (!strcmp(R[0][i].name(), "Artist"))
        {
          R[0][i].to(m_track->Artist);
        }
        else if (!strcmp(R[0][i].name(), "Album"))
        {
          R[0][i].to(m_track->Album);
        }
        else if (!strcmp(R[0][i].name(), "Genre"))
        {
          R[0][i].to(m_track->Genre);
        }
        else if (!strcmp(R[0][i].name(), "DiscNumber"))
        {
          R[0][i].to(m_track->DiscNumber);
        }
        else if (!strcmp(R[0][i].name(), "TrackNumber"))
        {
          R[0][i].to(m_track->TrackNumber);
        }
        else if (!strcmp(R[0][i].name(), "Length"))
        {
          R[0][i].to(m_track->Length);
        }
        else if (!strcmp(R[0][i].name(), "Uploader"))
        {
          R[0][i].to(m_track->Uploader);
        }
      }
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class AddVoteFunctor : public pqxx::transactor<>
{
  const char *m_username;
  const char *m_trackId;

public:
  explicit AddVoteFunctor(const char *username, const char *trackId) :
    m_username(username),
    m_trackId(trackId)
  {
  }

  void operator()(argument_type &T)
  {
    T.exec(std::string("INSERT INTO \"Votes\" VALUES('") + T.esc(m_username) + "', " + m_trackId + ")");
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetTrackByFilenameFunctor : public pqxx::transactor<>
{
  Track *m_track;
  const char *m_filename;

public:
  explicit GetTrackByFilenameFunctor(Track *track, const char *filename) :
    m_track(track),
    m_filename(filename)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.exec(std::string("SELECT * FROM \"Tracks\" WHERE \"Filename\"='") + T.esc(m_filename) + "'");
    if (R.empty())
    {
      // XXX: throw exception?
      m_track->TrackId = 0;
    }
    else
    {
      // XXX lookup by column name does not appear to work
      for (size_t i = 0; i < R[0].size(); i++)
      {
        if (!strcmp(R[0][i].name(), "TrackId"))
        {
          R[0][i].to(m_track->TrackId);
        }
        else if (!strcmp(R[0][i].name(), "Filename"))
        {
          R[0][i].to(m_track->Filename);
        }
        else if (!strcmp(R[0][i].name(), "Title"))
        {
          R[0][i].to(m_track->Title);
        }
        else if (!strcmp(R[0][i].name(), "Artist"))
        {
          R[0][i].to(m_track->Artist);
        }
        else if (!strcmp(R[0][i].name(), "Album"))
        {
          R[0][i].to(m_track->Album);
        }
        else if (!strcmp(R[0][i].name(), "Genre"))
        {
          R[0][i].to(m_track->Genre);
        }
        else if (!strcmp(R[0][i].name(), "DiscNumber"))
        {
          R[0][i].to(m_track->DiscNumber);
        }
        else if (!strcmp(R[0][i].name(), "TrackNumber"))
        {
          R[0][i].to(m_track->TrackNumber);
        }
        else if (!strcmp(R[0][i].name(), "Length"))
        {
          R[0][i].to(m_track->Length);
        }
        else if (!strcmp(R[0][i].name(), "Uploader"))
        {
          R[0][i].to(m_track->Uploader);
        }
      }
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetTracksFunctor : public pqxx::transactor<>
{
  std::vector<Track> *m_tracks;

public:
  explicit GetTracksFunctor(std::vector<Track> *tracks) :
    m_tracks(tracks)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.exec("SELECT * FROM \"Tracks\"");
    if (R.empty())
    {
      m_tracks->resize(0);
    }
    else
    {
      m_tracks->resize(R.size());
      for (size_t j = 0; j < R.size(); j++)
      {
        // XXX lookup by column name does not appear to work
        for (size_t i = 0; i < R[j].size(); i++)
        {
          if (!strcmp(R[j][i].name(), "TrackId"))
          {
            R[j][i].to((*m_tracks)[j].TrackId);
          }
          else if (!strcmp(R[j][i].name(), "Filename"))
          {
            R[j][i].to((*m_tracks)[j].Filename);
          }
          else if (!strcmp(R[j][i].name(), "Title"))
          {
            R[j][i].to((*m_tracks)[j].Title);
          }
          else if (!strcmp(R[j][i].name(), "Artist"))
          {
            R[j][i].to((*m_tracks)[j].Artist);
          }
          else if (!strcmp(R[j][i].name(), "Album"))
          {
            R[j][i].to((*m_tracks)[j].Album);
          }
          else if (!strcmp(R[j][i].name(), "Genre"))
          {
            R[j][i].to((*m_tracks)[j].Genre);
          }
          else if (!strcmp(R[j][i].name(), "DiscNumber"))
          {
            R[j][i].to((*m_tracks)[j].DiscNumber);
          }
          else if (!strcmp(R[j][i].name(), "TrackNumber"))
          {
            R[j][i].to((*m_tracks)[j].TrackNumber);
          }
          else if (!strcmp(R[j][i].name(), "Length"))
          {
            R[j][i].to((*m_tracks)[j].Length);
          }
          else if (!strcmp(R[j][i].name(), "Uploader"))
          {
            R[j][i].to((*m_tracks)[j].Uploader);
          }
        }
      }
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class SearchTracksFunctor : public pqxx::transactor<>
{
  const char *m_title;
  const char *m_artist;
  const char *m_album;
  std::vector<Track> *m_tracks;

public:
  explicit SearchTracksFunctor(std::vector<Track> *tracks, const char *title, const char *artist, const char *album) :
    m_tracks(tracks),
    m_title(title),
    m_artist(artist),
    m_album(album)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.prepared("search")(m_title)(m_artist)(m_album).exec();
    if (R.empty())
    {
      m_tracks->resize(0);
    }
    else
    {
      m_tracks->resize(R.size());
      for (size_t j = 0; j < R.size(); j++)
      {
        // XXX lookup by column name does not appear to work
        for (size_t i = 0; i < R[j].size(); i++)
        {
          if (!strcmp(R[j][i].name(), "TrackId"))
          {
            R[j][i].to((*m_tracks)[j].TrackId);
          }
          else if (!strcmp(R[j][i].name(), "Filename"))
          {
            R[j][i].to((*m_tracks)[j].Filename);
          }
          else if (!strcmp(R[j][i].name(), "Title"))
          {
            R[j][i].to((*m_tracks)[j].Title);
          }
          else if (!strcmp(R[j][i].name(), "Artist"))
          {
            R[j][i].to((*m_tracks)[j].Artist);
          }
          else if (!strcmp(R[j][i].name(), "Album"))
          {
            R[j][i].to((*m_tracks)[j].Album);
          }
          else if (!strcmp(R[j][i].name(), "Genre"))
          {
            R[j][i].to((*m_tracks)[j].Genre);
          }
          else if (!strcmp(R[j][i].name(), "DiscNumber"))
          {
            R[j][i].to((*m_tracks)[j].DiscNumber);
          }
          else if (!strcmp(R[j][i].name(), "TrackNumber"))
          {
            R[j][i].to((*m_tracks)[j].TrackNumber);
          }
          else if (!strcmp(R[j][i].name(), "Length"))
          {
            R[j][i].to((*m_tracks)[j].Length);
          }
          else if (!strcmp(R[j][i].name(), "Uploader"))
          {
            R[j][i].to((*m_tracks)[j].Uploader);
          }
        }
      }
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetVotesFunctor : public pqxx::transactor<>
{
  std::vector<Vote> *m_votes;

public:
  explicit GetVotesFunctor(std::vector<Vote> *votes) :
    m_votes(votes)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.exec("SELECT * FROM \"Votes\" LEFT JOIN \"Tracks\" ON \"Votes\".\"TrackId\"=\"Tracks\".\"TrackId\"");
    if (R.empty())
    {
      m_votes->resize(0);
    }
    else
    {
      m_votes->resize(R.size());
      for (size_t j = 0; j < R.size(); j++)
      {
        // XXX lookup by column name does not appear to work
        for (size_t i = 0; i < R[j].size(); i++)
        {
          if (!strcmp(R[j][i].name(), "UserName"))
          {
            R[j][i].to((*m_votes)[j].UserName);
          }
          else if (!strcmp(R[j][i].name(), "Timestamp"))
          {
            R[j][i].to((*m_votes)[j].Timestamp);
          }
          else if (!strcmp(R[j][i].name(), "TrackId"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.TrackId);
          }
          else if (!strcmp(R[j][i].name(), "Filename"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Filename);
          }
          else if (!strcmp(R[j][i].name(), "Title"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Title);
          }
          else if (!strcmp(R[j][i].name(), "Artist"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Artist);
          }
          else if (!strcmp(R[j][i].name(), "Album"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Album);
          }
          else if (!strcmp(R[j][i].name(), "Genre"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Genre);
          }
          else if (!strcmp(R[j][i].name(), "DiscNumber"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.DiscNumber);
          }
          else if (!strcmp(R[j][i].name(), "TrackNumber"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.TrackNumber);
          }
          else if (!strcmp(R[j][i].name(), "Length"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Length);
          }
          else if (!strcmp(R[j][i].name(), "Uploader"))
          {
            R[j][i].to((*m_votes)[j].ReqTrack.Uploader);
          }
        }
      }
    }
  }
};

Database::Database() :
  m_conn("host=localhost user=neztu password=passw0rd dbname=neztu")
{
  m_conn.prepare("search", "SELECT * FROM \"Tracks\" WHERE \"Title\" ~* $1 AND \"Artist\" ~* $2 AND \"Album\" ~* $3")("varchar", prepare::treat_string)("varchar", prepare::treat_string)("varchar", prepare::treat_string);
}

Track Database::GetTrack(unsigned int TrackId)
{
  Track t;
  std::vector<char> trackId;
  trackId.resize(12);
  snprintf(&trackId[0], 12, "%d", TrackId);

  m_conn.perform(GetTrackFunctor(&t, &trackId[0]));
  return t;
}

Track Database::GetTrack(const std::string &filename)
{
  Track t;
  m_conn.perform(GetTrackByFilenameFunctor(&t, filename.c_str()));
  return t;
}

void Database::GetTracks(std::vector<Track> *out)
{
  m_conn.perform(GetTracksFunctor(out));
}

void Database::GetTracks(std::vector<Track> *out, const std::string &title, const std::string &artist, const std::string &album)
{
  m_conn.perform(SearchTracksFunctor(out, title.c_str(), artist.c_str(), album.c_str()));
}

void Database::GetVotes(std::vector<Vote> *out)
{
  m_conn.perform(GetVotesFunctor(out));
}

void Database::AddVote(std::string username, unsigned int trackid)
{
  std::vector<char> trackId;
  trackId.resize(12);
  snprintf(&trackId[0], 12, "%d", trackid);

  m_conn.perform(AddVoteFunctor(username.c_str(), &trackId[0]));
}
