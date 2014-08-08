using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObject
{
    public class TransferData
    {
        //换乘边的换乘出发站ID
        public string TransferOriginID = "";
        //换乘边的换乘目的站ID
        public string TransferDestID = "";
        //始发到目的的权重
        public double TransferEdgeWeight = 0;
        //经过这两个站点的所有路线
        public List<RoutePlanResult>[] TransferPaths;
        //两个车站之间的某条线路
        public RoutePlanResult TransferPath;
    }
}
