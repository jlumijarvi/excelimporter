<%@ Page Title="Import Excel" Async="true" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ImportExcel.aspx.cs" Inherits="ExcelImporter.ImportExcel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <label class="label label-info">beta</label>
    <uc:ImportExcel ID="ImportExcelCtrl" runat="server"></uc:ImportExcel>
</asp:Content>
