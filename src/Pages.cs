//+----------------------------------------------------------------------------
//
//  Copyright (c) 2008 Matthew Loar.
//  See COPYING for license terms.
//
//  Abstract:
//      Implements the logic for individual pages.
//
//-----------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Web.Profile;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Neztu;

public class PlayListPage : Page
{
  public DataGrid PlayListData;

  public void Page_Load(object o, EventArgs e)
  {
    if (!IsPostBack)
    {
      INeztuDatabase database = DatabaseHelper.GetDatabase();

      Vote[] myVotes = database.GetVotes(User.Identity.Name);

      DataView dataView = new DataView();
      dataView.Table = new DataTable("PlayList");
      dataView.Table.Columns.Add(new DataColumn("Track"));
      dataView.Table.Columns.Add(new DataColumn("Album"));
      dataView.Table.Columns.Add(new DataColumn("Length"));
      foreach (Vote v in myVotes)
      {
        DataRowView rowView = dataView.AddNew();
        rowView["Track"] = v.ReqTrack.Title;
        rowView["Album"] = v.ReqTrack.Album;
        rowView["Length"] = v.ReqTrack.Length.ToString();
        rowView.EndEdit();
      }
      PlayListData.DataSource = dataView;
      PlayListData.DataBind();
      PlayListData.Visible = true;
    }
  }

  public void PlayListData_Command(object o, DataGridCommandEventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    Vote[] myVotes = database.GetVotes(User.Identity.Name);

    switch(((LinkButton)e.CommandSource).CommandName)
    {
      case "Up":
        if (e.Item.ItemIndex > 0)
        {
          database.SwapVotes(myVotes[e.Item.ItemIndex], myVotes[e.Item.ItemIndex - 1]);
        }
      break;
      case "Down":
        if (e.Item.ItemIndex < myVotes.Length - 1)
        {
          database.SwapVotes(myVotes[e.Item.ItemIndex], myVotes[e.Item.ItemIndex + 1]);
        }
      break;
      case "Remove":
        database.RemoveVote(myVotes[e.Item.ItemIndex].UserName, myVotes[e.Item.ItemIndex].ReqTrack.TrackId);
      break;
    }

    myVotes = database.GetVotes(User.Identity.Name);

    DataView dataView = new DataView();
    dataView.Table = new DataTable("PlayList");
    dataView.Table.Columns.Add(new DataColumn("Track"));
    dataView.Table.Columns.Add(new DataColumn("Album"));
    dataView.Table.Columns.Add(new DataColumn("Length"));
    foreach (Vote v in myVotes)
    {
      DataRowView rowView = dataView.AddNew();
      rowView["Track"] = v.ReqTrack.Title;
      rowView["Album"] = v.ReqTrack.Album;
      rowView["Length"] = v.ReqTrack.Length.ToString();
      rowView.EndEdit();
    }

    PlayListData.DataSource = dataView;
    PlayListData.DataBind();
    PlayListData.Visible = true;
  }
}

public class SearchPage : Page
{
  public Label StatusBar;
  public TextBox SongTitle;
  public TextBox Artist;
  public TextBox Album;
  public Button Search;
  public Button VoteAll;
  public DataGrid ResultsData;

  public void ResultsData_Command(object o, DataGridCommandEventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    // This is a little kludgey, but it works
    uint trackId = uint.Parse(e.Item.Cells[1].Text);
    Track t = database.GetTrack(trackId);
    if (t.TrackId == trackId)
      database.AddVote(User.Identity.Name, trackId);

    StatusBar.Text = string.Format("Voted for {0}.", t.Title);
  }

  public void VoteAll_Click(object o, EventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    foreach (DataGridItem d in ResultsData.Items)
    {
      // This is a little kludgey, but it works
      uint trackId = uint.Parse(d.Cells[1].Text);
      Track t = database.GetTrack(trackId);
      if (t.TrackId == trackId)
        database.AddVote(User.Identity.Name, trackId);
    }

    StatusBar.Text = string.Format("Voted for {0} tracks.", ResultsData.Items.Count);
  }

