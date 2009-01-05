//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the Dispatcher interface.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_DISPATCHER_H
#define NEZTU_DISPATCHER_H

#include "Configuration.h"
#include "Database.h"
#include "Request.h"

namespace neztu
{
  class Dispatcher
  {
    public:
      Dispatcher(const Configuration &config);
      void Dispatch(cgicc::FCgiIO &io);

      //typedef void(*path_handler)(cgicc::FCgiIO&, cgicc::Cgicc&, Database&);
      typedef void(*path_handler)(Request &);

    private:
      typedef std::map<std::string, path_handler> PathMap;
      PathMap m_paths;
      const Configuration &m_config;
      Database m_db;
  };
}

#endif
