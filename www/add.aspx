<%@ Page Language="C#" Inherits="AddPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
  <asp:Panel Id="UploadPanel" runat="server">
    <asp:FileUpload id="FileUploader" runat="server"></asp:FileUpload>
    <asp:Button id="UploadButton" Text="Upload!" OnClick="UploadButton_Click" runat="server" />
    <asp:Label Id="StatusBar" runat="server" />
  </asp:Panel>
</asp:Content>
