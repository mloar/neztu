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
using System.IO;

namespace Neztu
{
  class TagReader
  {
    class StreamAbstraction : TagLib.File.IFileAbstraction
    {
      private Stream m_stream;
      private string m_filename;

      public StreamAbstraction(string filename, Stream stream)
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

      public Stream ReadStream
      {
        get
        {
          return m_stream;
        }
      }

      public Stream WriteStream
      {
        get
        {
          return null;
        }
      }

      public void CloseStream (Stream stream)
      {
        stream.Close();
      }
    }

    public static void DoDirectory(INeztuDatabase db, string directory)
    {
      Console.WriteLine("Entering directory {0}...", directory);
      try
      {
        foreach (string file in Directory.GetFiles(directory))
        {
          // FIXME: This is a hack because TagLib appears to eat it on .wma files
          if (Path.GetExtension(file) == ".wma")
            continue;

          try
          {
            if (db.GetTrack(file).TrackId == 0)
            {
              Console.WriteLine("Attempting to read {0}...", file);
              Track t = ReadFile(file);
              t.Uploader = string.Empty;
              db.AddTrack(t);
            }
            else
            {
              Console.WriteLine("Already have {0}, skipping.", file);
            }
          }
          catch (ApplicationException)
          {
            Console.Error.WriteLine("{0} has no usable tags.  Skipping.", file);
          }
          catch (TagLib.UnsupportedFormatException)
          {
            Console.Error.WriteLine("{0} is an unsupported format.  Skipping.", file);
          }
          catch (TagLib.CorruptFileException)
          {
            Console.Error.WriteLine("{0} could not be read.  Skipping.", file);
          }
          catch (PostgresBackendException)
          {
            Console.Error.WriteLine("Database error occurred trying to add {0}.  Skipping.", file);
          }
        }

        foreach (string dir in Directory.GetDirectories(directory))
        {
          DoDirectory(db, dir);
        }
      }
      catch (UnauthorizedAccessException)
      {
        Console.Error.WriteLine("{0} could not be accessed.  Skipping.", directory);
      }
    }

    public static void Main(string[] args)
    {
      INeztuDatabase db = DatabaseHelper.GetDatabase();

      foreach (string arg in args)
      {
        DoDirectory(db, arg);

      }
    }

    public static Track ReadFile(string filename)
    {
      TagLib.File file;
      TagLib.Tag tag;

      try
      {
        file = TagLib.File.Create(filename);
        // Beware!  TagTypes.AllTags does not work!
        tag = file.GetTag(TagLib.TagTypes.Id3v2);
        if (tag == null)
        {
          tag = file.GetTag(TagLib.TagTypes.Id3v1);
          if (tag == null)
          {
            tag = file.GetTag(TagLib.TagTypes.Xiph);
            if (tag == null)
            {
              tag = file.GetTag(TagLib.TagTypes.Asf);
              if (tag == null)
              {
                // TODO: use a custom exception class
                throw new ApplicationException("File has no readable tags");
              }
            }
          }
        }
      }
      catch (FileNotFoundException)
      {
        throw new ApplicationException("Broken symlink?");
      }

      Track ret = new Track();
      ret.TrackId = 0;
      ret.Filename = Path.GetFullPath(filename);
      ret.Title = tag.Title == null ? "" : tag.Title;
      ret.Artist = tag.FirstPerformer == null ? "" : tag.FirstPerformer;
      ret.Album = tag.Album == null ? "" : tag.Album;
      ret.Genre = tag.FirstGenre == null ? "" : tag.FirstGenre;
      ret.DiscNumber = tag.Disc;
      ret.TrackNumber = tag.Track;
      ret.Length = file.Properties.Duration;
      ret.Uploader = string.Empty;

      return ret;
    }

    public static Track ReadStream(string filename, Stream stream)
    {
      TagLib.File file = TagLib.File.Create(new StreamAbstraction(filename, stream));
      TagLib.Tag tag = file.GetTag(TagLib.TagTypes.Id3v2);
      if (tag == null)
      {
        // TODO: use a custom exception class
        throw new ApplicationException("File has no readable tags");
      }

      Track ret = new Track();

      ret.TrackId = 0;
      ret.Filename = filename;
      ret.Title = tag.Title == null ? "" : tag.Title;
      ret.Artist = tag.FirstPerformer == null ? "" : tag.FirstPerformer;
      ret.Album = tag.Album == null ? "" : tag.Album;
      ret.Genre = tag.FirstGenre == null ? "" : tag.FirstGenre;
      ret.DiscNumber = tag.Disc;
      ret.TrackNumber = tag.Track;
      ret.Length = file.Properties.Duration;
      ret.Uploader = string.Empty;

      return ret;
    }
  }
}