  public void Search_Click(object o, EventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    Track[] tracks = database.GetTracks(SongTitle.Text, Artist.Text, Album.Text);
    if (tracks.Length == 0)
    {
      StatusBar.Text = "0 tracks found.  You apparently have bad taste in music.";
      ResultsData.Visible = false;
      VoteAll.Visible = false;
      return;
    }
    else
    {
      StatusBar.Text = string.Format("Showing {0} tracks.", tracks.Length);
      ResultsData.Visible = true;
      VoteAll.Visible = true;
    }

    DataView dataView = new DataView();
    dataView.Table = new DataTable("Tracks");
    dataView.Table.Columns.Add(new DataColumn("TrackId"));
    dataView.Table.Columns.Add(new DataColumn("Title"));
    dataView.Table.Columns.Add(new DataColumn("Album"));
    dataView.Table.Columns.Add(new DataColumn("Length"));
    foreach (Track t in tracks)
    {
      DataRowView rowView = dataView.AddNew();
      rowView["TrackId"] = t.TrackId;
      rowView["Title"] = t.Title;
      rowView["Album"] = t.Album;
      rowView["Length"] = t.Length.ToString();
      rowView.EndEdit();
    }

    ResultsData.DataSource = dataView;
    ResultsData.DataBind();
  }
}

public class AddPage : Page
{
  public FileUpload FileUploader;
  public Button UploadButton;
  public Panel UploadPanel;
  public Label UploadStatusBar;
  public Panel ScanPanel;
  public Button ScanButton;
  public Label ScanDirectory;
  public Label ScanStatusBar;

  public void Page_Load(object o, EventArgs e)
  {
    if (!IsPostBack)
    {
      UploadPanel.Visible = (ConfigurationManager.AppSettings["UploadDirectory"] != null);
      ScanPanel.Visible = (ConfigurationManager.AppSettings["ScanDirectory"] != null);
      ScanDirectory.Text = ConfigurationManager.AppSettings["ScanDirectory"] + Path.DirectorySeparatorChar + User.Identity.Name;
    }
  }

  public void UploadButton_Click(object o, EventArgs e)
  {
    if (FileUploader.HasFile)
    {
      Track t;
      string filename = Path.GetFileName(FileUploader.FileName);
      string uploadDirectory = ConfigurationManager.AppSettings["UploadDirectory"] + Path.DirectorySeparatorChar;

      // Mono doesn't do this correctly
      int index = filename.LastIndexOf('\\');
      if (index > -1)
      {
        filename = filename.Substring(index + 1);
      }

      try
      {
        t = TagReader.ReadStream(filename, FileUploader.FileContent);
      }
      catch (Exception)
      {
        UploadStatusBar.Text = "Could not read tags from file.";
        return;
      }

      try
      {
        int counter = 2;
        string savePath = uploadDirectory + filename;

        while (File.Exists(savePath))
        {
          savePath = uploadDirectory + Path.GetFileNameWithoutExtension(filename) + (counter++).ToString()
            + Path.GetExtension(filename);
        }

        FileUploader.SaveAs(savePath);
        UploadStatusBar.Text = "Uploaded file as " + Path.GetFileName(savePath);

        INeztuDatabase database = DatabaseHelper.GetDatabase();
        t.Filename = savePath;
        database.AddTrack(t);
      }
      catch (Exception ex)
      {
        UploadStatusBar.Text = "Could not save file.";
        Console.Error.WriteLine(ex.Message + ex.StackTrace);
      }
    }
  }

  public void ScanButton_Click(object o, EventArgs e)
  {
    if (Process.Start(ConfigurationManager.AppSettings["ScanCommand"], ConfigurationManager.AppSettings["ScanDirectory"] + Path.DirectorySeparatorChar + User.Identity.Name) != null)
    {
      ScanStatusBar.Text = "Scanning has been initiated in the background.";
    }
    else
    {
      ScanStatusBar.Text = "Could not initiate scan.";
    }
  }
}

