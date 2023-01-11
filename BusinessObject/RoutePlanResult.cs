namespace BusinessObject
{
    public class RoutePlanResult
    {
        public string[] passedNodeIds;

        public double weight;

        public RoutePlanResult(string[] strings, double d)
        {
            passedNodeIds = strings;
            weight = d;
        }

        public RoutePlanResult()
        {
        }

        public void AddPassedNodes(string[] strings, double d)
        {
            if (passedNodeIds != null)
            {
                var ss = new string[passedNodeIds.Length + strings.Length];
                weight += d;
                passedNodeIds.CopyTo(ss, 0);
                strings.CopyTo(ss, passedNodeIds.Length);
                passedNodeIds = ss;
            }
            else
            {
                passedNodeIds = strings;
                weight = d;
            }
        }
    }
}