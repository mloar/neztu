//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implementation of the Database class.
//
//-----------------------------------------------------------------------------

#include <string.h>
#include "Database.h"

using namespace pqxx;

void PopulateTrack(Track *track, const result::tuple &tuple)
{
  // XXX lookup by column name does not appear to work
  for (size_t i = 0; i < tuple.size(); i++)
  {
    if (!strcmp(tuple[i].name(), "TrackId"))
    {
      tuple[i].to(track->TrackId);
    }
    else if (!strcmp(tuple[i].name(), "Filename"))
    {
      tuple[i].to(track->Filename);
    }
    else if (!strcmp(tuple[i].name(), "Title"))
    {
      tuple[i].to(track->Title);
    }
    else if (!strcmp(tuple[i].name(), "Artist"))
    {
      tuple[i].to(track->Artist);
    }
    else if (!strcmp(tuple[i].name(), "Album"))
    {
      tuple[i].to(track->Album);
    }
    else if (!strcmp(tuple[i].name(), "Genre"))
    {
      tuple[i].to(track->Genre);
    }
    else if (!strcmp(tuple[i].name(), "DiscNumber"))
    {
      tuple[i].to(track->DiscNumber);
    }
    else if (!strcmp(tuple[i].name(), "TrackNumber"))
    {
      tuple[i].to(track->TrackNumber);
    }
    else if (!strcmp(tuple[i].name(), "Length"))
    {
      tuple[i].to(track->Length);
    }
    else if (!strcmp(tuple[i].name(), "Uploader"))
    {
      tuple[i].to(track->Uploader);
    }
  }
}

