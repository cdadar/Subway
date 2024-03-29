﻿using System.Collections;
using DataAccessLayer;
using SubwayQuery.DataModel;

namespace BusinessObject
{
    /// <summary>
    ///     RoutePlanner提供图算法中常用的最短路径算法功能
    ///     在所有的基础构建好后，路径规划算法就很容易实施了，该算法只要步骤如下：
    ///     1、用一张表(PlanCourse)记录远点到任何其他以及诶单的最小权值，初始化这张表时，如果远点能直通某节点，
    ///     则权值设为对应的边的权，否则设为double.MaxValue。
    ///     2、选取没有被处理并且当前积累权值最小的及诶单TargetNode，用其边的可达性来更新到达其他节点的路径和权值
    ///     (如果其他节点经此节点后权值变小则更新，否则不更新)，然后标记TargetNode为已处理。
    ///     3、重复2，知道所有的可达节点都被处理一遍。
    ///     4、从PlanCourse表中获取目的点的PassedPath，即为结果。
    /// </summary>
    public class ShortestTimePath
    {
        public ArrayList nodeList;

        public ShortestTimePath(params string[] flag)
        {
            var dalStations = new DalStations();
            dalStations.InitStations(flag);
            nodeList = DalStations.galPathStations;
        }

        public RoutePlanResult GetShortestTimePath(string originId, string destId)
        {
            var planCourse = new PlanCourse(nodeList, originId);
            var curNode = GetMinWeightRudeNode(planCourse, originId);

            #region 计算过程

            while (curNode != null)
            {
                var curPath = planCourse[curNode.Id];
                foreach (Edge edge in curNode.EdgeList)
                {
                    //已选取的顶点则不必考虑
                    if (curPath.PassedIdList.Contains(edge.EndNodeId)) continue;

                    var targetPath = planCourse[edge.EndNodeId];
                    var tempWeight = curPath.Weight + edge.Weight;
                    if (tempWeight < targetPath.Weight)
                    {
                        targetPath.Weight = tempWeight;
                        targetPath.PassedIdList.Clear();
                        for (var i = 0; i < curPath.PassedIdList.Count; i++)
                            targetPath.PassedIdList.Add(curPath.PassedIdList[i].ToString());
                        targetPath.PassedIdList.Add(curNode.Id);
                    }
                }

                //标志为已处理
                planCourse[curNode.Id].BeProcessed = true;
                //获取下一个未处理节点
                curNode = GetMinWeightRudeNode(planCourse, originId);
            }

            #endregion

            //表示规划结束
            return GetResult(planCourse, destId);
        }

        /// <summary>
        ///     从PlanCourse表中取出目标及诶单的PassedPath，这个PassedPath即是规划结果
        /// </summary>
        /// <param name="planCourse">迪克斯特拉(Dikastra)算法中，一个顶点到所有其他顶点的最短路径</param>
        /// <param name="destId">目标车站ID</param>
        /// <returns>到达目的站点的路线</returns>
        private RoutePlanResult GetResult(PlanCourse planCourse, string destId)
        {
            var pPath = planCourse[destId];

            if (pPath.Weight == int.MaxValue)
            {
                var result1 = new RoutePlanResult(null, int.MaxValue);
                return result1;
            }

            var passedNodeIDs = new string[pPath.PassedIdList.Count + 1];
            for (var i = 0; i < passedNodeIDs.Length - 1; i++) passedNodeIDs[i] = pPath.PassedIdList[i].ToString();
            passedNodeIDs[passedNodeIDs.Length - 1] = pPath.CurNodeId;

            var result = new RoutePlanResult(passedNodeIDs, pPath.Weight);

            return result;
        }

        /// <summary>
        ///     从PlanCourse取出一个当前累计权值最小，并且没有被处理过的节点
        /// </summary>
        /// <param name="planCourse">迪克斯特拉(Dikastra)算法中，一个顶点到所有其他顶点的最短路径</param>
        /// <param name="originId">北京地铁所有路线的车站</param>
        /// <returns>从当前站点可达通路的权重最小的边的节点</returns>
        private Node GetMinWeightRudeNode(PlanCourse planCourse, string originId)
        {
            var weight = double.MaxValue;
            Node destNode = null;

            foreach (Node node in nodeList)
            {
                if (node.Id == originId)
                    continue;
                var pPath = planCourse[node.Id];
                if (pPath.BeProcessed)
                    continue;
                if (pPath.Weight < weight)
                {
                    weight = pPath.Weight;
                    destNode = node;
                }
            }

            return destNode;
        }
    }
}