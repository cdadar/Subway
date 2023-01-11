using System.Collections;

namespace SubwayQuery.DataModel
{
    public class Node
    {
        /// <summary>
        ///     /茶盏坐标
        /// </summary>
        public double dMapX;

        public double dMapY;

        //Edge集合
        public ArrayList EdgeList;
        public string GPS = ""; //卫星定位
        public string Id; //车站ID
        public string IsExchangeSta = "NO"; //是否换乘
        public string PathId = ""; //线路ID
        public string PathName = ""; //线路名称
        public string StaName = ""; //车站名称

        /// <summary>
        ///     获取NextSta指向的边
        /// </summary>
        /// <returns></returns>
        public Edge GetNetEdge()
        {
            for (var i = 0; i < EdgeList.Count; i++)
            {
                var e = EdgeList[i] as Edge;

                if (e.EdgeDirection == DataDirection.next)
                    return e;
            }

            return null;
        }

        /// <summary>
        ///     获取PrivSta指向的边
        /// </summary>
        /// <returns></returns>
        public Edge GetPrivEdge()
        {
            for (var i = 0; i < EdgeList.Count; i++)
            {
                var e = EdgeList[i] as Edge;
                if (e.EdgeDirection == DataDirection.priv)
                    return e;
            }

            return null;
        }


        public Node Clone()
        {
            var cloneNode = new Node();
            cloneNode.Id = Id;
            cloneNode.PathName = PathName;
            cloneNode.PathId = PathId;
            cloneNode.IsExchangeSta = IsExchangeSta;
            cloneNode.dMapX = dMapX;
            cloneNode.dMapY = dMapY;
            cloneNode.StaName = StaName;
            cloneNode.GPS = GPS;

            var edges = new ArrayList();

            for (var i = 0; i < EdgeList.Count; i++)
            {
                var curE = EdgeList[i] as Edge;
                var e = new Edge();

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


        #region 构造函数

        public Node(string id, string staName, string pathName, string pathId, string gps, string isExchangeSta,
            double X, double Y)
        {
            Id = id;
            StaName = staName;
            PathId = pathId;
            PathName = pathName;
            IsExchangeSta = isExchangeSta;
            dMapX = X;
            dMapY = Y;
            GPS = gps;

            EdgeList = new ArrayList();
        }

        public Node()
        {
        }

        public Node(string id)
        {
            Id = id;
        }

        #endregion
    }
}