using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SubwayQuery.DataModel;
using BusinessObject;

namespace SubwayASP
{
    public partial class GoogleMap : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.InitStation();
                this.InitGoogleMap();
            }
        }

        protected void InitStation()
        {
            if (Request.QueryString["data"] != null)
            {
                string data = Request.QueryString["data"].ToString();
                //分析数据，显示地图
                if (data.Trim().Length > 0)
                {
                    string[] sp1 = data.Split(';');
                    
                    string[] allPassStations = sp1[0].Split(','); 
                    string curStation = sp1[1];//得到用户选择的车站

                    for (int i = 0; i < allPassStations.Length; i++)
                    {
                        Node station = new BizPlanPath().GetNode(allPassStations[i]);
                        ListItem itm = new ListItem(station.StaName, station.ID);
                        //正在定位车站的红色显示
                        if (station.ID == curStation)
                        {
                            itm.Selected = true;
                            itm.Attributes.Add("style", "color:red");
                        }
                        this.bltStations.Items.Add(itm);
                    }

                }
            }
        }

        protected void InitGoogleMap()
        {
            if (Request.QueryString["data"] != null)
            {
                string data = Request.QueryString["data"].ToString();
                //分析数据，显示地图
                if (data.Trim().Length > 0)
                {
                    string[] sp1 = data.Split(';');
                    
                    string[] allPassStations = sp1[0].Split(',');
                    string curStation = sp1[1];//得到用户选择的车站
                    string GPS = "";

                    Node station = new BizPlanPath().GetNode(curStation);
                    GPS = station.GPS;
                    string[] strsGps = GPS.Split(',');
                    this.googleMap.Attributes.Add("onload", "initialize(" + strsGps[0] + "," + strsGps[1] + ");");
                }
                else
                    this.googleMap.Attributes.Add("onload", "initialize(39.9493,116.3975);");
            }
        }

        protected void bltStations_Click(object sender, BulletedListEventArgs e)
        {
            this.bltStations.Items[e.Index].Attributes.Add("style", "color:red");
            ListItem list = this.bltStations.Items[e.Index];
            string data = Request.QueryString["data"].ToString().Split(';')[0];
            Response.Redirect("GoogleMap.aspx?data=" + data + ";" + list.Value.Trim());
        }
    }
}