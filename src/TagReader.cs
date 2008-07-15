//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      A simple application for importing existing files into the track
//      database.
//
//-----------------------------------------------------------------------------

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
        try
        {
          Track t = ReadFile(arg);
          t.UserName = string.Empty;
          db.AddTrack(t);
        }
        catch (TagLib.CorruptFileException)
        {
          Console.Error.WriteLine("{0} could not be read.  Skipping.", arg);
        }
        catch (PostgresBackendException)
        {
          Console.Error.WriteLine("Database error occurred trying to add {0}.  Skipping.", arg);
        }
      }
    }

    public static Track ReadFile(string filename)
    {
        File file = File.Create(filename);
        Tag tag = file.GetTag(TagTypes.Id3v2);
        Track ret;

        ret.TrackId = 0;
        ret.Filename = System.IO.Path.GetFullPath(filename);
        ret.Title = tag.Title == null ? "" : tag.Title;
        ret.Artist = tag.FirstPerformer == null ? "" : tag.FirstPerformer;
        ret.Album = tag.Album == null ? "" : tag.Album;
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

        ret.TrackId = 0;
        ret.Filename = null;
        ret.Title = tag.Title == null ? "" : tag.Title;
        ret.Artist = tag.FirstPerformer == null ? "" : tag.FirstPerformer;
        ret.Album = tag.Album == null ? "" : tag.Album;
        ret.DiscNumber = tag.Disc;
        ret.TrackNumber = tag.Track;
        ret.Length = file.Properties.Duration;
        ret.UserName = string.Empty;

        return ret;
    }
  }
}
