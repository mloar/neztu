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
      IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();

      Vote[] myVotes = stateDb.GetVotes(User.Identity.Name);

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
    IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();

    Vote[] myVotes = stateDb.GetVotes(User.Identity.Name);

    switch(((LinkButton)e.CommandSource).CommandName)
    {
      case "Up":
        if (e.Item.ItemIndex > 0)
        {
          stateDb.SwapVotes(myVotes[e.Item.ItemIndex], myVotes[e.Item.ItemIndex - 1]);
        }
      break;
      case "Down":
        if (e.Item.ItemIndex < myVotes.Length - 1)
        {
          stateDb.SwapVotes(myVotes[e.Item.ItemIndex], myVotes[e.Item.ItemIndex + 1]);
        }
      break;
      case "Remove":
        stateDb.RemoveVote(myVotes[e.Item.ItemIndex].UserName, myVotes[e.Item.ItemIndex].ReqTrack.TrackId);
      break;
    }

    myVotes = stateDb.GetVotes(User.Identity.Name);

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
  public TextBox SongTitle;
  public TextBox Artist;
  public TextBox Album;
  public Button Search;
  public Button VoteAll;
  public DataGrid ResultsData;

  public void ResultsData_Command(object o, DataGridCommandEventArgs e)
  {
    ITrackDatabase trackDb = DatabaseHelper.GetTrackDatabase();
    IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();

    // This is a little kludgey, but it works
    uint trackId = uint.Parse(e.Item.Cells[1].Text);
    Track t = trackDb.GetTrack(trackId);
    if (t.TrackId == trackId)
      stateDb.AddVote(User.Identity.Name, trackId);

    ((MasterPage)Master).StatusBar.Text = string.Format("Voted for {0}.", t.Title);
  }

  public void VoteAll_Click(object o, EventArgs e)
  {
    ITrackDatabase trackDb = DatabaseHelper.GetTrackDatabase();
    IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();

    foreach (DataGridItem d in ResultsData.Items)
    {
      // This is a little kludgey, but it works
      uint trackId = uint.Parse(d.Cells[1].Text);
      Track t = trackDb.GetTrack(trackId);
      if (t.TrackId == trackId)
        stateDb.AddVote(User.Identity.Name, trackId);
    }

    ((MasterPage)Master).StatusBar.Text = string.Format("Voted for {0} tracks.", ResultsData.Items.Count);
  }

  public void Search_Click(object o, EventArgs e)
  {
    ITrackDatabase trackDb = DatabaseHelper.GetTrackDatabase();

    Track[] tracks = trackDb.GetTracks(SongTitle.Text, Artist.Text, Album.Text);
    if (tracks.Length == 0)
    {
      ((MasterPage)Master).StatusBar.Text = "0 tracks found.  You apparently have bad taste in music.";
      ResultsData.Visible = false;
      VoteAll.Visible = false;
      return;
    }
    else
    {
      ((MasterPage)Master).StatusBar.Text = string.Format("Showing {0} tracks.", tracks.Length);
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
  public Label TheLabel;
  public HtmlForm UploadForm;
  public void Page_Load(object o, EventArgs e)
  {
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
        TheLabel.Text = "Could not read tags from file.";
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
        TheLabel.Text = "Uploaded file as " + Path.GetFileName(savePath);

        ITrackDatabase trackDb = DatabaseHelper.GetTrackDatabase();
        t.Filename = savePath;
        trackDb.AddTrack(t);
      }
      catch (Exception ex)
      {
        TheLabel.Text = "Could not save file.";
        Console.Error.WriteLine(ex.Message + ex.StackTrace);
      }
    }
  }
}

public class IndexPage : Page
{
  public Label TheLabel;
  public DataGrid QueueData;
  public void Page_Load(object o, EventArgs e)
  {
    if (!IsPostBack)
    {
      IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();
      IScheduler sched = new FIFOScheduler(null, stateDb);

      Vote[] queue = sched.GetSchedule();
      DataView dataView = new DataView();
      dataView.Table = new DataTable("Queue");
      dataView.Table.Columns.Add(new DataColumn("Track"));
      dataView.Table.Columns.Add(new DataColumn("Album"));
      dataView.Table.Columns.Add(new DataColumn("Length"));
      foreach (Vote v in queue)
      {
        DataRowView rowView = dataView.AddNew();
        rowView["Track"] = v.ReqTrack.Title;
        rowView["Album"] = v.ReqTrack.Album;
        rowView["Length"] = v.ReqTrack.Length.ToString();
        rowView.EndEdit();
      }
      QueueData.DataSource = dataView;
      QueueData.DataBind();
      QueueData.Visible = true;
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
  public Label StatusBar;
  public Label NowPlayingTrack;
  public Label NowPlayingArtist;

  public void Page_PreRender(object o, EventArgs e)
  {
    IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();

    Vote v = stateDb.GetCurrent();
    if (v.ReqTrack.TrackId == 0)
    {
      NowPlayingTrack.Text = "";
      NowPlayingArtist.Text = "";
    }
    else
    {
      NowPlayingTrack.Text = v.ReqTrack.Title;
      NowPlayingArtist.Text = v.ReqTrack.Artist;
    }
  }

  public void StartButton_Click(object o, EventArgs e)
  {
    // TODO: check is authorized

    Process.Start(ConfigurationManager.AppSettings["StartCommand"], ConfigurationManager.AppSettings["StartArgs"]);
    // Hack to get proper "Now Playing" output
    Thread.Sleep(1000);
  }

  public void StopButton_Click(object o, EventArgs e)
  {
    // TODO: check is authorized
    IStateDatabase stateDb = DatabaseHelper.GetStateDatabase();
    stateDb.AddHistory(string.Empty, 0);

    Process.Start(ConfigurationManager.AppSettings["StopCommand"], ConfigurationManager.AppSettings["StopArgs"]);
  }

  public void SkipButton_Click(object o, EventArgs e)
  {
    // TODO: check is authorized

    Process.Start(ConfigurationManager.AppSettings["SkipCommand"], ConfigurationManager.AppSettings["SkipArgs"]);
    // Hack to get proper "Now Playing" output
    Thread.Sleep(1000);
  }
}
