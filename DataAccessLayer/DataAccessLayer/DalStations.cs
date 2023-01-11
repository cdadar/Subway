using System.Collections;
using System.Web;
using System.Xml.Linq;
using SubwayQuery.DataModel;

namespace DataAccessLayer
{
    /// <summary>
    ///     数据访问层对象，实现XML路线的读取以及业务模型的初始化
    /// </summary>
    public class DalStations
    {
        /// <summary>
        ///     整个地铁交通网络所有的车站，也就是XML数据文件中所有Station标记的元素
        /// </summary>
        public static ArrayList galPathStations;

        /// <summary>
        ///     所有的换乘车站，也就是XML数据文件中，IsExChangeSta=YE的Station标记的元素
        /// </summary>
        public static ArrayList galPathExStations;

        public void InitStations(params string[] flag)
        {
            //每次启动程序，数据只加载一次，提高执行效率
            if (galPathExStations != null && galPathStations != null) return;

            galPathStations = new ArrayList();
            galPathExStations = new ArrayList();

            var mapFileName = "";
            if (flag.Length > 0 && flag[0] == "WPF")
                mapFileName = "SubwayData.xml";
            else
                mapFileName = HttpContext.Current.Server.MapPath("App_Data\\SubwayData.xml");

            var sData = XElement.Load(mapFileName);

            //初始化所有车站（图顶点）
            var Paths = sData.Elements("path");

            foreach (var path in Paths)
            {
                //访问path属性的属性
                var pathID = path.Attribute("id").Value;
                var pathName = path.Attribute("name").Value;

                var Stations = path.Elements("station");

                //初始化所有车站
                foreach (var station in Stations)
                {
                    var id = station.Attribute("id").Value;
                    var name = station.Attribute("name").Value;
                    var IsExchangeSta = station.Attribute("IsExchangeSta").Value;
                    var X = double.Parse(station.Attribute("X").Value);
                    var Y = double.Parse(station.Attribute("Y").Value);
                    var GPS = station.Attribute("GPS").Value;
                    var staNode = new Node(id, name, pathName, pathID, GPS, IsExchangeSta, X, Y);
                    galPathStations.Add(staNode);
                }
            }

            //初始化线路
            foreach (var path in Paths)
            {
                var Stations = path.Elements("station");

                foreach (var station in Stations)
                {
                    var id = station.Attribute("id").Value;
                    var PrivSta = station.Attribute("PrivSta").Value;
                    var NextSta = station.Attribute("NextSta").Value;
                    var ToPrivTime = station.Attribute("ToPrivTime").Value;
                    var ToNextTime = station.Attribute("ToNextTime").Value;
                    var IsExchangeSta = station.Attribute("IsExchangeSta").Value;

                    var node = GetNode(id);

                    //始发站没有前一站
                    if (PrivSta != "0000")
                    {
                        var aEdge1 = new Edge();

                        aEdge1.SatrtNodeId = id;
                        aEdge1.EndNodeId = PrivSta;
                        aEdge1.Weight = double.Parse(ToPrivTime);
                        aEdge1.EdgeDirection = DataDirection.priv;
                        node.EdgeList.Add(aEdge1);
                    }

                    //终点站没有下一站
                    if (NextSta != "9999")
                    {
                        var aEdge1 = new Edge();
                        aEdge1.SatrtNodeId = id;
                        aEdge1.EndNodeId = NextSta;
                        aEdge1.Weight = double.Parse(ToNextTime);
                        aEdge1.EdgeDirection = DataDirection.next;
                        node.EdgeList.Add(aEdge1);
                    }

                    //换乘
                    if (IsExchangeSta == "YE")
                    {
                        var ExStations = station.Elements("ExStation");

                        foreach (var exStation in ExStations)
                        {
                            var ToPath = exStation.Attribute("TOPath").Value.Split(',');
                            var NeedTime = exStation.Attribute("NeedTime").Value;

                            var aEdge1 = new Edge();
                            aEdge1.SatrtNodeId = id;
                            aEdge1.EndNodeId = ToPath[1];
                            aEdge1.EdgeDirection = DataDirection.trabsfer;
                            aEdge1.Weight = double.Parse(NeedTime);
                            aEdge1.IsStep = true;
                            node.EdgeList.Add(aEdge1);
                        }

                        galPathExStations.Add(node);
                    }
                }
            }
        }

        public Node GetNode(string nodeId, params string[] flag)
        {
            if (galPathExStations == null && galPathStations == null) InitStations(flag);
            for (var i = 0; i < galPathStations.Count; i++)
            {
                var node = (Node)galPathStations[i];

                if (node.Id == nodeId)
                    return node;
            }

            return null;
        }
    }
}