//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements the Configuration class.
//
//-----------------------------------------------------------------------------

#include <stdexcept>
#include <fstream>
#include <sstream>

#include "Configuration.h"

namespace neztu
{
  Configuration::Configuration(const std::string &path)
  {
    // First we insert empty settings into the map.  This way we can ensure that
    // they exist in the map even if they are not in the config file, and then we
    // can use the map as a reference for valid settings when reading the config
    // file.
    m_config.insert(std::make_pair("dbconn", ""));
    m_config.insert(std::make_pair("scheduler", ""));
    m_config.insert(std::make_pair("skipCommand", ""));

    std::ifstream file(path.c_str());
    if (!file.is_open())
    {
      throw std::runtime_error("could not open configuration file");
    }

    while (file.good())
    {
      std::string line;
      getline(file, line);

      // remove comments
      if (line.find('#') != std::string::npos)
      {
        line.erase(line.find('#'));
      }

      std::stringstream lineStream(line);
      lineStream >> std::ws;

      // If the line is now empty, we're done.
      if (!lineStream.eof())
      {
        std::string word;
        lineStream >> word;

        EntryMap::iterator entry = m_config.find(word);
        if (entry == m_config.end())
        {
          std::stringstream errmsg;
          errmsg << "Unrecognized configuration directive " << word;
          throw std::runtime_error(errmsg.str().c_str());
        }
        else if (!entry->second.empty())
        {
          std::stringstream errmsg;
          errmsg << "Duplicate configuration directive " << word;
          throw std::runtime_error(errmsg.str().c_str());
        }

        lineStream >> word;
        if (word.compare("="))
        {
          std::stringstream errmsg;
          errmsg << "Expected = but found " << word;
          throw std::runtime_error(errmsg.str().c_str());
        }

        // Store the value in the map.
        lineStream >> std::ws;
        getline(lineStream, entry->second);
      }
    }
  }

  const std::string& Configuration::GetDatabaseConnectionString() const
  {
    return m_config.find("dbconn")->second;
  }

  const std::string& Configuration::GetSchedulingMethod() const
  {
    return m_config.find("scheduler")->second;
  }

  const std::string& Configuration::GetSkipCommand() const
  {
    return m_config.find("skipCommand")->second;
  }
}
