<%@ Page Language="C#" AutoEventWireup="true" CodeFile="etoDF.aspx.cs" Inherits="etoDF" %>


<!DOCTYPE html>
<script src="../Scripts/jquery-1.11.1.min.js"></script>
<script src="../Scripts/bootstrap.min.js"></script>
<%--<link href="../jquery.mobile-1.3.2/jquery.mobile-1.3.2.min.css" rel="stylesheet" type="text/css" />--%>
<link href="../Content/bootstrap.css" rel="stylesheet" type="text/css" />


<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<%--<body>--%>
    <nav class="navbar ">
            <div class="container">
        <div class="jumbotron">
                          <form id="form1" runat="server">
                              <div class="row">
  <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3 " style="margin-bottom:20px">
      <asp:Button ID="Button1" Font-Size="26px" CssClass="btn btn-primary"  runat="server" Height="100px" OnClick="Button1_Click" Text="e簽核" Width="165px" />
  </div>
  <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3 " style="margin-bottom:20px">
      <asp:Button ID="Button2" Font-Size="26px" CssClass="btn btn-primary"  runat="server" Height="100px" OnClick="Button1_Click" Text="e簽核2" Width="165px" />
  </div>
  <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3 " style="margin-bottom:20px">
      <asp:Button ID="Button3" Font-Size="26px" CssClass="btn btn-primary"  runat="server" Height="100px" OnClick="Button1_Click" Text="e簽核3" Width="165px" />
  </div>
  <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3 " style="margin-bottom:20px">
      <asp:Button ID="Button4" Font-Size="26px" CssClass="btn btn-primary"  runat="server" Height="100px" OnClick="Button1_Click" Text="e簽核4" Width="165px" />
  </div>
</div>    

    </form>
        </div>

    </div>
    </nav>


</body>
</html>
