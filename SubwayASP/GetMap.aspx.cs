using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.UI;
using BusinessObject;

namespace SubwayASP
{
    public partial class GetMap : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///     将服务器ASPX页面的内容发送到客户端ＩＥ
        /// </summary>
        /// <param name="writer">发送的内容对象</param>
        protected override void Render(HtmlTextWriter writer)
        {
            //将生成的图片发挥客户端
            var ms = new MemoryStream();
            var theBitmap = CreateImage();
            theBitmap.Save(ms, ImageFormat.Jpeg);
            Response.ClearContent(); //需要输出图像信息，要修改HTTP头
            Response.ContentType = "image/jpeg";
            Response.BinaryWrite(ms.ToArray());
            theBitmap.Dispose();
            ms.Close();
            ms.Dispose();
            Response.End();
        }

        private Bitmap CreateImage()
        {
            var dataPath = HttpContext.Current.Server.MapPath("App_Data/OK.png");
            var bitmap = new Bitmap(dataPath);
            if (Request.QueryString["curSessionId"] != null)
            {
                var bizPlanPath = new BizPlanPath();
                var color = Color.Red;
                var sid = Request.QueryString["curSessionId"];
                var ret = (RoutePlanResult)Session[sid];
                if (ret != null && ret.passedNodeIds.Length > 0)
                {
                    var g = Graphics.FromImage(bitmap);
                    var path = ret.passedNodeIds;
                    for (var i = 0; i < path.Length; i++)
                    {
                        var station = bizPlanPath.GetNode(path[i]);
                        var LeftUpCornerX = Convert.ToInt32(station.dMapX) - 10;
                        var LeftUpCornerY = Convert.ToInt32(station.dMapY) - 10;
                        var newRect = new Rectangle(LeftUpCornerX, LeftUpCornerY, 20, 20);
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