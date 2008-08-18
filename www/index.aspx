<%@ Page Language="C#" Inherits="IndexPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
    <div id="statusbar"><asp:Label Id="StatusBar" runat="server" /></div>
    <div id="queuedata">
      <strong>Tracks in the queue:</strong>
      <anthem:DataGrid id="QueueData" UpdateAfterCallback="true" runat="server" />
    </div>
    <div id="randomtracks">
      <strong>Some random tracks for your voting pleasure:</strong>
      <asp:Button id="MoreTracks" OnClick="MoreTracks_Click" Text="More Tracks" runat="server" />
      <asp:DataGrid id="RandomTracks" AutoGenerateColumns="false"
          OnItemCommand="RandomTracks_Command" runat="server">
        <columns>
          <asp:ButtonColumn Text="Vote" CommandName="Vote"/>
          <asp:BoundColumn DataField="TrackId" Visible="false" />
          <asp:BoundColumn HeaderText="Title" DataField="Title" />
          <asp:BoundColumn HeaderText="Artist" DataField="Artist" />
          <asp:BoundColumn HeaderText="Album" DataField="Album" />
          <asp:BoundColumn HeaderText="Length" DataField="Length" />
        </columns>
      </asp:DataGrid>
    </div>
</asp:Content>
