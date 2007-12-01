<%@ Page Language="C#" Inherits="IndexPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
    <asp:Label id="TheLabel" runat="server" Text="Fuck you too" Visible="false" />
    <asp:DataGrid id="QueueData" runat="server" />
</asp:Content>
