using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Web.Profile;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Neztu;

public class IndexPage : Page
{
  public LoginView LoginView1;
  public void Page_Load(object o, EventArgs e)
  {
    if (Request.IsAuthenticated)
    {
      Label TheLabel = new Label();
      DataGrid dg = new DataGrid();
      foreach (Control c in LoginView1.Controls[0].Controls)
      {
        if (c is Label)
          TheLabel = (Label)c;
        if (c is DataGrid)
          dg = (DataGrid)c;
      }
      ITrackDatabase db = new PostgresTrackDatabase();
      dg.DataSource = db.GetViewofAll();
      dg.DataBind();
      if (!(Session["Number"] is int))
        Session["Number"] = 0;
      TheLabel.Text = ((int)Session["Number"]).ToString();
      Session["Number"] = ((int)Session["Number"]) + 1;
      if (Context.Profile.IsAnonymous)
      {
        TheLabel.Text += "anonymous profile";
      }
      else if (Membership.GetUser(Context.User.Identity.Name, false) == null)
      {
        TheLabel.Text += "you do not exist, creating you";
        // FIXME: this password should be randomly generated
        Membership.CreateUser(Context.User.Identity.Name, "fjdslafjdljfldsajfljds");
        Context.Profile["FirstName"] = "test";
        Context.Profile.Save();
      }
      else
      {
        TheLabel.Text += "FirstName: " + Context.Profile["FirstName"].ToString();
      }
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
        if (Membership.GetUser(Context.User.Identity.Name, false) == null)
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
