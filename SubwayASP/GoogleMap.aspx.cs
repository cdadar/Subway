using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using BusinessObject;

namespace SubwayASP
{
    public partial class GoogleMap : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitStation();
                InitGoogleMap();
            }
        }

        protected void InitStation()
        {
            if (Request.QueryString["data"] != null)
            {
                var data = Request.QueryString["data"];
                //分析数据，显示地图
                if (data.Trim().Length > 0)
                {
                    var sp1 = data.Split(';');

                    var allPassStations = sp1[0].Split(',');
                    var curStation = sp1[1]; //得到用户选择的车站

                    for (var i = 0; i < allPassStations.Length; i++)
                    {
                        var station = new BizPlanPath().GetNode(allPassStations[i]);
                        var itm = new ListItem(station.StaName, station.Id);
                        //正在定位车站的红色显示
                        if (station.Id == curStation)
                        {
                            itm.Selected = true;
                            itm.Attributes.Add("style", "color:red");
                        }

                        bltStations.Items.Add(itm);
                    }
                }
            }
        }

        protected void InitGoogleMap()
        {
            if (Request.QueryString["data"] != null)
            {
                var data = Request.QueryString["data"];
                //分析数据，显示地图
                if (data.Trim().Length > 0)
                {
                    var sp1 = data.Split(';');

                    var allPassStations = sp1[0].Split(',');
                    var curStation = sp1[1]; //得到用户选择的车站
                    var GPS = "";

                    var station = new BizPlanPath().GetNode(curStation);
                    GPS = station.GPS;
                    var strsGps = GPS.Split(',');
                    googleMap.Attributes.Add("onload", "initialize(" + strsGps[0] + "," + strsGps[1] + ");");
                }
                else
                {
                    googleMap.Attributes.Add("onload", "initialize(39.9493,116.3975);");
                }
            }
        }

        protected void bltStations_Click(object sender, BulletedListEventArgs e)
        {
            bltStations.Items[e.Index].Attributes.Add("style", "color:red");
            var list = bltStations.Items[e.Index];
            var data = Request.QueryString["data"].Split(';')[0];
            Response.Redirect("GoogleMap.aspx?data=" + data + ";" + list.Value.Trim());
        }
    }
}