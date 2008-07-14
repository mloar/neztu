<%@Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
  <head runat="server">
    <title>Neztu!</title>
  </head>
  <body>
    <div id="header"><asp:Image Id="Banner" SkinId="Banner" runat="server" /></div>
    <form runat="server">
      <asp:CreateUserWizard ID="CreateUserWizard" runat="server" ContinueDestinationPageUrl="~/index.aspx" />
    </form>
  </body>
</html>
