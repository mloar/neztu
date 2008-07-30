<%@ Page Language="C#" Inherits="IndexPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
    <div id="statusbar"><asp:Label Id="StatusBar" runat="server" /></div>
    <div id="queuedata"><asp:DataGrid id="QueueData" runat="server" /></div>
    <div id="randomtracks">
      <asp:DataGrid id="RandomTracks" OnItemCommand="RandomTracks_Command" runat="server">
        <columns>
          <asp:ButtonColumn Text="Vote" CommandName="Vote"/>
        </columns>
      </asp:DataGrid>
    </div>
</asp:Content>
