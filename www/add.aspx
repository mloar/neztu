<%@ Page Language="C#" Inherits="AddPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
  <div id="uploadpanel">
    <asp:Panel Id="UploadPanel" runat="server">
      <strong>Upload New Tracks</strong><br/>
      <asp:FileUpload id="FileUploader" runat="server"></asp:FileUpload>
      <asp:Button id="UploadButton" Text="Upload" OnClick="UploadButton_Click" runat="server" />
      <asp:Label Id="UploadStatusBar" runat="server" />
    </asp:Panel>
  </div>
  <div id="scanpanel">
    <asp:Panel Id="ScanPanel" runat="server">
      <strong>Scan User Directory</strong><br/>
      <asp:Label Id="ScanDirectory" runat="server" />
      <asp:Button id="ScanButton" Text="Scan" OnClick="ScanButton_Click" runat="server" />
      <asp:Label Id="ScanStatusBar" runat="server" />
    </asp:Panel>
  </div>
</asp:Content>
