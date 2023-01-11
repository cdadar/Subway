using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BusinessObject;
using SubwayQuery.DataModel;

namespace SubwayASP
{
    public partial class _default : System.Web.UI.Page
    {
        BizPlanPath bizPlanPath = new BizPlanPath();
        RoutePlanResult ret = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                this.InitUI();
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitUI()
        {
            ArrayList arStations = bizPlanPath.GetStations();

            for (int i = 0; i < arStations.Count; i++)
            {
                Node sta = (Node)arStations[i];
                this.ddlBegin.Items.Add(new ListItem(sta.PathName + ":" + sta.StaName, sta.Id));
                this.ddlEnd.Items.Add(new ListItem(sta.PathName + ":" + sta.StaName, sta.Id));
            }

            this.ddlBegin.SelectedIndex = 0;
            this.ddlEnd.SelectedIndex = 0;

            this.myMap.ImageUrl = "GetMap.aspx";
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            #region
            string startSta = this.ddlBegin.SelectedValue;//获取始发站
            string targetSta = this.ddlEnd.SelectedValue;//获取终点站
            string strOption = this.ddlOptions.SelectedValue;//获取查询选项

            //验证用户选择的路线是否符合查询要求
            string tmpRet = "";
            if (bizPlanPath.ValideData(startSta, targetSta, ref tmpRet) == false)
            {
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Message",
                    "<script>alert('" + tmpRet + "');</script>");
                return;
            }

            //调用业务外观进行路线查询
            ret=bizPlanPath.GetPlanPath(startSta,targetSta,strOption);
            if(ret==null)
            {
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Message",
                    "<script>alert('没有找到合适的路线！');</script>");
                return;
            }
            //显示标记出现路线后的地铁线路
            string sessionId = new Guid().ToString();
            Session[sessionId] = ret;
            ViewState["currSid"] = sessionId;
            myMap.ImageUrl = "GetMap.aspx?curSessionId=" + sessionId;
            //设置UI元素
            this.myMap.Visible = true;//默认设置地铁路线可见
            this.PnlXCView.Visible = false;//默认设置行程表格视图不可见
            this.rdoView.SelectedIndex = 0;//设置预览视图和行程视图的单选钮状态，即选中预览视图
            #endregion

