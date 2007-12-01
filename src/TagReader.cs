using System;
using TagLib;

namespace Neztu
{
  class TagReader
  {
    class StreamAbstraction : TagLib.File.IFileAbstraction
    {
      private System.IO.Stream m_stream;
      private string m_filename;

      public StreamAbstraction(string filename, System.IO.Stream stream)
      {
        m_filename = filename;
        m_stream = stream;
      }

      public string Name
      {
        get
        {
          return m_filename;
        }
      }

      public System.IO.Stream ReadStream
      {
        get
        {
          return m_stream;
        }
      }

      public System.IO.Stream WriteStream
      {
        get
        {
          return null;
        }
      }

      public void CloseStream (System.IO.Stream stream)
      {
        stream.Close();
      }
    }

    public static void Main(string[] args)
    {
      ITrackDatabase db = new PostgresTrackDatabase();

      foreach (string arg in args)
      {
        Track t = ReadFile(arg);
        t.UserName = string.Empty;
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
        ret.UserName = string.Empty;

        return ret;
    }

    public static Track ReadStream(string filename, System.IO.Stream stream)
    {
        File file = File.Create(new StreamAbstraction(filename, stream));
        Tag tag = file.GetTag(TagTypes.Id3v2);
        Track ret;

        ret.TrackId = Guid.Empty;
        ret.Filename = null;
        ret.Title = tag.Title;
        ret.Artist = tag.FirstPerformer;
        ret.Album = tag.Album;
        ret.DiscNumber = tag.Disc;
        ret.TrackNumber = tag.Track;
        ret.Length = file.Properties.Duration;
        ret.UserName = string.Empty;

        return ret;
    }
  }
}