void PopulateVote(Vote *vote, const result::tuple &tuple)
{
  // XXX lookup by column name does not appear to work
  for (size_t i = 0; i < tuple.size(); i++)
  {
    if (!strcmp(tuple[i].name(), "UserName"))
    {
      tuple[i].to(vote->UserName);
    }
    else if (!strcmp(tuple[i].name(), "Timestamp"))
    {
      tuple[i].to(vote->Timestamp);
    }
  }
  PopulateTrack(&vote->ReqTrack, tuple);
}

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
      PopulateTrack(m_track, R[0]);
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetRandomTracksFunctor : public pqxx::transactor<>
{
  std::vector<Track> *m_tracks;
  unsigned int m_count;

public:
  explicit GetRandomTracksFunctor(std::vector<Track> *tracks, unsigned int count) :
    m_tracks(tracks),
    m_count(count)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.prepared("random")(m_count).exec();
    m_tracks->resize(R.size());
    for (size_t i = 0; i < R.size(); i++)
    {
      PopulateTrack(&(*m_tracks)[i], R[i]);
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class AddTrackFunctor : public pqxx::transactor<>
{
  Track m_track;

public:
  explicit AddTrackFunctor(const Track &track) :
    m_track(track)
  {
  }

  void operator()(argument_type &T)
  {
    T.prepared("addtrack")(m_track.Filename)(m_track.Title)(m_track.Artist)(m_track.Album)(m_track.Genre)(m_track.DiscNumber)(m_track.TrackNumber)(m_track.Length)(m_track.Uploader).exec();
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
class AddHistoryFunctor : public pqxx::transactor<>
{
  const char *m_username;
  const char *m_trackId;

public:
  explicit AddHistoryFunctor(const char *username, const char *trackId) :
    m_username(username),
    m_trackId(trackId)
  {
  }

  void operator()(argument_type &T)
  {
    T.exec(std::string("INSERT INTO \"History\" VALUES('") + T.esc(m_username) + "', " + m_trackId + ")");
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class RemoveVoteFunctor : public pqxx::transactor<>
{
  const char *m_username;
  const char *m_trackId;

public:
  explicit RemoveVoteFunctor(const char *username, const char *trackId) :
    m_username(username),
    m_trackId(trackId)
  {
  }

  void operator()(argument_type &T)
  {
    T.exec(std::string("DELETE FROM \"Votes\" WHERE \"UserName\"='") + T.esc(m_username) + "' AND \"TrackId\"=" + m_trackId);
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class SwapVotesFunctor : public pqxx::transactor<>
{
  std::string m_username;
  std::string m_trackId1;
  std::string m_trackId2;

public:
  explicit SwapVotesFunctor(const std::string &username, unsigned int trackId1, unsigned int trackId2) :
    m_username(username)
  {
    std::stringstream trackId;
    trackId << trackId1;
    m_trackId1 = trackId.str();
    trackId.str(std::string());
    trackId << trackId2;
    m_trackId2 = trackId.str();
  }

  void operator()(argument_type &T)
  {
    std::string timestamp1, timestamp2;
    pqxx::result R = T.exec(std::string("SELECT \"Timestamp\" FROM \"Votes\" WHERE \"UserName\"='") + T.esc(m_username) + "' AND \"TrackId\"=" + m_trackId1);
    if (R.empty())
    {
      throw std::runtime_error("could not get timestamp");
    }
    R[0][0].to(timestamp1);

    R = T.exec(std::string("SELECT \"Timestamp\" FROM \"Votes\" WHERE \"UserName\"='") + T.esc(m_username) + "' AND \"TrackId\"=" + m_trackId2);
    if (R.empty())
    {
      throw std::runtime_error("could not get timestamp");
    }
    R[0][0].to(timestamp2);

    T.prepared("swap")(timestamp2)(m_username)(m_trackId1).exec();
    T.prepared("swap")(timestamp1)(m_username)(m_trackId2).exec();
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetCurrentFunctor : public pqxx::transactor<>
{
  Vote *m_vote;

public:
  explicit GetCurrentFunctor(Vote *vote) :
    m_vote(vote)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.exec(std::string("SELECT * FROM \"History\" LEFT JOIN \"Tracks\" ON \"History\".\"TrackId\"=\"Tracks\".\"TrackId\" ORDER BY \"Timestamp\" DESC LIMIT 1"));
    if (R.empty())
    {
      // XXX: throw exception?
      m_vote->ReqTrack.TrackId = 0;
    }
    else
    {
      PopulateVote(m_vote, R[0]);
    }
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
      PopulateTrack(m_track, R[0]);
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
        PopulateTrack(&(*m_tracks)[j], R[j]);
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
    m_title(title),
    m_artist(artist),
    m_album(album),
    m_tracks(tracks)
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
        PopulateTrack(&(*m_tracks)[j], R[j]);
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
    pqxx::result R = T.exec("SELECT * FROM \"Votes\" LEFT JOIN \"Tracks\" ON \"Votes\".\"TrackId\"=\"Tracks\".\"TrackId\" ORDER BY \"Timestamp\"");
    if (R.empty())
    {
      m_votes->resize(0);
    }
    else
    {
      m_votes->resize(R.size());
      for (size_t j = 0; j < R.size(); j++)
      {
        PopulateVote(&(*m_votes)[j], R[j]);
      }
    }
  }
};

// g++ won't let me make these function-local - not sure if I'm doing something wrong or not
class GetVotesForUserFunctor : public pqxx::transactor<>
{
  const char *m_username;
  std::vector<Vote> *m_votes;

public:
  explicit GetVotesForUserFunctor(std::vector<Vote> *votes, const char *username) :
    m_username(username),
    m_votes(votes)
  {
  }

  void operator()(argument_type &T)
  {
    pqxx::result R = T.exec(std::string("SELECT * FROM \"Votes\" LEFT JOIN \"Tracks\" ON \"Votes\".\"TrackId\"=\"Tracks\".\"TrackId\" WHERE \"UserName\"='") + T.esc(m_username) + "' ORDER BY \"Timestamp\"");
    if (R.empty())
    {
      m_votes->resize(0);
    }
    else
    {
      m_votes->resize(R.size());
      for (size_t j = 0; j < R.size(); j++)
      {
        PopulateVote(&(*m_votes)[j], R[j]);
      }
    }
  }
};

Database::Database(const Configuration &config) :
  m_conn(config.GetDatabaseConnectionString())
{
  m_conn.prepare("search", "SELECT * FROM \"Tracks\" WHERE \"Title\" ~* $1 AND \"Artist\" ~* $2 AND \"Album\" ~* $3")("varchar", prepare::treat_string)("varchar", prepare::treat_string)("varchar", prepare::treat_string);
  m_conn.prepare("swap", "UPDATE \"Votes\" SET \"Timestamp\"=$1 WHERE \"UserName\"=$2 AND \"TrackId\"=$3")("timestamp", prepare::treat_string)("varchar", prepare::treat_string)("integer", prepare::treat_direct);
  m_conn.prepare("addtrack", "INSERT INTO \"Tracks\" (\"Filename\", \"Title\", \"Artist\", \"Album\", \"Genre\", \"DiscNumber\", \"TrackNumber\", \"Length\", \"Uploader\") VALUES($1, $2, $3, $4, $5, $6, $7, $8, $9)")("varchar", prepare::treat_string)("varchar", prepare::treat_string)("varchar", prepare::treat_string)("varchar", prepare::treat_string)("varchar", prepare::treat_string)("integer", prepare::treat_direct)("integer", prepare::treat_direct)("integer", prepare::treat_direct)("varchar", prepare::treat_string);
  m_conn.prepare("random", "SELECT * FROM \"Tracks\" ORDER BY random() LIMIT $1")("integer", prepare::treat_direct);
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

void Database::GetVotes(std::vector<Vote> *out, const std::string &username)
{
  m_conn.perform(GetVotesForUserFunctor(out, username.c_str()));
}

unsigned int Database::AddTrack(const Track &track)
{
  // XXX this is supposed to return the assigned track id
  m_conn.perform(AddTrackFunctor(track));

  return 0;
}

void Database::AddVote(const std::string &username, unsigned int trackid)
{
  std::vector<char> trackId;
  trackId.resize(12);
  snprintf(&trackId[0], 12, "%d", trackid);

  m_conn.perform(AddVoteFunctor(username.c_str(), &trackId[0]));
}

void Database::AddHistory(const std::string &username, unsigned int trackid)
{
  std::vector<char> trackId;
  trackId.resize(12);
  snprintf(&trackId[0], 12, "%d", trackid);

  m_conn.perform(AddHistoryFunctor(username.c_str(), &trackId[0]));
}

void Database::RemoveVote(const std::string &username, unsigned int trackid)
{
  std::vector<char> trackId;
  trackId.resize(12);
  snprintf(&trackId[0], 12, "%d", trackid);

  m_conn.perform(RemoveVoteFunctor(username.c_str(), &trackId[0]));
}

void Database::SwapVotes(const std::string &username, unsigned int trackId1, unsigned int trackId2)
{
  m_conn.perform(SwapVotesFunctor(username, trackId1, trackId2));
}

Vote Database::GetCurrent()
{
  Vote v;
  m_conn.perform(GetCurrentFunctor(&v));
  return v;
}

void Database::GetRandomTracks(std::vector<Track> *tracks, unsigned int count)
{
  m_conn.perform(GetRandomTracksFunctor(tracks, count));
}
