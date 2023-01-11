using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BusinessObject;
using SubwayQuery.DataModel;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace SubwayWPF
{
    /// <summary>
    ///     Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Window
    {
        private readonly BizPlanPath bizPlanPath = new BizPlanPath();
        private readonly string googleMapUrl = "http://localhost:4169/GoogleMap.aspx?data=";
        private RoutePlanResult ret;

        public Home()
        {
            InitializeComponent();
            InitStation();
            InitDefaultMpa();
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            var startSta = ((ComboBoxItem)cmbStart.SelectedItem).DataContext.ToString();
            var targetSta = ((ComboBoxItem)cmbTarget.SelectedItem).DataContext.ToString();
            var strOption = ((ComboBoxItem)cmbOption.SelectedItem).DataContext.ToString();
            var tmpRet = "";
            if (bizPlanPath.ValideData(startSta, targetSta, ref tmpRet, "WPF") == false)
            {
                MessageBox.Show(tmpRet);
                return;
            }

            rdoYL.IsChecked = true;
            ret = bizPlanPath.GetPlanPath(startSta, targetSta, strOption, "WPF");
            DrawLine();
            PopupButton3.Content = "预计到达时间 ：" + ret.weight + "分";
        }

        //标记路线
        private void DrawLine()
        {
            if (ret != null && ret.passedNodeIds.Length > 0)
            {
                var bitmap = new Bitmap("OK.png");
                var g = Graphics.FromImage(bitmap);

                var path = ret.passedNodeIds;
                for (var i = 0; i < path.Length; i++)
                {
                    var station = bizPlanPath.GetNode(path[i]);
                    var newRect = new Rectangle(
                        Convert.ToInt32(station.dMapX) - 10, Convert.ToInt32(station.dMapY) - 10, 20, 20);
                    //设置背景色
                    g.FillEllipse(new SolidBrush(Color.Red), newRect);
                }

                var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                var map = new BitmapImage();
                map.BeginInit();
                map.StreamSource = ms;
                map.EndInit();
                myMap.Source = map;
            }
        }

        //显示比例
        private void cmbSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = (ComboBox)sender;
            switch (cb.SelectedIndex)
            {
                case 0:
                    if (myMap == null) break;
                    myMap.Width = 1002;
                    myMap.Height = 650;
                    Canvas.SetLeft(myMap, 0);
                    Canvas.SetTop(myMap, 0);
                    break;
                case 1:
                    myMap.Width = 2510 * 0.25;
                    myMap.Height = 2110 * 0.25;
                    break;
                case 2:
                    myMap.Width = 2510 * 0.5;
                    myMap.Height = 2110 * 0.5;
                    break;
                case 3:
                    myMap.Width = 2510 * 0.75;
                    myMap.Height = 2110 * 0.75;
                    break;
                case 4:
                    myMap.Width = 2510;
                    myMap.Height = 2110;
                    break;
                case 5:
                    myMap.Width = 2510 * 1.25;
                    myMap.Height = 2110 * 1.25;
                    break;
                case 6:
                    myMap.Width = 2510 * 1.5;
                    myMap.Height = 2110 * 1.5;
                    break;
            }
        }

        #region 初始化UI InitStation(),InitDefaultMpa()

        private void InitStation()
        {
            var bizPlanPath = new BizPlanPath();
            var arStations = bizPlanPath.GetStations("WPF");
            for (var i = 0; i < arStations.Count; i++)
            {
                var sta = (Node)arStations[i];
                var cb = new ComboBoxItem();
                cb.Content = sta.PathName + ":" + sta.StaName;
                cb.DataContext = sta.Id;
                cb.Name = "A" + i;
                cmbStart.Items.Add(cb);

                var cb1 = new ComboBoxItem();
                cb1.Content = sta.PathName + ":" + sta.StaName;
                cb1.DataContext = sta.Id;
                cb1.Name = "A" + i;
                cmbTarget.Items.Add(cb1);
            }

            cmbStart.SelectedIndex = 0;
            cmbTarget.SelectedIndex = 0;
        }

        private void InitDefaultMpa()
        {
            var bitmap = new Bitmap("OK.png");
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            var map = new BitmapImage();
            map.BeginInit();
            map.StreamSource = ms;
            map.EndInit();
            myMap.Source = map;
        }

        #endregion

        #region 拖放

        private bool IsMouseDown;
        private Point mousePoint;
        private object mouseCtrl;

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var theMousePoint = e.GetPosition(cavMapBase);

                    var tl = Canvas.GetLeft((UIElement)mouseCtrl) + (theMousePoint.X - mousePoint.X);
                    var tt = Canvas.GetTop((UIElement)mouseCtrl) + (theMousePoint.Y - mousePoint.Y);

                    Canvas.SetLeft((UIElement)mouseCtrl, tl);
                    Canvas.SetTop((UIElement)mouseCtrl, tt);
                    mousePoint = theMousePoint;
                }
        }

        private void myMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsMouseDown = true;
                mousePoint = e.GetPosition(cavMapBase);
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
            grdData.Visibility = Visibility.Collapsed;
            myMap.Visibility = Visibility.Visible;
            grdBingMap.Visibility = Visibility.Collapsed;
        }

        private void rdoXC_Click(object sender, RoutedEventArgs e)
        {
            myMap.Visibility = Visibility.Collapsed;
            grdData.Visibility = Visibility.Visible;
            grdBingMap.Visibility = Visibility.Collapsed;
            DispXCDetail();
        }

        #endregion

        #region 出行路线详细表格

        private void DispXCDetail()
        {
            for (var i = tbData.RowDefinitions.Count - 1; i > 0; i--) tbData.RowDefinitions.RemoveAt(i);

            //
            var strOption = ((ComboBoxItem)cmbOption.SelectedItem).DataContext.ToString();
            if (strOption == "2")
                txtPathDsc.Text = "换乘最少路线：";
            else
                txtPathDsc.Text = "用时最短路线：";

            if (ret != null) txtUseTime.Text = ret.weight.ToString();

            //去掉重复的站点，因为在换乘路线查询算法中，如果有换乘发生，则会产生一些重复的换乘站的
            //为了本算法的统一性，先去掉这些重复站点
            IEnumerable passNodes = ret.passedNodeIds.Distinct();
            var paths = new Queue();
            foreach (string v in passNodes) paths.Enqueue(v);
            Node priv, next;
            RowDefinition rowDef;
            Border border;
            Image img;
            TextBlock txt;
            WrapPanel wp;
            double passTime = 0;
            Node tmpPriv;
            var passStationCount = 0;

            priv = bizPlanPath.GetNode(paths.Dequeue().ToString());
            tmpPriv = priv.Clone();
            var row = 1;

            #region

            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(25);
            tbData.RowDefinitions.Add(rowDef);

            //0 column
            border = GetBorder(row);

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 0);
            tbData.Children.Add(border);

            //1 column
            border = GetBorder(row);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, 1);

            tbData.Children.Add(border);


            //2 column
            border = GetBorder(row);
            img = GetImage("station.png", 14, 15);
            border.Child = img;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 2);

            tbData.Children.Add(border);

            //3 column
            border = GetBorder(row);
            txt = new TextBlock();
            txt.Text = "【" + priv.PathName + "】 " + priv.StaName;
            txt.VerticalAlignment = VerticalAlignment.Center;
            border.Child = txt;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 3);
            tbData.Children.Add(border);

            //4 column
            border = GetBorder(row);
            txt = new TextBlock();
            txt.Text = "2元";
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.HorizontalAlignment = HorizontalAlignment.Center;
            border.Child = txt;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 4);
            tbData.Children.Add(border);

            //5 column
            border = GetBorder(row);
            wp = GetOperateColumn(ret.passedNodeIds, priv.Id);
            border.Child = wp;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 5);
            tbData.Children.Add(border);

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

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(25);
                    tbData.RowDefinitions.Add(rowDef);

                    //0 column
                    border = GetBorder(row);
                    img = GetImage("subway.png", 15, 14);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    tbData.Children.Add(border);

                    //1 column
                    border = GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = passTime + "分";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    border.Child = txt;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 1);

                    tbData.Children.Add(border);


                    //2 column
                    border = GetBorder(row);
                    img = GetImage("A" + tmpPriv.PathId + ".png", 6, 25);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 2);

                    tbData.Children.Add(border);

                    //3 column
                    border = GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = "【" + tmpPriv.PathName + "】 " + tmpPriv.StaName + "（全程）方向（" + passStationCount + "）";
                    ;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = txt;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 3);
                    tbData.Children.Add(border);

                    //4 column
                    border = GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 4);
                    tbData.Children.Add(border);

                    //5 column
                    border = GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 5);
                    tbData.Children.Add(border);

                    #endregion

                    row++;

                    #region 换乘路线

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(25);
                    tbData.RowDefinitions.Add(rowDef);

                    //0 column
                    border = GetBorder(row);
                    img = GetImage("person.png", 15, 14);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    tbData.Children.Add(border);

                    //1 column
                    border = GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = bizPlanPath.GetWeight(tmpPriv, priv) + "分";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    border.Child = txt;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 1);

                    tbData.Children.Add(border);


                    //2 column
                    border = GetBorder(row);
                    img = GetImage("transfer.png", 13, 12);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 2);

                    tbData.Children.Add(border);

                    //3 column
                    border = GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = "【" + next.StaName + "】 （经由" + tmpPriv.PathName + "-" + tmpPriv.StaName + "方向）换乘";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = txt;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 3);
                    tbData.Children.Add(border);

                    //4 column
                    border = GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 4);
                    tbData.Children.Add(border);

                    //5 column
                    border = GetBorder(row);
                    wp = GetOperateColumn(ret.passedNodeIds, next.Id);
                    border.Child = wp;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 5);
                    tbData.Children.Add(border);

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
                    tbData.RowDefinitions.Add(rowDef);

                    //0 column
                    border = GetBorder(row);
                    img = GetImage("subway.png", 15, 14);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    tbData.Children.Add(border);

                    //1 column
                    border = GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = passTime + "分";
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    border.Child = txt;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 1);

                    tbData.Children.Add(border);


                    //2 column
                    border = GetBorder(row);
                    img = GetImage("A" + tmpPriv.PathId + ".png", 6, 25);
                    border.Child = img;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 2);

                    tbData.Children.Add(border);

                    //3 column
                    border = GetBorder(row);
                    txt = new TextBlock();
                    txt.Text = "【" + priv.PathName + "】 " + priv.StaName + "（全程）方向（" + passStationCount + "）";
                    ;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = txt;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 3);
                    tbData.Children.Add(border);

                    //4 column
                    border = GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 4);
                    tbData.Children.Add(border);

                    //5 column
                    border = GetBorder(row);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 5);
                    tbData.Children.Add(border);

                    #endregion
                }
            }

            //输出结束站点
            row++;

            #region

            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(25);
            tbData.RowDefinitions.Add(rowDef);

            //0 column
            border = GetBorder(row);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, 0);
            tbData.Children.Add(border);

            //1 column
            border = GetBorder(row);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, 1);

            tbData.Children.Add(border);


            //2 column
            border = GetBorder(row);
            img = GetImage("station.png", 14, 15);
            border.Child = img;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 2);

            tbData.Children.Add(border);

            //3 column
            border = GetBorder(row);
            txt = new TextBlock();
            txt.Text = "【" + priv.PathName + "】 " + priv.StaName;
            txt.VerticalAlignment = VerticalAlignment.Center;
            border.Child = txt;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 3);
            tbData.Children.Add(border);

            //4 column
            border = GetBorder(row);

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 4);
            tbData.Children.Add(border);

            //5 column
            border = GetBorder(row);
            wp = GetOperateColumn(ret.passedNodeIds, priv.Id);
            border.Child = wp;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 5);
            tbData.Children.Add(border);

            #endregion
        }

        private Border GetBorder(int flag)
        {
            var border = new Border();
            border.Margin = new Thickness(0);
            border.BorderBrush = GetColorFromHex("#FFF4F0F0");
            border.BorderThickness = new Thickness(0.3);

            if (flag % 2 == 0)
                border.Background = GetColorFromHex("#00000000");
            else
                border.Background = GetColorFromHex("#FFF0F0EF");
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

        private Image GetImage(string imgName, int w, int h)
        {
            var img = new Image();
            img.Width = w;
            img.Height = h;
            var bitmapImg = new BitmapImage(new Uri("images/" + imgName, UriKind.Relative));
            img.Source = bitmapImg;

            return img;
        }

        private WrapPanel GetOperateColumn(string[] stationDatas, string currentStation)
        {
            var wp = new WrapPanel();
            wp.Width = 115;
            wp.Height = 16;
            wp.HorizontalAlignment = HorizontalAlignment.Center;

            var txt1 = new TextBlock();
            txt1.Name = "txtBing";
            txt1.Text = "Google地图";
            txt1.TextWrapping = TextWrapping.Wrap;
            txt1.FontSize = 13.3;
            txt1.MouseUp += txtBing_MouseUp;
            txt1.Cursor = Cursors.Hand;
            txt1.DataContext = GetMapData(stationDatas, currentStation);
            wp.Children.Add(txt1);

            var txt2 = new TextBlock();
            txt2.Name = "txtTime";
            txt2.Text = "时刻表";
            txt2.TextWrapping = TextWrapping.Wrap;
            txt2.FontSize = 13.3;
            txt2.Cursor = Cursors.Hand;
            txt2.Margin = new Thickness(15, 0, 0, 0);
            wp.Children.Add(txt2);
            return wp;
        }

        private void txtBing_MouseUp(object sender, MouseButtonEventArgs e)
        {
            myMap.Visibility = Visibility.Collapsed;
            grdData.Visibility = Visibility.Collapsed;
            grdBingMap.Visibility = Visibility.Visible;

            var txt1 = (TextBlock)sender;
            grdBingMap.Source = new Uri(googleMapUrl + txt1.DataContext);
        }

        private string GetMapData(string[] stationDatas, string currentStation)
        {
            var tmp = "";
            foreach (var s in stationDatas)
                if (tmp == "")
                    tmp = s;
                else
                    tmp += "," + s;
            tmp += ";" + currentStation;
            return tmp;
        }

        #endregion

        #region 实现菜单项

        private void myMap_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ConnectPopup.Placement = PlacementMode.Mouse;
            ConnectPopup.StaysOpen = false;
            ConnectPopup.IsOpen = true;
        }

        private void PopupButton1_Click(object sender, RoutedEventArgs e)
        {
            myMap.Visibility = Visibility.Collapsed;
            grdData.Visibility = Visibility.Collapsed;
            grdBingMap.Visibility = Visibility.Visible;
            ConnectPopup.IsOpen = false;
            grdBingMap.Source = new Uri(googleMapUrl);
        }

        private void PopupButton3_Click(object sender, RoutedEventArgs e)
        {
            myMap.Visibility = Visibility.Collapsed;
            grdData.Visibility = Visibility.Visible;
            ConnectPopup.IsOpen = false;
            DispXCDetail();
        }

        #endregion
    }
}