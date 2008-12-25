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
class SwapVotesFunctor : public pqxx::transactor<>
{
  Vote m_vote1;
  Vote m_vote2;

public:
  explicit SwapVotesFunctor(const Vote &vote1, const Vote &vote2) :
    m_vote1(vote1),
    m_vote2(vote2)
  {
  }

  void operator()(argument_type &T)
  {
    T.prepared("swap")(m_vote2.Timestamp)(m_vote1.UserName)(m_vote1.ReqTrack.TrackId).exec();
    T.prepared("swap")(m_vote1.Timestamp)(m_vote2.UserName)(m_vote2.ReqTrack.TrackId).exec();
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
    pqxx::result R = T.exec(std::string("SELECT * FROM \"Votes\" LEFT JOIN \"Tracks\" ON \"Votes\".\"TrackId\"=\"Tracks\".\"TrackId\" WHERE \"UserName\"='") + T.esc(m_username) + "'");
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

Database::Database() :
  m_conn("host=localhost user=neztu password=passw0rd dbname=neztu")
{
  m_conn.prepare("search", "SELECT * FROM \"Tracks\" WHERE \"Title\" ~* $1 AND \"Artist\" ~* $2 AND \"Album\" ~* $3")("varchar", prepare::treat_string)("varchar", prepare::treat_string)("varchar", prepare::treat_string);
  m_conn.prepare("swap", "UPDATE \"Votes\" SET \"Timestamp\"=$1 WHERE \"UserName\"=$2 AND \"TrackId\"=$3")("timestamp", prepare::treat_string)("varchar", prepare::treat_string)("integer", prepare::treat_direct);
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

void Database::AddVote(std::string username, unsigned int trackid)
{
  std::vector<char> trackId;
  trackId.resize(12);
  snprintf(&trackId[0], 12, "%d", trackid);

  m_conn.perform(AddVoteFunctor(username.c_str(), &trackId[0]));
}

void Database::SwapVotes(const Vote &vote1, const Vote &vote2)
{
  m_conn.perform(SwapVotesFunctor(vote1, vote2));
}
