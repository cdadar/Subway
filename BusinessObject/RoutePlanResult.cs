using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObject
{
    public class RoutePlanResult
    {
        public string[] passedNodeIDs;

        public double weight;

        public RoutePlanResult(string[] strings, double d)
        {
            this.passedNodeIDs = strings;
            this.weight = d;
        }
        public RoutePlanResult()
        { }

        public void AddPassedNodes(string[] strings, double d)
        {
            if (passedNodeIDs != null)
            {
                string[] ss = new string[passedNodeIDs.Length + strings.Length];
                this.weight += d;
                passedNodeIDs.CopyTo(ss, 0);
                strings.CopyTo(ss, passedNodeIDs.Length);
                passedNodeIDs = ss;
            }
            else
            {
                this.passedNodeIDs = strings;
                this.weight = d;
            }
        }
    }
}
