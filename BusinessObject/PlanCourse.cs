using System;
using System.Collections;
using SubwayQuery.DataModel;

namespace BusinessObject
{
    //PlanCourse缓存从源节点到其他任意节点的最小权值路径  路径表
    public class PlanCourse
    {
        private readonly Hashtable htPassedPath;

        public PlanCourse(ArrayList nodeList, string originId)
        {
            htPassedPath = new Hashtable();

            Node originNode = null;
            foreach (Node node in nodeList)
                if (node.Id == originId)
                {
                    originNode = node;
                }
                else
                {
                    var pPath = new PassedPath(node.Id);
                    htPassedPath.Add(node.Id, pPath);
                }

            if (originNode == null)
                throw new Exception("起始点不存在");
            InitializeWeight(originNode);
        }

        public PassedPath this[string nodeId] => (PassedPath)htPassedPath[nodeId];

        private void InitializeWeight(Node originNode)
        {
            if (originNode.EdgeList == null || originNode.EdgeList.Count == 0)
                return;
            foreach (Edge edge in originNode.EdgeList)
            {
                var pPath = this[edge.EndNodeId];
                if (pPath == null)
                    continue;
                pPath.PassedIdList.Add(originNode.Id);
                pPath.Weight = edge.Weight;
            }
        }
    }
}