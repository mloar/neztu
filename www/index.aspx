<%@ Page Language="C#" Inherits="IndexPage" %>
<!--#include file="header.inc"-->
<asp:LoginView id="LoginView1" runat="server">
  <AnonymousTemplate>
    <form runat="server">
      <asp:LoginStatus id="LoginStatus1" runat="server" />
    </form>
  </AnonymousTemplate>
  <LoggedInTemplate>
    <asp:Label id="Label1" runat="server"/>
    <asp:LoginName id="LoginName1" runat="server" FormatString="Welcome, {0}" />
    <asp:DataGrid runat="server" />
  </LoggedInTemplate>
</asp:LoginView>

<!--#include file="footer.inc"-->
