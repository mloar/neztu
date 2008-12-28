//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Defines the Configuration class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_CONFIGURATION_H
#define NEZTU_CONFIGURATION_H

#include <map>
#include <string>

class Configuration
{
  typedef std::map<std::string, std::string> EntryMap;
  EntryMap m_config;

  public:
    Configuration(const std::string &file);

    const std::string& GetDatabaseConnectionString() const;
};

#endif
