<%@ Page Language="C#" Inherits="ReadTagPage" %>
<!--#include file="header.inc"-->
<form runat="server">
<asp:Label id="theLabel" runat="server"/>
<asp:TextBox id="theTextBox" runat="server"/>
<asp:Button OnClick="SubmitBtn_Click" text="Read Tag" runat="server"/>
</form>
<!--#include file="footer.inc"-->
