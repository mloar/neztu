//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Defines the data model and Database class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_DATABASE_H
#define NEZTU_DATABASE_H

#include <string>
#include <vector>
#include <pqxx/pqxx>

#include "Configuration.h"

namespace neztu
{
  class Track
  {
    public:
      unsigned int TrackId;
      std::string Filename;
      std::string Title;
      std::string Artist;
      std::string Album;
      std::string Genre;
      unsigned int DiscNumber;
      unsigned int TrackNumber;
      unsigned int Length;
      std::string Uploader;
  };

  class Vote
  {
    public:
      std::string UserName;
      Track ReqTrack;
      std::string Timestamp;
  };

  class Database
  {
    public:
      Database(const Configuration &config);

      Track GetTrack(unsigned int TrackId);
      Track GetTrack(const std::string &filename);
      void GetTracks(std::vector<Track> *out);
      void GetTracks(std::vector<Track> *out, const std::string &title, const std::string &artist, const std::string &album);
      void GetRandomTracks(std::vector<Track> *out, unsigned int count);

      unsigned int AddTrack(const Track &newTrack);
      void RemoveTrack(unsigned int trackId);

      void GetVotes(std::vector<Vote> *out);
      void GetVotes(std::vector<Vote> *out, const std::string &username);
      void AddVote(const std::string &username, unsigned int trackId);
      void RemoveVote(const std::string &username, unsigned int trackId);
      void SwapVotes(const std::string &username, unsigned int trackId1, unsigned int trackId2);

      void GetHistory(std::vector<Vote> *out);
      void AddHistory(const std::string &username, unsigned int trackId);
      Vote GetCurrent();

    private:
      pqxx::connection m_conn;
  };
}

#endif
