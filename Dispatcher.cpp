//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements the pages of the FastCGI application.
//
//-----------------------------------------------------------------------------

#include <list>
#include <map>
#include <memory>
#include <iomanip>
#include <cgicc/Cgicc.h>
#include "cgicc/HTTPHTMLHeader.h"
#include "cgicc/HTMLClasses.h"

#include "FCgiIO.h"
#include "Database.h"
#include "Dispatcher.h"
#include "Scheduler.h"

using namespace cgicc;
using namespace std;

class NeztuHeading : public cgicc::MStreamable
{
  public:
    virtual void render(std::ostream& out) const
    {
      // Send HTTP header
      out << "Content-type: text/html; charset=utf-8\r\n\r\n";

      // Set up the HTML document
      out << HTMLDoctype() << endl;
      out << html() << head() << title("Neztu!") << cgicc::link(set("href", "resources/main.css").set("type", "text/css").set("rel", "stylesheet")) << head() << endl;
      out << body() << "<script type=\"text/javascript\" src=\"resources/Neztu.js\"></script>" << endl;

      out << "<div id=\"header\"><img src=\"resources/banner.jpg\" alt=\"Neztu!\">" << endl;
      out << "<table id=\"menu\">";
      out << "<tr><td><a href=\"./\">Home</a></td><td><a href=\"playlist\">Your Playlist</a></td><td><a href=\"search\">Search Tracks</a></td><td><a href=\"add\">Add Tracks</a></td></tr>";
      out << "</table></div>";
      out << "<div id=\"main\">";
    }
};

class NeztuFooter : public cgicc::MStreamable
{
  public:
    virtual void render(std::ostream& out) const
    {
      out << "</div>";
      out << "<div id=\"footer\"><div id=\"nowplaying\"><div id=\"nowplayingleft\">Now Playing:</div><div id=\"nowplayingright\"><span id=\"NowPlayingTitle\"></span><br><span id=\"NowPlayingArtist\"></span><br></div></div></div>";
      out << "<script type=\"text/javascript\">updateNowPlaying();</script>";
      out << body() << html();
    }
};

class Queue : public cgicc::MStreamable
{
  Database &m_db;

  public:
    Queue(Database &db) :
      m_db(db)
    {
    }

    virtual void render(std::ostream& out) const
    {
      std::vector<Vote> votes;
      m_db.GetVotes(&votes);
      std::list<Vote> voteList(votes.begin(), votes.end());
      voteList.sort(Scheduler());
      out << "<table border=\"1\" style=\"width:100%\"><caption>Queue</caption>" << endl;
      out << "<tr><th>Title</th><th>Artist</th><th>Album</th><th>Length</th><th>Requested By</th></tr>" << endl;
      for (std::list<Vote>::iterator iter = voteList.begin(); iter != voteList.end(); iter++)
      {
        out << "<tr><td>" << iter->ReqTrack.Title << "</td><td>" << iter->ReqTrack.Artist << "</td>";
        out << "<td>" << iter->ReqTrack.Album << "</td>";
        out << "<td>";
        out << setw(2) << setfill('0') << iter->ReqTrack.Length / 3600 << ":";
        out << setw(2) << setfill('0') << (iter->ReqTrack.Length % 3600) / 60 << ":";
        out << setw(2) << setfill('0') << (iter->ReqTrack.Length % 60) << "</td>";
        out << "<td>" << iter->UserName << "</td></tr>";
      }
      out << "</table>" << endl;
    }
};

void vote_handler(cgicc::FCgiIO &io, cgicc::Cgicc &cgi, Database &db)
{
  cgicc::form_iterator iter = cgi["trackId"];
  if (iter != cgi.getElements().end())
  {
    int trackId;
    std::stringstream str(cgi["trackId"]->getValue());
    str >> trackId;

    db.AddVote(cgi.getEnvironment().getRemoteUser(), trackId);
  }

  io << "Status: 204 No Content\r\n\r\n";
}

void index_handler(cgicc::FCgiIO &io, cgicc::Cgicc &cgi, Database &db)
{
  io << NeztuHeading() << endl;

  io << "<div id=\"queuedata\">" << Queue(db) << "</div>";

  io << NeztuFooter();
}

