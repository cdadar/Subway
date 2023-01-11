using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using System.Xml.Linq;

using SubwayQuery.DataModel;



namespace DataAccessLayer
{
    /// <summary>
    /// 数据访问层对象，实现XML路线的读取以及业务模型的初始化
    /// </summary>
    public class DalStations
    {
        /// <summary>
        /// 整个地铁交通网络所有的车站，也就是XML数据文件中所有Station标记的元素
        /// </summary>
        public static ArrayList galPathStations = null;

        /// <summary>
        /// 所有的换乘车站，也就是XML数据文件中，IsExChangeSta=YE的Station标记的元素
        /// </summary>
        public static ArrayList galPathExStations =  null;

        public void InitStations(params string[] flag)
        {
            //每次启动程序，数据只加载一次，提高执行效率
            if (galPathExStations != null && galPathStations != null) return;

            galPathStations = new ArrayList();
            galPathExStations = new ArrayList();

            string mapFileName = "";
            if (flag.Length > 0 && flag[0] == "WPF")
            {
                mapFileName = "SubwayData.xml";
            }
            else
                mapFileName = System.Web.HttpContext.Current.Server.MapPath("App_Data\\SubwayData.xml");

            XElement sData = XElement.Load(mapFileName);

            //初始化所有车站（图顶点）
            IEnumerable<XElement> Paths = sData.Elements("path");

            foreach (XElement path in Paths)
            {
                //访问path属性的属性
                string pathID = path.Attribute("id").Value;
                string pathName = path.Attribute("name").Value;

                IEnumerable<XElement> Stations = path.Elements("station");

                //初始化所有车站
                foreach (XElement station in Stations)
                {
                    string id = station.Attribute("id").Value;
                    string name = station.Attribute("name").Value;
                    string IsExchangeSta = station.Attribute("IsExchangeSta").Value;
                    double X = double.Parse(station.Attribute("X").Value);
                    double Y = double.Parse(station.Attribute("Y").Value);
                    string GPS = station.Attribute("GPS").Value;
                    Node staNode = new Node(id, name, pathName, pathID, GPS, IsExchangeSta, X, Y);
                    galPathStations.Add(staNode);
                }
            }

            //初始化线路
            foreach (XElement path in Paths)
            {
                IEnumerable<XElement> Stations = path.Elements("station");

                foreach (XElement station in Stations)
                {
                    string id = station.Attribute("id").Value;
                    string PrivSta = station.Attribute("PrivSta").Value;
                    string NextSta = station.Attribute("NextSta").Value;
                    string ToPrivTime = station.Attribute("ToPrivTime").Value;
                    string ToNextTime = station.Attribute("ToNextTime").Value;
                    string IsExchangeSta = station.Attribute("IsExchangeSta").Value;

                    Node node = this.GetNode(id);
                  
                        //始发站没有前一站
                        if (PrivSta != "0000")
                        {
                            Edge aEdge1 = new Edge();

                            aEdge1.SatrtNodeId = id;
                            aEdge1.EndNodeId = PrivSta;
                            aEdge1.Weight = double.Parse(ToPrivTime);
                            aEdge1.EdgeDirection = DataDirection.priv;
                            node.EdgeList.Add(aEdge1);
                        }
                        //终点站没有下一站
                        if (NextSta != "9999")
                        {
                            Edge aEdge1 = new Edge();
                            aEdge1.SatrtNodeId = id;
                            aEdge1.EndNodeId = NextSta;
                            aEdge1.Weight = double.Parse(ToNextTime);
                            aEdge1.EdgeDirection = DataDirection.next;
                            node.EdgeList.Add(aEdge1);
                        }
                        //换乘
                        if (IsExchangeSta == "YE")
                        {
                            IEnumerable<XElement> ExStations = station.Elements("ExStation");

                            foreach (XElement exStation in ExStations)
                            {
                                string[] ToPath = exStation.Attribute("TOPath").Value.Split(',');
                                string NeedTime = exStation.Attribute("NeedTime").Value;

                                Edge aEdge1 = new Edge();
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

        public Node GetNode(string nodeId,params string[] flag)
        {
            if (galPathExStations == null && galPathStations == null) this.InitStations(flag);
            for (int i = 0; i < galPathStations.Count; i++)
            {
                Node node = (Node)galPathStations[i];

                if (node.Id == nodeId)
                    return node;
            }

            return null;
        }
    }
}
