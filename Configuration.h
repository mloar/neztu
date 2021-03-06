//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the Configuration class.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_CONFIGURATION_H
#define NEZTU_CONFIGURATION_H

#include <map>
#include <string>

namespace neztu
{
    class Configuration
    {
        typedef std::map<std::string, std::string> EntryMap;
        EntryMap m_config;

    public:
        Configuration(const std::string &file);

        const std::string& GetDatabaseConnectionString() const;
        const std::string& GetSchedulingMethod() const;
        const std::string& GetSkipCommand() const;
        const std::string& GetApplicationDirectory() const;
        const std::string& GetMusicBaseDirectory() const;
    };
}

#endif // NEZTU_CONFIGURATION_H
