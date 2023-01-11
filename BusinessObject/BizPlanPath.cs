using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using SubwayQuery.DataModel;
using DataAccessLayer;

namespace BusinessObject
{
    public class BizPlanPath
    {
        /// <summary>
        /// 查询地铁换横路线的统一接口
        /// </summary>
        /// <param name="originId">始发站</param>
        /// <param name="destId">目标站</param>
        /// <param name="strOption">1：用时最少路线查询；2：换乘最少路线查询</param>
        /// <returns>最优的换乘路线</returns>
        public RoutePlanResult GetPlanPath(string originId, string destId, string strOption,params string[] flag)
        {
            ShortestTimePath p = new ShortestTimePath(flag);
            TransferLestPath t = new TransferLestPath(flag);
            RoutePlanResult ret = null;
            //换乘最少路线查询
            if (strOption == "2")
            {
                ret = t.GetTransferLeastPath(originId, destId);
            }
            else
                ret = p.GetShortestTimePath(originId, destId);
            return ret;
        }
        /// <summary>
        /// 从车站列表中获得一个车站
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public Node GetNode(string nodeId)
        {
            DalStations dalStations = new DalStations();
            return dalStations.GetNode(nodeId);
        }

        /// <summary>
        /// 得到整个地铁网络的所有站点
        /// </summary>
        /// <returns></returns>
        public ArrayList GetStations(params string[] flag)
        {
            DalStations ds = new DalStations();
            ds.InitStations(flag);
            return DalStations.galPathStations;
        }

        /// <summary>
        /// 检查请求的路线查询是否合法
        /// </summary>
        /// <param name="originId"></param>
        /// <param name="destId"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool ValideData(string originId, string destId, ref string ret,params string[] flag)
        {
            if (destId == originId)
            {
                ret = "始发站和目的站不能相同！";
                return false;
            }
            //判断是否是换乘站点，如果是直接提示步行换乘
            DalStations dals = new DalStations();
            dals.InitStations(flag);
            for (int i = 0; i < DalStations.galPathExStations.Count; i++)
            {
                Node station = DalStations.galPathExStations[i] as Node;
                for (int j = 0; j < station.EdgeList.Count; j++)
                {
                    Edge e = station.EdgeList[j] as Edge;
                    if (e.IsStep)
                    {
                        if ((e.SatrtNodeId == originId && e.EndNodeId == destId)
                            || (e.SatrtNodeId == destId && e.EndNodeId == originId))
                        {
                            ret = "您想要查询的车站时换乘路线！您不需要乘车！";
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 获取同一路线上两个车站之间的运行时间
        /// </summary>
        /// <param name="sta1">相邻车站起始站</param>
        /// <param name="sta2">相邻车站终点站</param>
        /// <returns>这个边的权重</returns>
        public double GetWeight(Node sta1, Node sta2)
        {
            ArrayList edges = sta1.EdgeList;

            for (int i = 0; i < edges.Count; i++)
            {
                Edge e = edges[i] as Edge;

                if (e.SatrtNodeId == sta1.Id && e.EndNodeId == sta2.Id)
                    return e.Weight;
            }
            return double.MaxValue;
        }
    }
}
