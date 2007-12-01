<%@ Page Language="C#" Inherits="SearchPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
  <form id="SearchForm" runat="server">
    <asp:TextBox id="Title" runat="server" />
    <asp:TextBox id="Artist" runat="server" />
    <asp:TextBox id="Album" runat="server" />
    <asp:Button id="Search" runat="server" />
    <asp:DataGrid id="ResultsData" OnItemCommand="ResultsData_Command" runat="server" />
  </form>
</asp:Content>