public class IndexPage : Page
{
  public Anthem.DataGrid QueueData;
  public DataGrid RandomTracks;
  public Button MoreTracks;
  public Label StatusBar;

  public void Page_PreRender(object o, EventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();
    IScheduler sched = DatabaseHelper.GetScheduler();

    DataView dataView = new DataView();
    dataView.Table = new DataTable("Queue");
    dataView.Table.Columns.Add(new DataColumn("Track"));
    dataView.Table.Columns.Add(new DataColumn("Album"));
    dataView.Table.Columns.Add(new DataColumn("Length"));
    dataView.Table.Columns.Add(new DataColumn("Requested By"));
    dataView.Table.Columns.Add(new DataColumn("Status"));

    Vote[] pending = database.GetPending();
    foreach (Vote v in pending)
    {
      DataRowView rowView = dataView.AddNew();
      rowView["Track"] = v.ReqTrack.Title;
      rowView["Album"] = v.ReqTrack.Album;
      rowView["Length"] = v.ReqTrack.Length.ToString();
      rowView["Requested By"] = v.UserName;
      rowView["Status"] = "Pending";
      rowView.EndEdit();
    }

    Vote[] queue = sched.GetSchedule();
    foreach (Vote v in queue)
    {
      DataRowView rowView = dataView.AddNew();
      rowView["Track"] = v.ReqTrack.Title;
      rowView["Album"] = v.ReqTrack.Album;
      rowView["Length"] = v.ReqTrack.Length.ToString();
      rowView["Requested By"] = v.UserName;
      rowView["Status"] = "Queued";
      rowView.EndEdit();
    }
    QueueData.DataSource = dataView;
    QueueData.DataBind();

    if (!Anthem.Manager.IsCallBack && !IsPostBack)
    {
      if (database is IRandomizableTrackDatabase)
      {
        Track[] tracks = ((IRandomizableTrackDatabase)database).GetRandomTracks(10);

        dataView = new DataView();
        dataView.Table = new DataTable("Tracks");
        dataView.Table.Columns.Add(new DataColumn("TrackId"));
        dataView.Table.Columns.Add(new DataColumn("Title"));
        dataView.Table.Columns.Add(new DataColumn("Artist"));
        dataView.Table.Columns.Add(new DataColumn("Album"));
        dataView.Table.Columns.Add(new DataColumn("Length"));
        foreach (Track t in tracks)
        {
          DataRowView rowView = dataView.AddNew();
          rowView["TrackId"] = t.TrackId;
          rowView["Title"] = t.Title;
          rowView["Artist"] = t.Artist;
          rowView["Album"] = t.Album;
          rowView["Length"] = t.Length.ToString();
          rowView.EndEdit();
        }

        RandomTracks.DataSource = dataView;
        RandomTracks.DataBind();
      }
    }
  }

  public void RandomTracks_Command(object o, DataGridCommandEventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    // This is a little kludgey, but it works
    uint trackId = uint.Parse(e.Item.Cells[1].Text);
    Track t = database.GetTrack(trackId);
    if (t.TrackId == trackId)
      database.AddVote(User.Identity.Name, trackId);

    StatusBar.Text = string.Format("Voted for {0}.", t.Title);
  }

  public void MoreTracks_Click(object o, EventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    if (database is IRandomizableTrackDatabase)
    {
      Track[] tracks = ((IRandomizableTrackDatabase)database).GetRandomTracks(10);

      DataView dataView = new DataView();
      dataView.Table = new DataTable("Tracks");
      dataView.Table.Columns.Add(new DataColumn("TrackId"));
      dataView.Table.Columns.Add(new DataColumn("Title"));
      dataView.Table.Columns.Add(new DataColumn("Artist"));
      dataView.Table.Columns.Add(new DataColumn("Album"));
      dataView.Table.Columns.Add(new DataColumn("Length"));
      foreach (Track t in tracks)
      {
        DataRowView rowView = dataView.AddNew();
        rowView["TrackId"] = t.TrackId;
        rowView["Title"] = t.Title;
        rowView["Artist"] = t.Artist;
        rowView["Album"] = t.Album;
        rowView["Length"] = t.Length.ToString();
        rowView.EndEdit();
      }

      RandomTracks.DataSource = dataView;
      RandomTracks.DataBind();
    }
  }
}

