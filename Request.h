//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the Request class for storing per-request state.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_REQUEST_H
#define NEZTU_REQUEST_H

#include <cgicc/Cgicc.h>

#include "Configuration.h"
#include "Database.h"
#include "FCgiIO.h"

namespace neztu
{
    class Request
    {
    public:
        const Configuration &config;
        Database &db;
        cgicc::FCgiIO &io;
        cgicc::Cgicc cgi;

    public:
        Request(const Configuration &config, Database &database, cgicc::FCgiIO &io);
    };
}

#endif
