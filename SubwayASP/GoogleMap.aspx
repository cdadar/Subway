<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GoogleMap.aspx.cs" Inherits="SubwayASP.GoogleMap" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta name ="viewport" content="initial-scale=16,user-scalable=yes" />
    <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>
    <script type="text/javascript">
        function initialize(gpsx, gpsy) {
            var latlng = new google.maps.LatLng(gpsx, gpsy);
            var myOptions = { zoom: 16, center: latlng, mapTypeId: google.maps.MapTypeId.ROADMAP };
            var map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);
        }
    </script>
</head>
<body topmargin="0" leftmargin="0" runat="server" id="googleMap">
    <form id="form1" runat="server">
    <div>
    <table cellpadding="0" cellspacing="0" border="0" width="910px" style="border: 1px outset #C0C0C0">
            <tr>
                <td style="border: 1px outset #C0C0C0">
                    <div style="width: 160px; height: 450px; overflow: auto;">
                        <asp:BulletedList ID="bltStations" runat="server" BulletStyle="Numbered" 
                            DisplayMode="LinkButton" onclick="bltStations_Click">
                            
                        </asp:BulletedList>
                    </div>
                </td>
                <td style="border: 1px outset #C0C0C0">
                	<div id="map_canvas" style="width: 750px; height: 450px"></div>                
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
