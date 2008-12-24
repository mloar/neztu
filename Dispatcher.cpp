#include <map>
#include <memory>
#include <cgicc/Cgicc.h>
#include "cgicc/HTTPHTMLHeader.h"
#include "cgicc/HTMLClasses.h"

#include "FCgiIO.h"
#include "Dispatcher.h"
#include "Database.h"

using namespace cgicc;
using namespace std;

class NeztuHeading : public cgicc::MStreamable
{
  public:
    virtual void render(std::ostream& out) const
    {
      // Send HTTP header
      out << HTTPHTMLHeader() << endl;

      // Set up the HTML document
      out << HTMLDoctype() << endl;
      out << html() << head(title("Neztu!")).add(cgicc::link(set("href", "resources/main.css").set("type", "text/css").set("rel", "stylesheet"))) << endl;
      out << body() << endl;

      out << "<div id=\"header\"><img src=\"resources/banner.jpg\" alt=\"Neztu!\"></div>" << endl;
    }
};

template<typename _CharT, typename _Traits>
  inline basic_ostream<_CharT, _Traits>&
operator<<(basic_ostream<_CharT, _Traits>& __os, const NeztuHeading &heading)
{
  heading.render(__os);
  return __os;
}

void vote_handler(cgicc::FCgiIO &io)
{
  cgicc::Cgicc cgi(&io);
  cgicc::CgiEnvironment cgienv(&io);

  cgicc::form_iterator iter = cgi["trackId"];
  if (iter != cgi.getElements().end())
  {
    int trackId;
    std::stringstream str(cgi["trackId"]->getValue());
    str >> trackId;

    Database db;
    db.AddVote(cgienv.getRemoteUser(), trackId);
  }

  io << "Status: 204 No Content\r\n\r\n";
}

void index_handler(cgicc::FCgiIO &io)
{
  io << NeztuHeading() << endl;

  Database db;
  std::vector<Vote> votes;
  db.GetVotes(&votes);
  io << "<table>" << endl << "<tr><th>Title</th><th>Voted By</th></tr>" << endl;
  for (std::vector<Vote>::iterator iter = votes.begin(); iter != votes.end(); iter++)
  {
    io << "<tr><td>" << iter->ReqTrack.Title << "</td><td>" << iter->UserName << "</td></tr>" << endl;
  }
  io << "</table>" << endl;

  // Close the HTML document
  io << body() << html();
}

void search_handler(cgicc::FCgiIO &io)
{
  io << NeztuHeading() << endl;

  cgicc::Cgicc cgi(&io);
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
  // XXX cgicc throws an exception trying to parse POST data - this needs investigation
  io << "<form method=\"GET\">";
  io << "<strong>Title</strong><input type=\"text\" name=\"title\" value=\"" << title << "\">";
  io << "<strong>Artist</strong><input type=\"text\" name=\"artist\" value=\"" << artist << "\">";
  io << "<strong>Album</strong><input type=\"text\" name=\"album\" value=\"" << album << "\">";
  io << "<input type=\"submit\"></form>";

  if (!title.empty() || !artist.empty() || !album.empty())
  {
    Database db;
    std::vector<Track> tracks;
    db.GetTracks(&tracks, title.c_str(), artist.c_str(), album.c_str());
    if (!tracks.empty())
    {
      io << "<table><th></th><th>Title</th><th>Artist</th><th>Album</th></tr>";
      for (std::vector<Track>::iterator iter = tracks.begin(); iter != tracks.end(); iter++)
      {
        io << "<tr><td><a href=\"vote?trackId=" << iter->TrackId << "\">Vote</a></td>";
        io << "<td>" << iter->Title << "</td><td>" << iter->Artist << "</td><td>" << iter->Album << "</td></tr>";
      }
      io << "</table>";
    }
    else
    {
      io << "No tracks found.";
    }
  }

  // Close the HTML document
  io << body() << html();
}

void test_handler(cgicc::FCgiIO &io)
{
  io << NeztuHeading() << endl;

  Database db;
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

  // Close the HTML document
  io << body() << html();
}

void resource_handler(cgicc::FCgiIO &io)
{
  // XXX improve this
  static std::map<std::string, std::string> m_contentTypes;
  if (m_contentTypes.empty())
  {
    m_contentTypes.insert(std::make_pair(".css", "text/css"));
    m_contentTypes.insert(std::make_pair(".jpg", "image/jpeg"));
  }

  cgicc::CgiEnvironment cgienv(&io);
  std::string path = cgienv.getPathTranslated();
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

Dispatcher::Dispatcher()
{
  m_paths.insert(std::make_pair("/vote", vote_handler));
  m_paths.insert(std::make_pair("/test", test_handler));
  m_paths.insert(std::make_pair("/search", search_handler));
  m_paths.insert(std::make_pair("/resources/banner.jpg", resource_handler));
  m_paths.insert(std::make_pair("/resources/main.css", resource_handler));
  m_paths.insert(std::make_pair("/", index_handler));
  m_paths.insert(std::make_pair("", index_handler));
}

void Dispatcher::Dispatch(cgicc::FCgiIO &io)
{
  try
  {
    //cgicc::Cgicc cgi(&io);
    cgicc::CgiEnvironment cgienv(&io);
    PathMap::iterator iter = m_paths.find(cgienv.getPathInfo());
    if (iter != m_paths.end())
    {
      iter->second(io);
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
