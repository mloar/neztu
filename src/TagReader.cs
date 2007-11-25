using System;
using TagLib;

namespace Neztu
{
  class TagReader
  {
    public static void Main(string[] args)
    {
      ITrackDatabase db = new PostgresTrackDatabase();

      foreach (string arg in args)
      {
        Track t = ReadFile(arg);
        t.UserId = Guid.Empty;
        db.AddTrack(t);
      }
    }

    public static Track ReadFile(string filename)
    {
        File file = File.Create(filename);
        Tag tag = file.GetTag(TagTypes.Id3v2);
        Track ret;

        ret.TrackId = Guid.Empty;
        ret.Filename = filename;
        ret.Title = tag.Title;
        ret.Artist = tag.FirstPerformer;
        ret.Album = tag.Album;
        ret.DiscNumber = tag.Disc;
        ret.TrackNumber = tag.Track;
        ret.Length = file.Properties.Duration;
        ret.UserId = Guid.Empty;

        return ret;
    }
  }
}
