using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace BusinessObject
{
    /// <summary>
    /// 用于缓存计算机过程中到达某个节点的权值最小的路径
    /// </summary>
    public class PassedPath
    {
        
        // 是否已被处理
        public bool BeProcessed;

        // 当前正在处理的节点（迪克斯特拉(Dikastra)算法）
        public string CurNodeID;

        //累积的权值
        public double Weight;

        //路径
        public ArrayList PassedIDList;

        public PassedPath(string ID)
        {
            this.CurNodeID = ID;
            this.Weight = double.MaxValue;
            this.PassedIDList = new ArrayList();
            this.BeProcessed = false;
        }

        public PassedPath()
        {
            this.PassedIDList = new ArrayList();
        }
    }
}
