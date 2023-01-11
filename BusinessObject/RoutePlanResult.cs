using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObject
{
    public class RoutePlanResult
    {
        public string[] passedNodeIds;

        public double weight;

        public RoutePlanResult(string[] strings, double d)
        {
            this.passedNodeIds = strings;
            this.weight = d;
        }
        public RoutePlanResult()
        { }

        public void AddPassedNodes(string[] strings, double d)
        {
            if (passedNodeIds != null)
            {
                string[] ss = new string[passedNodeIds.Length + strings.Length];
                this.weight += d;
                passedNodeIds.CopyTo(ss, 0);
                strings.CopyTo(ss, passedNodeIds.Length);
                passedNodeIds = ss;
            }
            else
            {
                this.passedNodeIds = strings;
                this.weight = d;
            }
        }
    }
}
