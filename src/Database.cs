namespace Neztu
{
  struct Track
  {
    uint TrackId;
    string Filename;
    string Title;
    string Artist;
    string Album;
    uint DiscNumber;
    uint TrackNumber;
    uint UserId;
  }

  struct User
  {
    uint UserId;
    string Username;
  }

  interface TrackDatabase
  {
    Track GetTrack(uint trackId);
    Track[] GetTracks(string title, string artist, string album);

    uint AddTrack(Track newTrack);
    void RemoveTrack(uint trackId);
  }

  interface UserDatabase
  {
    User GetUser(uint userId);
    User GetUser(string username);

    uint AddUser(User newUser);
    void RemoveUser(uint userId);
  }
}
