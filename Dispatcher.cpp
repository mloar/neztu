//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
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
#include "Configuration.h"
#include "Database.h"
#include "Dispatcher.h"
#include "Scheduler.h"
#include "TagReader.h"

using namespace cgicc;
using namespace std;

namespace neztu
{
    std::string GetUserName(Cgicc &cgi)
    {
        std::string ret = cgi.getEnvironment().getRemoteUser();

        if (ret.find('@') != std::string::npos)
        {
            ret.erase(ret.find('@'));
        }

        return ret;
    }

    class NeztuHeading : public cgicc::MStreamable
    {
    public:
        virtual void render(std::ostream& out) const
        {
            // Send HTTP header
            out << "Content-type: text/html; charset=utf-8\r\n\r\n";

            // Set up the HTML document
            out << HTMLDoctype() << endl
                << html() << head() << title("Neztu!")
                << cgicc::link(set("href", "resources/main.css").set("type", "text/css").set("rel", "stylesheet"))
                << head() << endl
                << body() << "<script type=\"text/javascript\" src=\"resources/Neztu.js\"></script>" << endl
                << "<div id=\"header\"><img src=\"resources/banner.jpg\" alt=\"Neztu!\">" << endl
                << "<table id=\"menu\">"
                << "<tr><td><a href=\"./\">Home</a></td>"
                << "<td><a href=\"playlist\">Your Playlist</a></td>"
                << "<td><a href=\"search\">Search Tracks</a></td><td><a href=\"add\">Add Tracks</a></td></tr>"
                << "</table></div>"
                << "<div id=\"statusbar\"></div>"
                << "<div id=\"main\">";
        }
    };

    class NeztuFooter : public cgicc::MStreamable
    {
    public:
        virtual void render(std::ostream& out) const
        {
            out << "</div>"
                << "<div id=\"footer\">"
                << "<div id=\"nowplaying\">"
                << "<div id=\"nowplayingleft\">"
                << "<span id=\"nowplayingtext\">Now Playing:</span><br>"
                << "<span id=\"skiplink\">"
                << "<a href=\"skip\" onclick=\"javascript:return skipSong();\">Skip!</a>"
                << "</span>"
                << "</div>"
                << "<div id=\"nowplayingright\">"
                << "<span id=\"NowPlayingTitle\"></span><br>"
                << "<span id=\"NowPlayingArtist\"></span><br></div>"
                << "</div>"
                << "</div>"
                << "<script type=\"text/javascript\">updateNowPlaying();</script>"
                << body() << html();
        }
    };

    class Queue : public cgicc::MStreamable
    {
        mutable Scheduler m_scheduler;

    public:
        Queue(const Configuration &config, Database &db) :
            m_scheduler(config, db)
        {
        }

        virtual void render(std::ostream& out) const
        {
            const std::list<std::vector<Vote>::iterator> &voteList = m_scheduler.GetSchedule();
            out << "<table border=\"1\" style=\"width:100%\"><caption>Queue</caption>" << endl;
            out << "<tr><th>Title</th><th>Artist</th><th>Album</th><th>Length</th><th>Requested By</th></tr>" << endl;
            for (
                std::list<std::vector<Vote>::iterator>::const_iterator iter = voteList.begin();
                iter != voteList.end();
                iter++
                )
            {
                out << "<tr><td>" << (*iter)->ReqTrack.Title << "</td><td>" << (*iter)->ReqTrack.Artist << "</td>";
                out << "<td>" << (*iter)->ReqTrack.Album << "</td>";
                out << "<td>";
                out << setw(2) << setfill('0') << (*iter)->ReqTrack.Length / 3600 << ":";
                out << setw(2) << setfill('0') << ((*iter)->ReqTrack.Length % 3600) / 60 << ":";
                out << setw(2) << setfill('0') << ((*iter)->ReqTrack.Length % 60) << "</td>";
                out << "<td>" << (*iter)->UserName << "</td></tr>";
            }
            out << "</table>" << endl;
        }
    };

    class RandomTracks : public cgicc::MStreamable
    {
        Database &m_db;

    public:
        RandomTracks(Database &db) :
            m_db(db)
        {
        }

