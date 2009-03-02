function GetXMLHttpRequest() {
  if (window.XMLHttpRequest) {
    return new XMLHttpRequest();
  } else {
    if (window.XMLHttpRequestProgID) { return new ActiveXObject(window.XMLHttpRequestProgID);
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
    x.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=utf-8", true);
    x.setRequestHeader("Accept-Encoding", "gzip, deflate");
    x.onreadystatechange = function() {
      if (x.readyState != 4) {
        return;
      }

      if (x.responseText.length == 0) {
        // Failed - try again 10 minutes later
        setTimeout(updateNowPlaying, 10000);
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
    x.send(null);
  }
}

function skipSong() {
  var x = GetXMLHttpRequest();
  if (!x) {
    // Can't do AJAX - do it the old fashioned way
    return true;
  } else {
      var requestTimer = setTimeout(
              function() {
              x.abort();
              document.getElementById("statusbar").innerHTML = "Skip song request timed out."
              }
              , 10000);
      x.open("GET", "skip", true);
      x.onreadystatechange = function() {
          if (x.readyState != 4) {
              return;
          }

          clearTimeout(requestTimer);

          if (x.status == 204)
          {
              document.getElementById("statusbar").innerHTML = "Song skipped.";
              setTimeout(updateNowPlaying, 500);
          }
          else if (x.status == 403)
          {
              document.getElementById("statusbar").innerHTML = "Server rejected our skip song request.";
          }
          else
          {
              document.getElementById("statusbar").innerHTML = "Unexpected failure trying to skip song.";
          }
      }
      x.send(null);
  }

  return false;
}

function voteForSong(trackId) {
  var x = GetXMLHttpRequest();
  if (!x) {
    // Can't do AJAX - do it the old fashioned way
    return true;
  } else {
      var requestTimer = setTimeout(
              function() {
              x.abort();
              document.getElementById("statusbar").innerHTML = "Vote timed out."
              }
              , 10000);
      x.open("GET", "vote?trackId=" + trackId , true);
      x.onreadystatechange = function() {
          if (x.readyState != 4) {
              return;
          }

          clearTimeout(requestTimer);

          if (x.status == 204)
          {
              document.getElementById("statusbar").innerHTML = "Voted for track " + trackId + ".";
              setTimeout(updateNowPlaying, 100);
          }
          else if (x.status == 403)
          {
              document.getElementById("statusbar").innerHTML = "Server rejected our vote.";
          }
          else
          {
              document.getElementById("statusbar").innerHTML = "Unexpected failure trying to vote.";
          }
      }
      x.send(null);
  }

  return false;
}
