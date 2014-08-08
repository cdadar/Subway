using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using SubwayQuery.DataModel;
using BusinessObject;

namespace SubwayASP
{
    public partial class GetMap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 将服务器ASPX页面的内容发送到客户端ＩＥ
        /// </summary>
        /// <param name="writer">发送的内容对象</param>
        protected override void Render(HtmlTextWriter writer)
        {
            //将生成的图片发挥客户端
            MemoryStream ms = new MemoryStream();
            Bitmap theBitmap = CreateImage();
            theBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            Response.ClearContent();//需要输出图像信息，要修改HTTP头
            Response.ContentType = "image/jpeg";
            Response.BinaryWrite(ms.ToArray());
            theBitmap.Dispose();
            ms.Close();
            ms.Dispose();
            Response.End();
        }
        private Bitmap CreateImage()
        {
            string dataPath = System.Web.HttpContext.Current.Server.MapPath("App_Data/OK.png");
            Bitmap bitmap = new Bitmap(dataPath);
            if (Request.QueryString["curSessionId"] != null)
            {
                BizPlanPath bizPlanPath = new BizPlanPath();
                System.Drawing.Color color = System.Drawing.Color.Red;
                string sid = Request.QueryString["curSessionId"].ToString();
                RoutePlanResult ret = (RoutePlanResult)Session[sid];
                if (ret != null && ret.passedNodeIDs.Length > 0)
                {
                    Graphics g = Graphics.FromImage(bitmap);
                    string[] path = ret.passedNodeIDs;
                    for (int i = 0; i < path.Length; i++)
                    {
                        Node station = bizPlanPath.GetNode(path[i]);
                        Int32 LeftUpCornerX = Convert.ToInt32(station.dMapX) - 10;
                        Int32 LeftUpCornerY = Convert.ToInt32(station.dMapY) - 10;
                        System.Drawing.Rectangle newRect = new Rectangle(LeftUpCornerX, LeftUpCornerY, 20, 20);
                        //设置背景色
                        g.FillEllipse(new SolidBrush(color), newRect);
                    }
                    g.Dispose();
                }
            }
            return bitmap;
        }
    }
}