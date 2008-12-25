//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements the entry point for the FastCGI application.
//
//-----------------------------------------------------------------------------

#include <fcgiapp.h>
#include <stdlib.h>
#include <cgicc/Cgicc.h>

#include "FCgiIO.h"
#include "Database.h"
#include "Dispatcher.h"

int main(int argc, char* argv[])
{
  // This code allows the opportunity to attach a debugger before the request is
  // handled.
  int stopSleep = 0;
  while (getenv("SLEEP") != NULL && stopSleep == 0) {
    sleep(2);
  }

  if (FCGX_Init())
  {
    fprintf(stderr, "FCGX_Init() failed\n");
    return 1;
  }

  // We initialize the dispatcher outside of the loop so that we do
  // initialization tasks once and maintain a database connection between
  // requests.
  Dispatcher dispatcher;

  FCGX_Request req;
  FCGX_InitRequest(&req, 0, 0);
  while (!FCGX_Accept_r(&req))
  {
    try
    {
      cgicc::FCgiIO io(req);
      dispatcher.Dispatch(io);
    }
    catch (std::exception &e)
    {
      FCGX_FPrintF(req.err, "exception caught at outer loop: %s\n", e.what());
      // We shouldn't be getting any exceptions here, so we rethrow to terminate
      // the process.
      throw;
    }
  }

  fprintf(stderr,
      "FCGX_Accept_r() returned an error.\n"
      "This probably means this was run from the command line and not as a "
      "FastCGI application.\n");

  return 0;
}
