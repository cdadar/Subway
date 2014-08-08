using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubwayQuery.DataModel
{
    public class Edge
    {
        public string SatrtNodeID;//开始站点
        public string EndNodeID;//目的站
        public bool IsStep = false;//是否换乘
        public double Weight;//到下一站或上一站所需的时间作为权值
        public DataDirection EdgeDirection = DataDirection.none;//方向
    }
    public enum DataDirection
    {
        next,
        priv,
        trabsfer,
        none
    }
}
