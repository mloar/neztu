<%@ Page Language="C#" Inherits="AddPage" MasterPageFile="~/neztu.master" %>
<asp:Content id="MainContent" ContentPlaceHolderID="Main" runat="server">
  <form id="UploadForm" runat="server">
    <asp:FileUpload id="FileUploader" runat="server"></asp:FileUpload>
    <asp:Button id="UploadButton" Text="Upload!" OnClick="UploadButton_Click" runat="server" />
  </form>
  <asp:Label id="TheLabel" runat="server" />
</asp:Content>
