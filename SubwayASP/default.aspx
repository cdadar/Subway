<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SubwayASP._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>北京地铁查询系统</title>
    <script src="App_Themes/default/jsRightMenu.js" type="text/javascript"></script>
    <meta content="text/html; charset=uf-8" http-equiv="Content-Type" />
    <link rel ="Stylesheet" type="text/css" href="App_Themes/default/default.css" />
</head>
<body>
    <form id="form1" runat="server">

        <div>
        <div class="bigBorder">
            <div class="topArea">
                <table border="0" cellspacing="0" cellpadding="0" align="center">
                    <tbody>
                        <tr>
                            <td>
                                起始站：<asp:DropDownList ID="ddlBegin" runat="server" Width="133px">
                                </asp:DropDownList>
                               
                                终点站：<asp:DropDownList ID="ddlEnd" runat="server" Width="133px">
                                </asp:DropDownList>
                                选项：<asp:DropDownList ID="ddlOptions" runat="server" Width="133px">
                                <asp:ListItem Text ="用时最短路线查询" Value ="0"></asp:ListItem>
                                <asp:ListItem Text ="换乘最少路线查询" Value ="1"></asp:ListItem>
                                </asp:DropDownList>
                                
                                &nbsp;&nbsp;
                                <asp:Button ID="btnQuery" runat ="server" Text="查询" Width="55px" 
                                    onclick="btnQuery_Click" />
                                
                                &nbsp;&nbsp; 显示比例：<asp:DropDownList ID="ddlSize" runat="server" Width="80px" 
                                    AutoPostBack=true onselectedindexchanged="ddlSize_SelectedIndexChanged">
                                <asp:ListItem Text="适合尺寸" Value="1"></asp:ListItem>
                                <asp:ListItem Text="25%" Value="25"></asp:ListItem>
                                <asp:ListItem Text="50%" Value="50"></asp:ListItem>
                                <asp:ListItem Text="75%" Value="75"></asp:ListItem>
                                <asp:ListItem Text="100%" Value="100"></asp:ListItem>
                                <asp:ListItem Text="125%" Value="125"></asp:ListItem>
                                <asp:ListItem Text="150%" Value="150"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td align="left">
                                <asp:RadioButtonList ID="rdoView" runat ="server" RepeatDirection="Horizontal" 
                                    AutoPostBack=true onselectedindexchanged="rdoView_SelectedIndexChanged">
                                    <asp:ListItem Text="预览视图" Value="0"></asp:ListItem>
                                    <asp:ListItem Text="行程视图" Value="1"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="mapArea">
                <div id="d" class="map" runat="server">
                    <asp:Image runat ="server" ID="myMap" CssClass="map"/>
                    
                    <asp:Panel ID="PnlXCView" runat ="server" Visible="false" BorderStyle="None" Width="651px">
                        <table cellpadding="0" cellspacing="0" border="0" width="100%" style="background:App_Themes/default/images/gdtop.png">
                            <tr>
                                <td style="width:55px">&nbsp;</td>
                                <td style="height:54; vertical-align:middle">
                                    <asp:Label ID="txtPathDsc" Text="用时最少路径" Font-Size="14" ForeColor="#FF2832E8" Font-Bold="true" runat ="server"></asp:Label>
                                    <asp:Label ID="labtime" Text="时间" Font-Size="14" ForeColor="#FF0D0D0D" Font-Bold="true" runat ="server"></asp:Label>
                                    <asp:Label ID="txtUseTime" Font-Size="14" ForeColor="#FFF61111"  runat ="server"></asp:Label>
                                    <asp:Label ID="Label1" Text="分" Font-Size="14" ForeColor="#FF0D0D0D"  runat ="server"></asp:Label>
                                    <asp:Label ID="Label2" Text="| 票价" Font-Size="14" ForeColor="#FF0D0D0D"  runat ="server"></asp:Label>
                                    <asp:Label ID="Label3" Text="2" Font-Size="14" ForeColor="#FFF61111"  runat ="server"></asp:Label>
                                    <asp:Label ID="Label4" Text="元" Font-Size="14" ForeColor="#FF0D0D0D"  runat ="server"></asp:Label>
                                </td>
                            </tr>
                        </table>
                        <asp:Table runat="server" ID="tbData" Width="100%" CellPadding="0" CellSpacing="1" BorderStyle="None" BorderWidth="0" BackColor="Silver">
                            <asp:TableRow runat="server" Height="25px" BackColor="#FFD7CECE">
                                <asp:TableCell runat="server"></asp:TableCell>
                                <asp:TableCell runat="server" HorizontalAlign="Center" Text="时间"></asp:TableCell>
                                <asp:TableCell runat="server" Text=""></asp:TableCell>
                                <asp:TableCell runat="server" Text="&nbsp;&nbsp;路径"></asp:TableCell>
                                <asp:TableCell runat="server" HorizontalAlign="Center" Text="金额"></asp:TableCell>
                                <asp:TableCell runat="server" HorizontalAlign="Center" Text="相关信息链接"></asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </asp:Panel>
                </div>
            </div>
        </div>
       </div>

        <asp:HiddenField ID="hdUseTime" runat="server" Value="0" />
        <asp:HiddenField ID="hdMapData" runat="server"  Value=""/>
        <asp:Button ID="hdInvokeServer" runat="server" Text="Button" BorderStyle="None" 
            Width="0" Height="0" onclick="hdInvokeServer_Click" />

    </form>
    <script>
        var d = document.getElementById("d"), drag = false, _x, _y;
        d.onmousedown = function (e) {
            drag = true; d.style.position = "absolute";
            var e = e || window.event;
            _x = (e.x || e.clientX) - this.offsetLeft;
            _y = (e.y || e.clientY) - this.offsetTop;
        }
        document.onmousemove = function (e) {
            var e = e || window.event;
            if (!drag) return;
            d.style.left = (e.x || e.clientX) - _x + "px";
            d.style.top = (e.y || e.clientY) - _y + "px";
        }
        document.onmouseup = new Function("drag=false");

        window.onload = ShowRightMenu;
    </script>
</body>
</html>
