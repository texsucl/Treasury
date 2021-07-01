<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="SSO.Report.ReportViewer" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
            <style>
span.glyphui {
    margin: 1px;
}
.ToolbarPageNav input {
    margin: 1px;
}
.ToolbarRefresh.WidgetSet,
.ToolbarPrint.WidgetSet,
.ToolbarBack.WidgetSet,
.ToolbarPowerBI.WidgetSet {
    height: 32px;
}
.WidgetSet {
    height: 32px;
}
.HoverButton {
    height:32px;
}
.NormalButton {
    height:32px;
}
.NormalButton table,
.HoverButton table,
.aspNetDisabled table {
    width: 56px;
}
.DisabledButton {
    height:32px;
}
.ToolbarFind,
.ToolbarZoom {
    padding-top: 3px;
}
.ToolBarBackground {
    background-color: #bdbdbd!important;
}
    </style>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>

<body>
    <form id="form1" runat="server">
    <div  style="width:100%; height:800px">
           <asp:ScriptManager runat="server"></asp:ScriptManager> 
           <rsweb:ReportViewer ID="rptViewer" runat="server" Width="100%" Height="800px"> 
           </rsweb:ReportViewer>
    </div>
    </form>
</body>
</html>
