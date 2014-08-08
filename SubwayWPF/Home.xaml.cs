using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SubwayQuery.DataModel;
using System.Collections;
using BusinessObject;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace SubwayWPF
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Window
    {
        RoutePlanResult ret = null;
        BizPlanPath bizPlanPath = new BizPlanPath();
        string googleMapUrl = "http://localhost:4169/GoogleMap.aspx?data=";

        public Home()
        {
            this.InitializeComponent();
            this.InitStation();
            this.InitDefaultMpa();
        }

        #region  初始化UI InitStation(),InitDefaultMpa()
        private void InitStation()
        {
            BizPlanPath bizPlanPath = new BizPlanPath();
            ArrayList arStations = bizPlanPath.GetStations(new string[1] { "WPF" });
            for (int i = 0; i < arStations.Count; i++)
            {
                Node sta = (Node)arStations[i];
                ComboBoxItem cb = new ComboBoxItem();
                cb.Content = sta.PathName + ":" + sta.StaName;
                cb.DataContext = sta.ID;
                cb.Name = "A" + i.ToString();
                cmbStart.Items.Add(cb);

                ComboBoxItem cb1 = new ComboBoxItem();
                cb1.Content = sta.PathName + ":" + sta.StaName;
                cb1.DataContext = sta.ID;
                cb1.Name = "A" + i.ToString();
                cmbTarget.Items.Add(cb1);

            }
            cmbStart.SelectedIndex = 0;
            cmbTarget.SelectedIndex = 0;
        }

        private void InitDefaultMpa()
        {
            Bitmap bitmap = new Bitmap("OK.png");
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            BitmapImage map = new BitmapImage();
            map.BeginInit();
            map.StreamSource = ms;
            map.EndInit();
            this.myMap.Source = map;

        }
        #endregion
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            string startSta = ((ComboBoxItem)this.cmbStart.SelectedItem).DataContext.ToString();
            string targetSta = ((ComboBoxItem)this.cmbTarget.SelectedItem).DataContext.ToString();
            string strOption = ((ComboBoxItem)this.cmbOption.SelectedItem).DataContext.ToString();
            string tmpRet = "";
            if (bizPlanPath.ValideData(startSta, targetSta, ref tmpRet, new string[] { "WPF" }) == false)
            {
                MessageBox.Show(tmpRet);
                return;
            }
            this.rdoYL.IsChecked = true;
            ret = bizPlanPath.GetPlanPath(startSta, targetSta, strOption, new string[] { "WPF" });
            this.DrawLine();
            this.PopupButton3.Content = "预计到达时间 ：" + ret.weight.ToString() + "分";
        }

        //标记路线
        private void DrawLine()
        {
            if (ret != null && ret.passedNodeIDs.Length > 0)
            {
                Bitmap bitmap = new Bitmap("OK.png");
                Graphics g = Graphics.FromImage(bitmap);

                string[] path = ret.passedNodeIDs;
                for (int i = 0; i < path.Length; i++)
                {
                    Node station = bizPlanPath.GetNode(path[i]);
                    System.Drawing.Rectangle newRect = new System.Drawing.Rectangle(
                        Convert.ToInt32(station.dMapX) - 10, Convert.ToInt32(station.dMapY) - 10, 20, 20);
                    //设置背景色
                    g.FillEllipse(new SolidBrush(System.Drawing.Color.Red), newRect);

                }
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                BitmapImage map = new BitmapImage();
                map.BeginInit();
                map.StreamSource = ms;
                map.EndInit();
                this.myMap.Source = map;
            }
        }
        //显示比例
        private void cmbSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            switch (cb.SelectedIndex)
            {
                case 0:
                    if (this.myMap == null) break;
                    this.myMap.Width = 1002;
                    this.myMap.Height = 650;
                    Canvas.SetLeft(this.myMap, 0);
                    Canvas.SetTop(this.myMap, 0);
                    break;
                case 1:
                    this.myMap.Width = 2510 * 0.25;
                    this.myMap.Height = 2110 * 0.25;
                    break;
                case 2:
                    this.myMap.Width = 2510 * 0.5;
                    this.myMap.Height = 2110 * 0.5;
                    break;
                case 3:
                    this.myMap.Width = 2510 * 0.75;
                    this.myMap.Height = 2110 * 0.75;
                    break;
                case 4:
                    this.myMap.Width = 2510;
                    this.myMap.Height = 2110;
                    break;
                case 5:
                    this.myMap.Width = 2510 * 1.25;
                    this.myMap.Height = 2110 * 1.25;
                    break;
                case 6:
                    this.myMap.Width = 2510 * 1.5;
                    this.myMap.Height = 2110 * 1.5;
                    break;
            }
        }

        #region 拖放

        bool IsMouseDown = false;
        System.Windows.Point mousePoint;
        object mouseCtrl = null;

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point theMousePoint = e.GetPosition(this.cavMapBase);

                    double tl = Canvas.GetLeft(((UIElement)mouseCtrl)) + (theMousePoint.X - mousePoint.X);
                    double tt = Canvas.GetTop(((UIElement)mouseCtrl)) + (theMousePoint.Y - mousePoint.Y);

                    Canvas.SetLeft((UIElement)mouseCtrl, tl);
                    Canvas.SetTop((UIElement)mouseCtrl, tt);
                    mousePoint = theMousePoint;
                }
            }
        }

        private void myMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsMouseDown = true;
                mousePoint = e.GetPosition(this.cavMapBase);
                mouseCtrl = sender;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseDown)
                IsMouseDown = false;

        }
        #endregion

        #region 预览视图，行程视图事件处理
        private void rdoYL_Click(object sender, RoutedEventArgs e)
        {
            this.grdData.Visibility = System.Windows.Visibility.Collapsed;
            this.myMap.Visibility = System.Windows.Visibility.Visible;
            this.grdBingMap.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void rdoXC_Click(object sender, RoutedEventArgs e)
        {
            this.myMap.Visibility = System.Windows.Visibility.Collapsed;
            this.grdData.Visibility = System.Windows.Visibility.Visible;
            this.grdBingMap.Visibility = System.Windows.Visibility.Collapsed;
            this.DispXCDetail();
        }
        #endregion
        #region 出行路线详细表格
        private void DispXCDetail()
        {
            for (int i = this.tbData.RowDefinitions.Count - 1; i > 0; i--)
            {
                this.tbData.RowDefinitions.RemoveAt(i);
            }

            //
            string strOption = ((ComboBoxItem)this.cmbOption.SelectedItem).DataContext.ToString();
            if (strOption == "2")
                this.txtPathDsc.Text = "换乘最少路线：";
            else
                this.txtPathDsc.Text = "用时最短路线：";

            if (this.ret != null) this.txtUseTime.Text = ret.weight.ToString();

            //去掉重复的站点，因为在换乘路线查询算法中，如果有换乘发生，则会产生一些重复的换乘站的
            //为了本算法的统一性，先去掉这些重复站点
            IEnumerable passNodes = this.ret.passedNodeIDs.Distinct();
            Queue paths = new Queue();
            foreach (string v in passNodes)
            {
                paths.Enqueue(v);
            }
            Node priv, next;
            RowDefinition rowDef;
            Border border;
            System.Windows.Controls.Image img;
            TextBlock txt;
            WrapPanel wp;
            double passTime = 0;
            Node tmpPriv;
            int passStationCount = 0;

            priv = bizPlanPath.GetNode(paths.Dequeue().ToString());
            tmpPriv = priv.Clone();
            int row = 1;

            #region
            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(25);
            this.tbData.RowDefinitions.Add(rowDef);

            //0 column
            border = this.GetBorder(row);

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 0);
            this.tbData.Children.Add(border);

            //1 column
            border = this.GetBorder(row);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, 1);

            this.tbData.Children.Add(border);


            //2 column
            border = this.GetBorder(row);
            img = this.GetImage("station.png", 14, 15);
            border.Child = img;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 2);

            this.tbData.Children.Add(border);

            //3 column
            border = this.GetBorder(row);
            txt = new TextBlock();
            txt.Text = "【" + priv.PathName + "】 " + priv.StaName;
            txt.VerticalAlignment = VerticalAlignment.Center;
            border.Child = txt;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 3);
            this.tbData.Children.Add(border);

            //4 column
            border = this.GetBorder(row);
            txt = new TextBlock();
            txt.Text = "2元";
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.HorizontalAlignment = HorizontalAlignment.Center;
            border.Child = txt;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 4);
            this.tbData.Children.Add(border);

            //5 column
            border = this.GetBorder(row);
            wp = this.GetOperateColumn(ret.passedNodeIDs, priv.ID);
            border.Child = wp;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 5);
            this.tbData.Children.Add(border);
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
                if (paths.Count > 0 && tmpPriv.PathID != next.PathID)
                {
                    row++;
                    #region 正常路线
                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(25);
                    this.tbData.RowDefinitions.Add(rowDef);

                    //0 column
                    border = this.GetBorder(row);
                    img = this.GetImage("subway.png", 15, 14);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    this.tbData.Children.Add(border);

                    //1 column
                    border = this.GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = passTime.ToString() + "分";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    border.Child = txt;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 1);

                    this.tbData.Children.Add(border);


                    //2 column
                    border = this.GetBorder(row);
                    img = this.GetImage("A" + tmpPriv.PathID + ".png", 6, 25);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 2);

                    this.tbData.Children.Add(border);

                    //3 column
                    border = this.GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = "【" + tmpPriv.PathName + "】 " + tmpPriv.StaName + "（全程）方向（" + passStationCount.ToString() + "）"; ;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = txt;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 3);
                    this.tbData.Children.Add(border);

                    //4 column
                    border = this.GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 4);
                    this.tbData.Children.Add(border);

                    //5 column
                    border = this.GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 5);
                    this.tbData.Children.Add(border);
                    #endregion

                    row++;
                    #region  换乘路线
                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(25);
                    this.tbData.RowDefinitions.Add(rowDef);

                    //0 column
                    border = this.GetBorder(row);
                    img = this.GetImage("person.png", 15, 14);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    this.tbData.Children.Add(border);

                    //1 column
                    border = this.GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = bizPlanPath.GetWeight(tmpPriv, priv).ToString() + "分";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    border.Child = txt;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 1);

                    this.tbData.Children.Add(border);


                    //2 column
                    border = this.GetBorder(row);
                    img = this.GetImage("transfer.png", 13, 12);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 2);

                    this.tbData.Children.Add(border);

                    //3 column
                    border = this.GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = "【" + next.StaName + "】 （经由" + tmpPriv.PathName + "-" + tmpPriv.StaName + "方向）换乘";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = txt;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 3);
                    this.tbData.Children.Add(border);

                    //4 column
                    border = this.GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 4);
                    this.tbData.Children.Add(border);

                    //5 column
                    border = this.GetBorder(row);
                    wp = this.GetOperateColumn(ret.passedNodeIDs, next.ID);
                    border.Child = wp;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 5);
                    this.tbData.Children.Add(border);
                    #endregion

                    passStationCount = 0;
                    passTime = 0;
                }
                if (paths.Count == 0)
                {
                    row++;
                    #region 正常路线
                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(25);
                    this.tbData.RowDefinitions.Add(rowDef);

                    //0 column
                    border = this.GetBorder(row);
                    img = this.GetImage("subway.png", 15, 14);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    this.tbData.Children.Add(border);

                    //1 column
                    border = this.GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = passTime.ToString() + "分";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    border.Child = txt;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 1);

                    this.tbData.Children.Add(border);


                    //2 column
                    border = this.GetBorder(row);
                    img = this.GetImage("A" + tmpPriv.PathID + ".png", 6, 25);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 2);

                    this.tbData.Children.Add(border);

                    //3 column
                    border = this.GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = "【" + priv.PathName + "】 " + priv.StaName + "（全程）方向（" + passStationCount.ToString() + "）"; ;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = txt;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 3);
                    this.tbData.Children.Add(border);

                    //4 column
                    border = this.GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 4);
                    this.tbData.Children.Add(border);

                    //5 column
                    border = this.GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 5);
                    this.tbData.Children.Add(border);
                    #endregion

                }
            }

            //输出结束站点
            row++;
            #region
            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(25);
            this.tbData.RowDefinitions.Add(rowDef);

            //0 column
            border = this.GetBorder(row);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, 0);
            this.tbData.Children.Add(border);

            //1 column
            border = this.GetBorder(row);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, 1);

            this.tbData.Children.Add(border);


            //2 column
            border = this.GetBorder(row);
            img = this.GetImage("station.png", 14, 15);
            border.Child = img;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 2);

            this.tbData.Children.Add(border);

            //3 column
            border = this.GetBorder(row);
            txt = new TextBlock();
            txt.Text = "【" + priv.PathName + "】 " + priv.StaName;
            txt.VerticalAlignment = VerticalAlignment.Center;
            border.Child = txt;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 3);
            this.tbData.Children.Add(border);

            //4 column
            border = this.GetBorder(row);

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 4);
            this.tbData.Children.Add(border);

            //5 column
            border = this.GetBorder(row);
            wp = this.GetOperateColumn(ret.passedNodeIDs, priv.ID);
            border.Child = wp;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 5);
            this.tbData.Children.Add(border);
            #endregion
        }
        private Border GetBorder(int flag)
        {
            Border border = new Border();
            border.Margin = new Thickness(0);
            border.BorderBrush=this.GetColorFromHex("#FFF4F0F0");
            border.BorderThickness = new Thickness(0.3);

            if (flag % 2 == 0)
                border.Background = this.GetColorFromHex("#00000000");
            else
                border.Background = this.GetColorFromHex("#FFF0F0EF");
            return border;
        }
        private SolidColorBrush GetColorFromHex(string myColor)
        {
            return new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(
                    Convert.ToByte(myColor.Substring(1, 2), 16),
                    Convert.ToByte(myColor.Substring(3, 2), 16),
                    Convert.ToByte(myColor.Substring(5, 2), 16),
                    Convert.ToByte(myColor.Substring(7, 2), 16)
                 )
            );
        }
        private System.Windows.Controls.Image GetImage(string imgName, int w, int h)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Width = w;
            img.Height = h;
            BitmapImage bitmapImg = new BitmapImage(new Uri("images/" + imgName, UriKind.Relative));
            img.Source = bitmapImg;

            return img;
        }
        private WrapPanel GetOperateColumn(string[] stationDatas, string currentStation)
        {
            WrapPanel wp = new WrapPanel();
            wp.Width = 115;
            wp.Height = 16;
            wp.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock txt1 = new TextBlock();
            txt1.Name = "txtBing";
            txt1.Text = "Google地图";
            txt1.TextWrapping = TextWrapping.Wrap;
            txt1.FontSize = 13.3;
            txt1.MouseUp += new MouseButtonEventHandler(this.txtBing_MouseUp);
            txt1.Cursor = Cursors.Hand;
            txt1.DataContext = this.GetMapData(stationDatas, currentStation);
            wp.Children.Add(txt1);

            TextBlock txt2 = new TextBlock();
            txt2.Name = "txtTime";
            txt2.Text = "时刻表";
            txt2.TextWrapping = TextWrapping.Wrap;
            txt2.FontSize = 13.3;
            txt2.Cursor = Cursors.Hand;
            txt2.Margin = new Thickness(15, 0, 0, 0);
            wp.Children.Add(txt2);
            return wp;
        }
        private void txtBing_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.myMap.Visibility = System.Windows.Visibility.Collapsed;
            this.grdData.Visibility = System.Windows.Visibility.Collapsed;
            this.grdBingMap.Visibility = System.Windows.Visibility.Visible;

            TextBlock txt1 = (TextBlock)sender;
            this.grdBingMap.Source = new Uri(this.googleMapUrl + txt1.DataContext.ToString());
        }
        private string GetMapData(string[] stationDatas, string currentStation)
        {
            string tmp = "";
            foreach (string s in stationDatas)
            {
                if (tmp == "")
                    tmp = s;
                else
                    tmp += "," + s;
            }
            tmp += ";" + currentStation;
            return tmp;
        }
        #endregion
        #region 实现菜单项
        private void myMap_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ConnectPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
            this.ConnectPopup.StaysOpen = false;
            this.ConnectPopup.IsOpen = true;
        }

        private void PopupButton1_Click(object sender, RoutedEventArgs e)
        {
            this.myMap.Visibility = System.Windows.Visibility.Collapsed;
            this.grdData.Visibility = System.Windows.Visibility.Collapsed;
            this.grdBingMap.Visibility = System.Windows.Visibility.Visible;
            this.ConnectPopup.IsOpen = false;
            this.grdBingMap.Source = new Uri(googleMapUrl);

        }

        private void PopupButton3_Click(object sender, RoutedEventArgs e)
        {
            this.myMap.Visibility = System.Windows.Visibility.Collapsed;
            this.grdData.Visibility = System.Windows.Visibility.Visible;
            this.ConnectPopup.IsOpen = false;
            this.DispXCDetail();
        }

        #endregion

    }
}