void playlist_handler(cgicc::FCgiIO &io, cgicc::Cgicc &cgi, Database &db)
{
  io << NeztuHeading() << endl;

  cgicc::form_iterator swap = cgi["swap"];
  if (swap != cgi.getElements().end() && swap->getValue() == "yes")
  {
    cgicc::form_iterator track1 = cgi["track1"], track2 = cgi["track2"];
    if (track1 != cgi.getElements().end() && track2 != cgi.getElements().end())
    {
      std::stringstream trackIdStr1(track1->getValue()), trackIdStr2(track2->getValue());
      unsigned int trackId1, trackId2;
      trackIdStr1 >> trackId1;
      trackIdStr2 >> trackId2;
      db.SwapVotes(cgi.getEnvironment().getRemoteUser(), trackId1, trackId2);
    }
  }

  cgicc::form_iterator remove = cgi["remove"];
  if (remove != cgi.getElements().end() && remove->getValue() == "yes")
  {
    cgicc::form_iterator track1 = cgi["track1"];
    if (track1 != cgi.getElements().end())
    {
      std::stringstream str(track1->getValue());
      unsigned int trackId;
      str >> trackId;
      db.RemoveVote(cgi.getEnvironment().getRemoteUser(), trackId);
    }
  }

  std::vector<Vote> votes;
  db.GetVotes(&votes, cgi.getEnvironment().getRemoteUser());

  io << "<table border=\"1\" style=\"width:100%\"><caption>Your Playlist</caption>" << endl;
  io << "<tr><th></th><th></th><th>Title</th><th>Artist</th><th>Length</th></tr>" << endl;
  unsigned int lastTrackId = 0;
  for (std::vector<Vote>::iterator iter = votes.begin(); iter != votes.end(); iter++)
  {
    io << "<tr><td>";
    if (lastTrackId)
    {
      io << "<a href=\"?swap=yes&track1=" << lastTrackId << "&track2=" << iter->ReqTrack.TrackId << "\">Up</a>";
    }
    io << "</td><td><a href=\"?remove=yes&track1=" << iter->ReqTrack.TrackId << "\">Remove</td>";
    io << "<td>" << iter->ReqTrack.Title << "</td>";
    io << "<td>" << iter->ReqTrack.Artist << "</td>";
    io << "<td>";
    io << setw(2) << setfill('0') << iter->ReqTrack.Length / 3600 << ":";
    io << setw(2) << setfill('0') << (iter->ReqTrack.Length % 3600) / 60 << ":";
    io << setw(2) << setfill('0') << (iter->ReqTrack.Length % 60) << "</td></tr>" << endl;

    lastTrackId = iter->ReqTrack.TrackId;
  }
  io << "</table>" << endl;

  io << NeztuFooter();
}

void search_handler(cgicc::FCgiIO &io, cgicc::Cgicc &cgi, Database &db)
{
  io << NeztuHeading() << endl;

  std::string title, artist, album;
  {
    cgicc::form_iterator iter = cgi["title"];
    if (iter != cgi.getElements().end())
    {
      title = iter->getValue();
    }
  }
  {
    cgicc::form_iterator iter = cgi["artist"];
    if (iter != cgi.getElements().end())
    {
      artist = iter->getValue();
    }
  }
  {
    cgicc::form_iterator iter = cgi["album"];
    if (iter != cgi.getElements().end())
    {
      album = iter->getValue();
    }
  }

  // XXX replace with cgicc stuff (maybe)
  io << "<div id=\"searchbox\">";
  io << "<form method=\"POST\">";
  io << "<table><tr><th>Title</th><th>Artist</th><th>Album</th><th></th></tr>";
  io << "<tr><td><input type=\"text\" name=\"title\" value=\"" << title << "\"></td>";
  io << "<td><input type=\"text\" name=\"artist\" value=\"" << artist << "\"></td>";
  io << "<td><input type=\"text\" name=\"album\" value=\"" << album << "\"></td>";
  io << "<td><input type=\"submit\" value=\"Search\"></td></tr></table>";
  io << "</form>";

  if (!title.empty() || !artist.empty() || !album.empty())
  {
    std::vector<Track> tracks;
    db.GetTracks(&tracks, title.c_str(), artist.c_str(), album.c_str());
    if (!tracks.empty())
    {
      io << "<table border=\"1\"><th></th><th>Title</th><th>Artist</th><th>Album</th></tr>";
      for (std::vector<Track>::iterator iter = tracks.begin(); iter != tracks.end(); iter++)
      {
        // XXX need a method of providing confirmation that vote was recorded
        io << "<tr><td><a href=\"vote?trackId=" << iter->TrackId << "\">Vote</a></td>";
        io << "<td>" << iter->Title << "</td><td>" << iter->Artist << "</td><td>" << iter->Album << "</td></tr>";
      }
      io << "</table>";
    }
    else
    {
      io << "<p>No tracks found.</p>";
    }
  }

  io << "</div>";

  io << NeztuFooter();
}