        virtual void render(std::ostream& out) const
        {
            std::vector<Track> tracks;
            m_db.GetRandomTracks(&tracks, 10);
            out << "<table border=\"1\" style=\"width:100%\"><caption>Random Tracks</caption>" << endl;
            out << "<tr><th></th><th>Title</th><th>Artist</th><th>Album</th><th>Length</th></tr>" << endl;
            for (std::vector<Track>::iterator iter = tracks.begin(); iter != tracks.end(); iter++)
            {
                out << "<tr><td>" << "<a onclick=\"javascript:return voteForSong(" 
                    << iter->TrackId << ");\" href=\"vote?trackId=" << iter->TrackId << "\">Vote</a></td>";
                out << "<td>" << iter->Title << "</td><td>" << iter->Artist << "</td>";
                out << "<td>" << iter->Album << "</td>";
                out << "<td>";
                out << setw(2) << setfill('0') << iter->Length / 3600 << ":";
                out << setw(2) << setfill('0') << (iter->Length % 3600) / 60 << ":";
                out << setw(2) << setfill('0') << (iter->Length % 60) << "</td>";
                out << "</td></tr>";
            }
            out << "</table>" << endl;
        }
    };

    void vote_handler(Request &req)
    {
        FCgiIO &io = req.io;
        Cgicc &cgi = req.cgi;
        Database &db = req.db;

        cgicc::form_iterator iter = cgi["trackId"];
        if (iter != cgi.getElements().end())
        {
            int trackId;
            std::stringstream str(cgi["trackId"]->getValue());
            str >> trackId;

            db.AddVote(GetUserName(cgi), trackId);
        }

        io << "Status: 204 No Content\r\n\r\n";
    }

    void skip_handler(Request &req)
    {
        Vote v = req.db.GetCurrent();
        if (v.UserName == "neztu" || v.UserName == GetUserName(req.cgi))
        {
            system(req.config.GetSkipCommand().c_str());
            req.io << "Status: 204 No Content\r\n\r\n";
        }
        else
        {
            req.io << "Status: 403 Forbidden\r\n";
            req.io << "Content-type: text/plain\r\n\r\n";
            req.io << "Not your song!\r\n";
        }
    }

    void index_handler(Request &req)
    {
        req.io << NeztuHeading() << endl;

        req.io << "<div id=\"queuedata\">" << Queue(req.config, req.db) << "</div>";
        req.io << "<div id=\"randomtracks\">" << RandomTracks(req.db) << "</div>";

        req.io << NeztuFooter();
    }

    void redirect_index_handler(Request &req)
    {
        req.io << "Location: http://" << req.cgi.getEnvironment().getServerName()
            << req.cgi.getEnvironment().getScriptName() << "/\r\n\r\n";
    }

