#!/bin/sh
MONO_SHARED_DIR=/var/run/liquidsoap
DAEMON=/home/matt/devel/neztu/www/bin/Daemon.exe

export MONO_SHARED_DIR

case $1 in
-u)
  # We are preparing a track for playing.  This needs to be synchronous.
  $DAEMON "$@"
  ;;
-r)
  # We are preparing a track for playing.  This needs to be synchronous.
  $DAEMON "$@"
  ;;
-n)
  # We are notifying Neztu of a track change.  This is run on the main thread,
  # so we do so asynchronously.
  $DAEMON "$@" >/dev/null 2>&1 &
  disown
  ;;
*)
  echo unrecognized option
  exit 1
  ;;
esac
