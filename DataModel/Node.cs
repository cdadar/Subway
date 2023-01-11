using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SubwayQuery.DataModel
{
    public class Node
    {
        public string Id;         //车站ID
        public string StaName = "";//车站名称
        public string PathName = "";//线路名称
        public string PathId = "";  //线路ID
        public string GPS = "";    //卫星定位
        public string IsExchangeSta = "NO";//是否换乘

        /// <summary>
        /// /茶盏坐标
        /// </summary>
        public double dMapX = 0.0;
        public double dMapY = 0.0;
        //Edge集合
        public ArrayList EdgeList;


        #region  构造函数
        public Node(string id, string staName, string pathName, string pathId, string gps, string isExchangeSta, double X, double Y)
        {
            this.Id = id;
            this.StaName = staName;
            this.PathId = pathId;
            this.PathName = pathName;
            this.IsExchangeSta = isExchangeSta;
            this.dMapX = X;
            this.dMapY = Y;
            this.GPS = gps;

            this.EdgeList = new ArrayList();
        }
        public Node()
        { }

        public Node(string id)
        {
            this.Id = id;
        }

        #endregion

        /// <summary>
        /// 获取NextSta指向的边
        /// </summary>
        /// <returns></returns>
        public Edge GetNetEdge()
        {
            for (int i = 0; i < EdgeList.Count; i++)
            {
                Edge e = EdgeList[i] as Edge;

                if (e.EdgeDirection == DataDirection.next)
                    return e;
            }
            return null;
        }

        /// <summary>
        /// 获取PrivSta指向的边
        /// </summary>
        /// <returns></returns>
        public Edge GetPrivEdge()
        {
            for (int i = 0; i < EdgeList.Count; i++)
            {
                Edge e=EdgeList[i] as Edge;
                if (e.EdgeDirection == DataDirection.priv)
                    return e;
            }
            return null;
        }


        public Node Clone()
        {
            Node cloneNode = new Node();
            cloneNode.Id = this.Id;
            cloneNode.PathName = this.PathName;
            cloneNode.PathId = this.PathId;
            cloneNode.IsExchangeSta = this.IsExchangeSta;
            cloneNode.dMapX = this.dMapX;
            cloneNode.dMapY = this.dMapY;
            cloneNode.StaName = this.StaName;
            cloneNode.GPS = this.GPS;

            ArrayList edges = new ArrayList();

            for (int i = 0; i < this.EdgeList.Count; i++)
            {
                Edge curE = this.EdgeList[i] as Edge;
                Edge e = new Edge();

                e.EdgeDirection = curE.EdgeDirection;
                e.EndNodeId = curE.EndNodeId;
                e.IsStep = curE.IsStep;
                e.SatrtNodeId = curE.SatrtNodeId;
                e.Weight = curE.Weight;

                edges.Add(e);
            }
            cloneNode.EdgeList = edges;

            return cloneNode;
        }
    }
}
