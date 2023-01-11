namespace SubwayQuery.DataModel
{
    public class Edge
    {
        public DataDirection EdgeDirection = DataDirection.none; //方向
        public string EndNodeId; //目的站
        public bool IsStep = false; //是否换乘
        public string SatrtNodeId; //开始站点
        public double Weight; //到下一站或上一站所需的时间作为权值
    }

    public enum DataDirection
    {
        next,
        priv,
        trabsfer,
        none
    }
}