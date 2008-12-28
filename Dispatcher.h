//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Defines the Dispatcher interface.
//
//-----------------------------------------------------------------------------

#ifndef NEZTU_DISPATCHER_H
#define NEZTU_DISPATCHER_H

#include "Configuration.h"
#include "Database.h"

class Dispatcher
{
public:
  Dispatcher(const Configuration &config);
  void Dispatch(cgicc::FCgiIO&);

  typedef void(*path_handler)(cgicc::FCgiIO&, cgicc::Cgicc&, Database&);

private:
  typedef std::map<std::string, path_handler> PathMap;
  PathMap m_paths;
  Database m_db;
};

#endif
