using System.Collections.Generic;

namespace BusinessObject
{
    public class TransferData
    {
        //换乘边的换乘目的站ID
        public string TransferDestId = "";

        //始发到目的的权重
        public double TransferEdgeWeight = 0;

        //换乘边的换乘出发站ID
        public string TransferOriginId = "";

        //两个车站之间的某条线路
        public RoutePlanResult TransferPath;

        //经过这两个站点的所有路线
        public List<RoutePlanResult>[] TransferPaths;
    }
}