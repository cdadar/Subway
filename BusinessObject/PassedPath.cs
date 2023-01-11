using System.Collections;

namespace BusinessObject
{
    /// <summary>
    ///     用于缓存计算机过程中到达某个节点的权值最小的路径
    /// </summary>
    public class PassedPath
    {
        // 是否已被处理
        public bool BeProcessed;

        // 当前正在处理的节点（迪克斯特拉(Dikastra)算法）
        public string CurNodeId;

        //路径
        public ArrayList PassedIdList;

        //累积的权值
        public double Weight;

        public PassedPath(string Id)
        {
            CurNodeId = Id;
            Weight = double.MaxValue;
            PassedIdList = new ArrayList();
            BeProcessed = false;
        }

        public PassedPath()
        {
            PassedIdList = new ArrayList();
        }
    }
}