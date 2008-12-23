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

void index_handler(cgicc::FCgiIO &io)
{
  // Send HTTP header
  io << HTTPHTMLHeader() << endl;

  // Set up the HTML document
  io << html() << head(title("Neztu!")) << endl;
  io << body() << endl;
  io << "Hi!" << endl;

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

void test_handler(cgicc::FCgiIO &io)
{
  // Send HTTP header
  io << HTTPHTMLHeader() << endl;

  // Set up the HTML document
  io << html() << head(title("Neztu!")) << endl;
  io << body() << endl;

  Database db;
  Track t = db.GetTrack(3200);
  io << t.Title.c_str() << " by " << t.Artist.c_str() << endl;
  t = db.GetTrack("/home/matt/winfiles/Music/Cat Power/Moon Pix/Cat Power_Moon Pix_11_Peking Saint.mp3");
  io << t.Title.c_str() << " by " << t.Artist.c_str() << endl;
  std::vector<Track> tracks;
  db.GetTracks(&tracks);
  io << tracks.size() << " tracks" << endl; 

  // Close the HTML document
  io << body() << html();
}

Dispatcher::Dispatcher()
{
  m_paths.insert(std::make_pair("/test", test_handler));
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
    io << e.what() << endl;
    io << body() << html();
  }
}
