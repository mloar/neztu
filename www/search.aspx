<%@ Page Language="C#" Inherits="SearchPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
  <table><tr><th>Title</th><th>Artist</th><th>Album</th><th></th></tr>
  <tr>
    <td><asp:TextBox id="SongTitle" runat="server" /></td>
    <td><asp:TextBox id="Artist" runat="server" /></td>
    <td><asp:TextBox id="Album" runat="server" /></td>
    <td><asp:Button id="Search" OnClick="Search_Click" Text="Search" runat="server" /></td>
  </tr>
  </table>
  <asp:DataGrid id="ResultsData" OnItemCommand="ResultsData_Command" visible="false" runat="server">
    <columns>
      <asp:ButtonColumn Text="Vote" CommandName="Vote"/>
    </columns>
  </asp:DataGrid>
  <asp:Button id="VoteAll" OnClick="VoteAll_Click" Text="Vote All" visible="false" runat="server" />
</asp:Content>
