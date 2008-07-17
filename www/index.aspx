<%@ Page Language="C#" Inherits="IndexPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
    <div id="statusbar"><asp:Label Id="StatusBar" runat="server" /></div>
    <asp:DataGrid id="QueueData" runat="server" />
</asp:Content>