            this.hdUseTime.Value = ret.weight.ToString();
            this.hdMapData.Value = this.GetMapData(ret.passedNodeIds, ret.passedNodeIds[0]);
        }

        protected void ddlSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlSize.SelectedIndex == 0)
            {
                this.d.Style.Add("widht", "995px");
                this.d.Style.Add("height", "543px");
                this.myMap.Width = Unit.Pixel(955);
                this.myMap.Height = Unit.Pixel(543);
            }
            else
            {
                double s = (double)int.Parse(this.ddlSize.SelectedValue.ToString()) / 100;
                int w = (int)(2510 * s);
                int h = (int)(2110 * s);
                this.d.Style.Add("widht", w.ToString() + "px");
                this.d.Style.Add("height", h.ToString() + "px");
                this.myMap.Width = Unit.Pixel(w);
                this.myMap.Height = Unit.Pixel(h);
            }
        }

        protected void rdoView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdoView.SelectedIndex == 0)
            {
                this.myMap.Visible = true;
                this.PnlXCView.Visible = false;
            }
            else
            {
                this.myMap.Visible=false;
                this.PnlXCView.Visible=true;
                this.DispXCDetail();
            }
        }


        private void DispXCDetail()
        {
            if (ViewState["currSid"] == null) return;

            ret = (RoutePlanResult)Session[ViewState["currSid"].ToString()];
            for (int i = this.tbData.Rows.Count - 1; i > 0; i--)
            {
                this.tbData.Rows.RemoveAt(i);
            }

            //
            string strOption = this.ddlOptions.SelectedValue;
            if (strOption == "2")
                this.txtPathDsc.Text = "换乘最少路线：";
            else
                this.txtPathDsc.Text = "用时最短路线：";

            if (this.ret != null) this.txtUseTime.Text = ret.weight.ToString();

            //去掉重复的站点，因为在换乘路线查询算法中，如果有换乘发生，则会产生一些重复的换乘站的
            //为了本算法的统一性，先去掉这些重复站点
            IEnumerable passNodes = this.ret.passedNodeIds.Distinct();
            Queue paths = new Queue();
            foreach (string v in passNodes)
            {
                paths.Enqueue(v);
            }
            Node priv, next;
            TableRow rowDef;
            Image img;
            Literal txt;
            double passTime = 0;
            Node tmpPriv;
            int passStationCount = 0;
            TableCell cel;

            priv = bizPlanPath.GetNode(paths.Dequeue().ToString());
            tmpPriv = priv.Clone();
            int row = 1;

            #region
            rowDef = new TableRow();
            rowDef.Height = Unit.Pixel(25);
            this.SetRow(rowDef, row);
            this.tbData.Rows.Add(rowDef);

            //0 column
            rowDef.Cells.Add(new TableCell());

            //1
            rowDef.Cells.Add(new TableCell());

            //2 column
            cel = new TableCell();
            cel.HorizontalAlign = HorizontalAlign.Center;
            img = this.GetImage("station.png", 14, 15);
            cel.Controls.Add(img);
            rowDef.Cells.Add(cel);

            //3 column
            cel = new TableCell();
            txt = new Literal();
            txt.Text = "【" + priv.PathName + "】 " + priv.StaName;
            cel.Controls.Add(txt);
            rowDef.Cells.Add(cel);

            //4 column
            cel = new TableCell();
            cel.HorizontalAlign = HorizontalAlign.Center;
            txt = new Literal();
            txt.Text = "2元";
            cel.Controls.Add(txt);
            rowDef.Cells.Add(cel);

            //5 column
            cel = this.GetOperateColumn(ret.passedNodeIds, priv.Id);
            cel.HorizontalAlign = HorizontalAlign.Center;
            rowDef.Cells.Add(cel);
            #endregion

            while (paths.Count > 0)
            {
                next = bizPlanPath.GetNode(paths.Peek().ToString());

                if (paths.Count > 0)
                {
                    tmpPriv = priv.Clone();
                    passTime += bizPlanPath.GetWeight(priv, next);
                    priv = next.Clone();
                    paths.Dequeue();
                    passStationCount++;
                }
                if (paths.Count > 0 && tmpPriv.PathId != next.PathId)
                {
                    row++;
                    #region 正常路线
                    rowDef = new TableRow();
                    rowDef.Height = Unit.Pixel(25);
                    this.SetRow(rowDef, row);
                    this.tbData.Rows.Add(rowDef);

                    //0 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    img = this.GetImage("subway.png", 15, 14);
                    cel.Controls.Add(img);
                    rowDef.Cells.Add(cel);

                    //1 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    txt = new Literal();
                    txt.Text = passTime.ToString() + "分";
                    cel.Controls.Add(txt);
                    rowDef.Cells.Add(cel);


                    //2 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    img = this.GetImage("A" + tmpPriv.PathId + ".png", 6, 25);
                    cel.Controls.Add(img);
                    rowDef.Cells.Add(cel);

                    //3 column
                    cel = new TableCell();
                    txt = new Literal();
                    txt.Text = "【" + tmpPriv.PathName + "】 " + tmpPriv.StaName + "（全程）方向（" + passStationCount.ToString() + "）"; ;
                    cel.Controls.Add(txt);
                    rowDef.Cells.Add(cel);

                    //4 column
                    rowDef.Cells.Add(new TableCell());

                    //5 column
                    rowDef.Cells.Add(new TableCell());
                    #endregion

                    row++;
                    #region  换乘路线
                    rowDef = new TableRow();
                    rowDef.Height = Unit.Pixel(25);
                    this.SetRow(rowDef, row);
                    this.tbData.Rows.Add(rowDef);

                    //0 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    img = this.GetImage("person.png", 15, 14);
                    cel.Controls.Add(img);
                    rowDef.Cells.Add(cel);

                    //1 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    txt = new Literal();
                    txt.Text = bizPlanPath.GetWeight(tmpPriv, priv).ToString() + "分";
                    cel.Controls.Add(txt);
                    rowDef.Cells.Add(cel);

                    //2 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    img = this.GetImage("transfer.png", 13, 12);
                    cel.Controls.Add(img);
                    rowDef.Cells.Add(cel);

                    //3 column
                    cel = new TableCell();
                    txt = new Literal();
                    txt.Text = "【" + next.StaName + "】 （经由" + tmpPriv.PathName + "-" + tmpPriv.StaName + "方向）换乘";
                    cel.Controls.Add(txt);
                    rowDef.Cells.Add(cel);

                    //4 column
                    rowDef.Cells.Add(new TableCell());

                    //5 column
                    cel = this.GetOperateColumn(ret.passedNodeIds, next.Id);
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    rowDef.Cells.Add(cel);
                    #endregion

                    passStationCount = 0;
                    passTime = 0;
                }
                if (paths.Count == 0)
                {
                    row++;
                    #region 正常路线
                    rowDef = new TableRow();
                    rowDef.Height = Unit.Pixel(25);
                    this.SetRow(rowDef, row);
                    this.tbData.Rows.Add(rowDef);

                    //0 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    img = this.GetImage("subway.png", 15, 14);
                    cel.Controls.Add(img);
                    rowDef.Cells.Add(cel);

                    //1 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    txt = new Literal();
                    txt.Text = passTime.ToString() + "分";
                    cel.Controls.Add(txt);
                    rowDef.Cells.Add(cel);

                    //2 column
                    cel = new TableCell();
                    cel.HorizontalAlign = HorizontalAlign.Center;
                    img = this.GetImage("A" + tmpPriv.PathId + ".png", 6, 25);
                    cel.Controls.Add(img);
                    rowDef.Cells.Add(cel);

                    //3 column
                    cel = new TableCell();
                    txt = new Literal();
                    txt.Text = "【" + priv.PathName + "】 " + priv.StaName + "（全程）方向（" + passStationCount.ToString() + "）"; ;
                    cel.Controls.Add(txt);
                    rowDef.Cells.Add(cel);

                    //4 column
                    rowDef.Cells.Add(new TableCell());

                    //5 column
                    rowDef.Cells.Add(new TableCell());
                    #endregion

                }
            }

            //输出结束站点
            row++;
            #region
            rowDef = new TableRow();
            rowDef.Height = Unit.Pixel(25);
            this.SetRow(rowDef, row);
            this.tbData.Rows.Add(rowDef);

            //0 column
            rowDef.Cells.Add(new TableCell());

            //1 column
            rowDef.Cells.Add(new TableCell());

            //2 column
            cel = new TableCell();
            cel.HorizontalAlign = HorizontalAlign.Center;
            img = this.GetImage("station.png", 14, 15);
            cel.Controls.Add(img);
            rowDef.Cells.Add(cel);

            //3 column
            cel = new TableCell();
            txt = new Literal();
            txt.Text = "【" + priv.PathName + "】 " + priv.StaName;
            cel.Controls.Add(txt);
            rowDef.Cells.Add(cel);

            //4 column
            rowDef.Cells.Add(new TableCell());

            //5 column
            cel = this.GetOperateColumn(ret.passedNodeIds, priv.Id);
            cel.HorizontalAlign = HorizontalAlign.Center;
            rowDef.Cells.Add(cel);
            #endregion
        }

        /// <summary>
        ///依据美工的设计，设置表格行的背景 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowIndex"></param>
        private void SetRow(TableRow row, int rowIndex)
        {
            if (rowIndex % 2 == 0)
            {
                row.BackColor = System.Drawing.Color.FromArgb(Convert.ToByte("FF", 16), Convert.ToByte("FF", 16), Convert.ToByte("FF", 16), Convert.ToByte("FF", 16));

            }
            else
                row.BackColor = System.Drawing.Color.FromArgb(Convert.ToByte("FF", 16), Convert.ToByte("F0", 16), Convert.ToByte("F0", 16), Convert.ToByte("EF", 16));
        }
        /// <summary>
        /// 表格中有的单元格要显示图片，这里创建一个image对象
        /// </summary>
        /// <param name="imgName"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        private Image GetImage(string imgName, int w, int h)
        {
            Image img = new Image();
            img.Width = Unit.Pixel(w);
            img.Height = Unit.Pixel(h);
            img.ImageUrl = "App_Themes/default/images/" + imgName;
            img.BorderStyle = BorderStyle.None;
            return img;
        }
        /// <summary>
        /// 获得传递到显示Google地图页面的参数
        /// </summary>
        /// <param name="stationDatas">所有经过的站点</param>
        /// <param name="currentStation">当前站点</param>
        /// <returns>此次查询所有经过车站的ID使用逗号分割后组成的字符串</returns>
        private string GetMapData(string[] stationDatas, string currentStation)
        {
            string tmp = "";
            foreach (string s in stationDatas)
            {
                if (tmp == "") tmp = s;
                else
                    tmp += "," + s;
            }
            tmp += ";" + currentStation;
            return tmp;
        }

        private TableCell GetOperateColumn(string[] stationDatas, string currentStation)
        {
            string tmp = this.GetMapData(stationDatas, currentStation);
            TableCell cel = new TableCell();

            HyperLink lik = new HyperLink();
            lik.Text = "Google地图";
            lik.NavigateUrl = "GoogleMap.aspx?data=" + HttpUtility.HtmlEncode(tmp);
            cel.Controls.Add(lik);

            HyperLink lik2 = new HyperLink();
            lik2.Text = "时刻表";
            cel.Controls.Add(lik2);

            return cel;
        }

        protected void hdInvokeServer_Click(object sender, EventArgs e)
        {
            this.rdoView.SelectedIndex = 1;
            this.myMap.Visible = false;
            this.PnlXCView.Visible = true;
            this.DispXCDetail();
        }
    }
}