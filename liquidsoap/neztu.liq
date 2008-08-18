#!/usr/bin/liquidsoap

set("server.telnet", true)

neztu_daemon = '/home/matt/devel/neztu/liquidsoap/daemon.sh'
neztu_notify = fun (t) -> system(neztu_daemon ^ ' -n ' ^ quote('$(filename)' % t))
neztu_request = fun () -> on_track(neztu_notify, request.dynamic({request.create(get_process_output(neztu_daemon ^ ' -u'))}))
neztu_random = fun () -> on_track(neztu_notify, request.dynamic({request.create(get_process_output(neztu_daemon ^ ' -r'))}))

out = output.prefered()
out(
    add(
      weights=[5,1],
      [
      input.harbor(id='announce', "announce"),
      fallback([
        fallback([
          neztu_request(),
          neztu_random(),
          once(single("say:Welcome to Neztu!")),
          delay(5.,single("say:We are experiencing technical difficulties.  Please stand by.")),
          blank(duration=5.)
          ])
        ])
      ])
   )
