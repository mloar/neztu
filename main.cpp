#include <fcgiapp.h>
#include <stdlib.h>
#include "FCgiIO.h"
#include "Dispatcher.h"

int main(int argc, char* argv[])
{
  Dispatcher dispatcher;

  int stopSleep = 0;
  while (getenv("SLEEP") != NULL && stopSleep == 0) {
    sleep(2);
  }

  if (FCGX_Init())
  {
    fprintf(stderr, "wtf FCGX_Init failed\n");
  }

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
      // just discard it
    }
  }

  return 0;
}
