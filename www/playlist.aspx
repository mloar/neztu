<%@ Page Language="C#" Inherits="PlayListPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
  <asp:DataGrid id="PlayListData" OnItemCommand="PlayListData_Command" Visible="false" runat="server">
    <Columns>
      <asp:ButtonColumn CommandName="Up" Text="Up" runat="server" />
      <asp:ButtonColumn CommandName="Down" Text="Down" runat="server" />
      <asp:ButtonColumn CommandName="Remove" Text="Remove" runat="server" />
    </Columns>
  </asp:DataGrid>
</asp:Content>