void test_handler(cgicc::FCgiIO &io, cgicc::Cgicc &cgi, Database &db)
{
  io << NeztuHeading() << endl;

  Track t = db.GetTrack(3200);
  io << t.Title.c_str() << " by " << t.Artist.c_str() << endl;
  t = db.GetTrack("/home/matt/winfiles/Music/Cat Power/Moon Pix/Cat Power_Moon Pix_11_Peking Saint.mp3");
  io << t.Title.c_str() << " by " << t.Artist.c_str() << endl;
  std::vector<Track> tracks;
  db.GetTracks(&tracks);
  io << tracks.size() << " tracks" << endl; 
  db.GetTracks(&tracks, "", "Cat Power", "");
  for (std::vector<Track>::iterator iter = tracks.begin(); iter != tracks.end(); iter++)
  {
    io << iter->Title << "<br>";
  }

  io << NeztuFooter();
}

void resource_handler(cgicc::FCgiIO &io, cgicc::Cgicc &cgi, Database&)
{
  // XXX improve this
  static std::map<std::string, std::string> m_contentTypes;
  if (m_contentTypes.empty())
  {
    m_contentTypes.insert(std::make_pair(".css", "text/css"));
    m_contentTypes.insert(std::make_pair(".jpg", "image/jpeg"));
    m_contentTypes.insert(std::make_pair(".js", "text/javascript"));
  }

  std::string path = cgi.getEnvironment().getPathTranslated();
  std::map<std::string, std::string>::iterator iter = m_contentTypes.find(path.substr(path.rfind('.')));
  if (iter == m_contentTypes.end())
  {
    throw std::runtime_error("unrecognized file type");
  }
  std::ifstream file(path.c_str(), std::ifstream::in | std::ifstream::binary);
  if (!file.is_open())
  {
    throw std::runtime_error("could not open resource file");
  }
  io << "Content-type: " << iter->second << "\r\n\r\n";
  io << file.rdbuf();
}

void nowplaying_handler(FCgiIO &io, Cgicc &cgi, Database &db)
{
  io << "Content-type: text/plain; charset=utf-8\r\n\r\n";

  Vote v = db.GetCurrent();
  tm tim;
  std::string timeout = "10000"; // arbitrary default
  if (strptime(v.Timestamp.c_str(), "%Y-%m-%d %H:%M:%S", &tim))
  {
    int diff = static_cast<int>(difftime(mktime(&tim), time(NULL))) + v.ReqTrack.Length;
    if (diff <= 0)
    {
      diff = 10000;
    }
    else
    {
      diff *= 1000;
    }

    std::stringstream strstream;
    strstream << diff;
    timeout = strstream.str();
  }

  io << Queue(db);
  io << v.ReqTrack.Title << "\n" << v.ReqTrack.Artist << "\n" << timeout;
}

Dispatcher::Dispatcher()
{
  m_paths.insert(std::make_pair("/vote", vote_handler));
  m_paths.insert(std::make_pair("/test", test_handler));
  m_paths.insert(std::make_pair("/playlist", playlist_handler));
  m_paths.insert(std::make_pair("/search", search_handler));
  m_paths.insert(std::make_pair("/nowplaying", nowplaying_handler));
  m_paths.insert(std::make_pair("/resources/banner.jpg", resource_handler));
  m_paths.insert(std::make_pair("/resources/main.css", resource_handler));
  m_paths.insert(std::make_pair("/resources/Neztu.js", resource_handler));
  m_paths.insert(std::make_pair("/", index_handler));
  m_paths.insert(std::make_pair("", index_handler));
}

void Dispatcher::Dispatch(cgicc::FCgiIO &io)
{
  try
  {
    cgicc::Cgicc cgi(&io);
    PathMap::iterator iter = m_paths.find(cgi.getEnvironment().getPathInfo());
    if (iter != m_paths.end())
    {
      iter->second(io, cgi, m_db);
    }
    else
    {
      // Send HTTP header
      io << HTTPHTMLHeader() << endl;

      // Set up the HTML document
      io << html() << head(title("Neztu!")) << endl;
      io << body() << endl;
      io << "Page not found" << endl;
      io << body() << html() << endl;
    }
  }
  catch(exception& e) {
    io.err() << e.what() << endl;
  }
}
