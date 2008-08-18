#!/usr/bin/liquidsoap
out = output.icecast.vorbis(mount="/announce", port=8005)
out(
        on_blank(length=1., shutdown, mksafe(once(single("say:"^read()))))
   )
