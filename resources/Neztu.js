function GetXMLHttpRequest() {
  if (window.XMLHttpRequest) {
    return new XMLHttpRequest();
  } else {
    if (window.XMLHttpRequestProgID) {
      return new ActiveXObject(window.XMLHttpRequestProgID);
    } else {
      var progIDs = ["Msxml2.XMLHTTP.6.0", "Msxml2.XMLHTTP.5.0", "Msxml2.XMLHTTP.4.0", "MSXML2.XMLHTTP.3.0", "MSXML2.XMLHTTP", "Microsoft.XMLHTTP"];
      for (var i = 0; i < progIDs.length; ++i) {
        var progID = progIDs[i];
        try {
          var x = new ActiveXObject(progID);
          window.XMLHttpRequestProgID = progID;
          return x;
        } catch (e) {
        }
      }
    }
  }
  return null;
}

function updateNowPlaying() {
  var x = GetXMLHttpRequest();
  if (!x) {
    // Can't do AJAX - reload the page.
    window.reload();
  } else {
    x.open("GET", "nowplaying");
    x.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
    x.setRequestHeader("Accept-Encoding", "gzip, deflate");
    x.onreadystatechange = function() {
      if (x.readyState != 4) {
        // Failed - wait 10 seconds and try again
        setTimeout(updateNowPlaying, 10000);
        return;
      }
      var queue = x.responseText.substr(0, x.responseText.indexOf("</table>\n"));
      var title = x.responseText.substr(queue.length + 9, x.responseText.substr(queue.length + 9).indexOf("\n"));
      var artist = x.responseText.substr(
          queue.length + title.length + 10,
          x.responseText.substr(queue.length + title.length + 10).indexOf("\n")
          );
      var timeout = x.responseText.substr(queue.length + title.length + artist.length + 11);

      if (document.getElementById("queuedata"))
      {
        document.getElementById("queuedata").innerHTML = queue;
      }
      document.getElementById("NowPlayingTitle").innerHTML = title;
      document.getElementById("NowPlayingArtist").innerHTML = artist;
      setTimeout(updateNowPlaying, timeout);
    }
    x.send("");
  }
}