public class ReadTagPage : Page
{
  public TextBox theTextBox;
  public Label theLabel;

  public void SubmitBtn_Click(object sender, EventArgs e)
  {
    if (IsPostBack)
    {
      Track t = TagReader.ReadFile(theTextBox.Text);
      theLabel.Text = t.Title;
    }
  }
}

public class LoginPage : Page
{
  public void Page_Load(object sender, EventArgs e)
  {
    if (ConfigurationManager.AppSettings["ExternalAuthEnabled"] != null
        && ConfigurationManager.AppSettings["ExternalAuthEnabled"].ToLower() == "true")
    {
      string user = Request.ServerVariables["REMOTE_USER"];
      if (user != null && user != "")
      {
        if (Membership.GetUser(user, false) == null)
        {
          byte[] random = new byte[32];
          RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
          rng.GetBytes(random);

          string password = "";
          foreach (byte b in random)
          {
            password += b;
          }

          Membership.CreateUser(user, password);
        }

        FormsAuthentication.RedirectFromLoginPage(user, false);
      }
    }
  }
}

public class MasterPage : System.Web.UI.MasterPage
{
  public Panel DaemonControls;
  public Anthem.Label NowPlayingTrack;
  public Anthem.Label NowPlayingArtist;
  public Anthem.Timer NowPlayingTimer;

  public void Page_PreRender(object o, EventArgs e)
  {
    INeztuDatabase database = DatabaseHelper.GetDatabase();

    Vote v = database.GetCurrent();
    if (v.ReqTrack.TrackId == 0)
    {
      NowPlayingTrack.Text = "";
      NowPlayingArtist.Text = "";
      NowPlayingTimer.StopTimer();
    }
    else
    {
      NowPlayingTrack.Text = v.ReqTrack.Title;
      NowPlayingArtist.Text = v.ReqTrack.Artist;
      int interval = (int)((v.ReqTrack.Length - (DateTime.Now - v.Timestamp)).TotalSeconds * 1000 + 2000);
      if (interval < 2000)
      {
        // Something is borked.  Tell the client to check back in 10 seconds and hopefully it will fix itself.
        interval = 10000;
      }

      NowPlayingTimer.Interval = interval;
      NowPlayingTimer.StartTimer();
    }

    DaemonControls.Visible = Context.User.IsInRole("Administrators");
  }

  public void NowPlayingTimer_Tick(object o, EventArgs e)
  {
    // All the actual work is done in Page_PreRender
  }

  public void StartButton_Click(object o, EventArgs e)
  {
    if (Context.User.IsInRole("Administrators"))
    {
      Process.Start(ConfigurationManager.AppSettings["StartCommand"], ConfigurationManager.AppSettings["StartArgs"]);
      // Hack to get proper "Now Playing" output
      Thread.Sleep(1000);
    }
  }

  public void StopButton_Click(object o, EventArgs e)
  {
    if (Context.User.IsInRole("Administrators"))
    {
      INeztuDatabase database = DatabaseHelper.GetDatabase();
      database.AddHistory(string.Empty, 0);

      Process.Start(ConfigurationManager.AppSettings["StopCommand"], ConfigurationManager.AppSettings["StopArgs"]);
    }
  }

  public void SkipButton_Click(object o, EventArgs e)
  {
    if (Context.User.IsInRole("Administrators"))
    {
      Process.Start(ConfigurationManager.AppSettings["SkipCommand"], ConfigurationManager.AppSettings["SkipArgs"]);
      // Hack to get proper "Now Playing" output
      Thread.Sleep(1000);
    }
  }
}
