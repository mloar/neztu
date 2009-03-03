//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements the Request class.
//
//-----------------------------------------------------------------------------

#include "Request.h"

namespace neztu
{
    Request::Request(
        const Configuration &conf,
        Database &database,
        cgicc::FCgiIO &fcgiIo
        ) :
        config(conf),
        db(database),
        io(fcgiIo),
        cgi(&io)
    {
    }
}