    void playlist_handler(Request &req)
    {
        FCgiIO &io = req.io;
        Cgicc &cgi = req.cgi;
        Database &db = req.db;

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
                db.SwapVotes(GetUserName(cgi), trackId1, trackId2);
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
                db.RemoveVote(GetUserName(cgi), trackId);
            }
        }

        std::vector<Vote> votes;
        db.GetVotes(&votes, GetUserName(cgi));

        io << NeztuHeading() << endl;
        io << "<table border=\"1\" style=\"width:100%\"><caption>Your Playlist</caption>" << endl;
        io << "<tr><th></th><th></th><th>Title</th><th>Artist</th><th>Length</th></tr>" << endl;
        unsigned int lastTrackId = 0;
        for (std::vector<Vote>::iterator iter = votes.begin(); iter != votes.end(); iter++)
        {
            io << "<tr><td>";
            if (lastTrackId)
            {
                io << "<a href=\"?swap=yes&track1=" << lastTrackId 
                    << "&track2=" << iter->ReqTrack.TrackId << "\">Up</a>";
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

    void add_handler(Request &req)
    {
        FCgiIO &io = req.io;
        Cgicc &cgi = req.cgi;
        Database &db = req.db;

        io << NeztuHeading() << endl;

        cgicc::form_iterator mydir = cgi["mydir"];
        if (mydir != cgi.getElements().end() && mydir->getValue() == "Index")
        {
            io << "Results:";
            io << "<pre>";
            TagReader::ProcessPath(
                db,
                req.config.GetMusicBaseDirectory() + "/" + GetUserName(cgi),
                io
                );
            io << "</pre>";
        }

        io << "<form method=\"POST\">";
        io << req.config.GetMusicBaseDirectory() + "/" + GetUserName(cgi);
        io << "<input type=\"submit\" name=\"mydir\" value=\"Index\">";
        io << "</form>";
        io << NeztuFooter();
    }

    void search_handler(Request &req)
    {
        FCgiIO &io = req.io;
        Cgicc &cgi = req.cgi;
        Database &db = req.db;
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
                    io << "<tr><td><a onclick=\"javascript:return voteForSong(" << iter->TrackId
                        << ");\" href=\"vote?trackId=" << iter->TrackId << "\">Vote</a></td>";
                    io << "<td>" << iter->Title << "</td><td>" << iter->Artist << "</td><td>"
                        << iter->Album << "</td></tr>";
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

    void resource_handler(Request &req)
    {
        FCgiIO &io = req.io;
        Cgicc &cgi = req.cgi;

        // XXX improve this
        static std::map<std::string, std::string> m_contentTypes;
        if (m_contentTypes.empty())
        {
            m_contentTypes.insert(std::make_pair(".css", "text/css"));
            m_contentTypes.insert(std::make_pair(".jpg", "image/jpeg"));
            m_contentTypes.insert(std::make_pair(".js", "text/javascript"));
        }

        std::string path = req.config.GetApplicationDirectory()
            + cgi.getEnvironment().getPathInfo();

        std::map<std::string, std::string>::iterator iter = m_contentTypes.find(path.substr(path.rfind('.')));
        if (iter == m_contentTypes.end())
        {
            throw std::runtime_error("unrecognized file type");
        }
        std::ifstream file(path.c_str(), std::ifstream::in | std::ifstream::binary);
        if (!file.is_open())
        {
            throw std::runtime_error(std::string("could not open resource file: ") + path);
        }
        io << "Content-type: " << iter->second << "\r\n\r\n";
        io << file.rdbuf();
    }

    void nowplaying_handler(Request &req)
    {
        Vote v = req.db.GetCurrent();
        tm tim;
        std::string timeout = "3000"; // arbitrary default
        if (strptime(v.Timestamp.c_str(), "%Y-%m-%d %H:%M:%S", &tim))
        {
            int diff = static_cast<int>(difftime(mktime(&tim), time(NULL))) + v.ReqTrack.Length + 1;
            if (diff > 0)
            {
                std::stringstream strstream;
                strstream << diff * 1000;
                timeout = strstream.str();
            }
        }

        req.io << "Content-type: text/plain; charset=utf-8\r\n\r\n";
        req.io << Queue(req.config, req.db);
        req.io << v.ReqTrack.Title << "\n" << v.ReqTrack.Artist << "\n" << timeout;
    }

    Dispatcher::Dispatcher(const Configuration &config) :
        m_config(config),
        m_db(config)
    {
        m_paths.insert(std::make_pair("/add", add_handler));
        m_paths.insert(std::make_pair("/vote", vote_handler));
        m_paths.insert(std::make_pair("/skip", skip_handler));
        m_paths.insert(std::make_pair("/playlist", playlist_handler));
        m_paths.insert(std::make_pair("/search", search_handler));
        m_paths.insert(std::make_pair("/nowplaying", nowplaying_handler));
        m_paths.insert(std::make_pair("/resources/banner.jpg", resource_handler));
        m_paths.insert(std::make_pair("/resources/main.css", resource_handler));
        m_paths.insert(std::make_pair("/resources/Neztu.js", resource_handler));
        m_paths.insert(std::make_pair("/", index_handler));
        m_paths.insert(std::make_pair("", redirect_index_handler));
    }

    void Dispatcher::Dispatch(cgicc::FCgiIO &io)
    {
        try
        {
            Request req(m_config, m_db, io);
            PathMap::iterator iter = m_paths.find(req.cgi.getEnvironment().getPathInfo());
            if (iter != m_paths.end())
            {
                iter->second(req);
            }
            else
            {
                // Send HTTP header
                req.io << HTTPHTMLHeader() << endl;

                // Set up the HTML document
                req.io << html() << head(title("Neztu!")) << endl;
                req.io << body() << endl;
                req.io << "Page not found" << endl;
                req.io << body() << html() << endl;
            }
        }
        catch(std::exception& e)
        {
            if (io.good())
            {
                io << "Status: 500 Internal Server Error\r\n\r\n";
            }
            if (io.err().good())
            {
                io.err() << e.what() << endl;
            }
        }
    }
}
