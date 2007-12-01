using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
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
      ITrackDatabase trackDb = new PostgresTrackDatabase();
      IStateDatabase stateDb = new PostgresStateDatabase();
      stateDb.Initialize(trackDb);

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
    ITrackDatabase trackDb = new PostgresTrackDatabase();
    IStateDatabase stateDb = new PostgresStateDatabase();
    stateDb.Initialize(trackDb);

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

        ITrackDatabase trackDb = new PostgresTrackDatabase();
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
    ITrackDatabase db = new PostgresTrackDatabase();
    IStateDatabase stateDb = new PostgresStateDatabase();
    stateDb.Initialize(db);
    IScheduler sched = new FIFOScheduler();
    sched.Initialize(null, stateDb);

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

          Membership.CreateUser(Context.User.Identity.Name, password);
        }

        FormsAuthentication.RedirectFromLoginPage(user, false);
      }
    }
  }
}

public class MasterPage : System.Web.UI.MasterPage
{
  public Label NowPlayingTrack;
  public Label NowPlayingArtist;

  public void Page_Load(object o, EventArgs e)
  {
    ITrackDatabase trackDb = new PostgresTrackDatabase();
    IStateDatabase stateDb = new PostgresStateDatabase();
    stateDb.Initialize(trackDb);

    Vote v = stateDb.GetCurrent();
    NowPlayingTrack.Text = v.ReqTrack.Title;
    NowPlayingArtist.Text = v.ReqTrack.Artist;
  }

  public void StartButton_Click(object o, EventArgs e)
  {
    // TODO: check is authorized

    Process.Start(ConfigurationManager.AppSettings["StartCommand"], ConfigurationManager.AppSettings["StartArgs"]);
  }

  public void StopButton_Click(object o, EventArgs e)
  {
    // TODO: check is authorized

    Process.Start(ConfigurationManager.AppSettings["StopCommand"], ConfigurationManager.AppSettings["StopArgs"]);
  }

  public void SkipButton_Click(object o, EventArgs e)
  {
    // TODO: check is authorized

    Process.Start(ConfigurationManager.AppSettings["SkipCommand"], ConfigurationManager.AppSettings["SkipArgs"]);
  }
}
