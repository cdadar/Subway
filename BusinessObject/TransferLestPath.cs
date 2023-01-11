using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataAccessLayer;
using SubwayQuery.DataModel;

namespace BusinessObject
{
    public class TransferLestPath
    {
        private readonly DalStations dalStations = new DalStations();

        public TransferLestPath(params string[] flag)
        {
            dalStations.InitStations(flag);
        }

        /// <summary>
        ///     换乘最少路线查询
        ///     设任意出发站和目的站点，当可以直接到达时则直接到达，不能直接到达则搜索经过一次换乘能到达的线路
        ///     当换乘一次不能到达，则考虑换乘2次。一次类推，以换乘次数为步长进行搜索，一直进行下去
        /// </summary>
        /// <param name="originID">起始点ID</param>
        /// <param name="destID">目的站ID</param>
        /// <returns>到达目的站的最少换乘路线</returns>
        public RoutePlanResult GetTransferLeastPath(string originID, string destID)
        {
            //如果可以直接到达
            var pathS = GetDirectPath2(originID, destID);
            if (pathS != null) return GetShortestTimepath(pathS);

            //如果一次换乘可以到达
            var rs1 = GetOneTimeTransferPath(originID, destID);
            if (rs1 != null) return rs1;

            //如果2次换乘可以到达
            var rs2 = GetTwoTimeTransferPath(originID, destID);
            if (rs2 != null) return rs2;

            //如果n次换乘可以到达 n>3 && n<6
            for (var i = 3; i < 6; i++)
            {
                var rs3 = GetNTimeTransferPathQ(originID, destID, i);
                if (rs3.weight < 1000) return rs3;
            }

            return null;
        }

        #region 能够直接到达的算法

        private List<RoutePlanResult> GetDirectPath2(string originID, string destID)
        {
            var planResults = new List<RoutePlanResult>();

            var targetSta = dalStations.GetNode(destID);
            var stations = DalStations.galPathStations;
            var StaOnLine = new ArrayList(); //存放起始站路线上的车站

            foreach (Node node in stations)
                if (node.Id.Substring(0, 2) == originID.Trim().Substring(0, 2))
                    StaOnLine.Add(node);

            foreach (Node node in StaOnLine)
                if (node.StaName == targetSta.StaName)
                    destID = node.Id;

            if (originID.Trim().Substring(0, 2) != destID.Trim().Substring(0, 2))
            {
                return null;
            }

            var station = dalStations.GetNode(originID);
            var pass1 = new PassedPath();
            pass1.PassedIdList.Add(originID);

            //标记是否能到达目的站点，而不是到达线路终点
            //因为到达路线终点后，也计算为一条路线，但是此线路没有到达目标站点）
            var IsArrive = false;

            #region 向前查找

            var nextEdge = station.GetNetEdge();
            while (nextEdge != null && nextEdge.EndNodeId != "9999" && !IsArrive)
            {
                pass1.PassedIdList.Add(nextEdge.EndNodeId);
                pass1.Weight += nextEdge.Weight;

                if (nextEdge.EndNodeId != destID)
                {
                    station = dalStations.GetNode(nextEdge.EndNodeId);
                    nextEdge = station.GetNetEdge();
                }
                else
                {
                    IsArrive = true;
                }
            }

            if (IsArrive)
            {
                var planResult =
                    new RoutePlanResult(pass1.PassedIdList.ToArray(Type.GetType("System.String")) as string[],
                        pass1.Weight);
                planResults.Add(planResult);
            }

            #endregion

            #region 向后查找

            station = dalStations.GetNode(originID);
            var pass2 = new PassedPath();
            pass2.PassedIdList.Add(originID);
            IsArrive = false;
            var privEdge = station.GetPrivEdge();
            while (privEdge != null && privEdge.EndNodeId != "0000" && !IsArrive)
            {
                pass2.PassedIdList.Add(privEdge.EndNodeId);
                pass2.Weight += privEdge.Weight;

                if (privEdge.EndNodeId != destID)
                {
                    station = dalStations.GetNode(privEdge.EndNodeId);
                    privEdge = station.GetPrivEdge();
                }
                else
                {
                    IsArrive = true;
                }
            }

            if (IsArrive)
            {
                var planResult =
                    new RoutePlanResult(pass2.PassedIdList.ToArray(Type.GetType("System.String")) as string[],
                        pass2.Weight);
                planResults.Add(planResult);
            }

            #endregion

            return planResults;
        }

        #endregion

        #region 换乘一次的算法

        /// <summary>
        ///     获得经过一次换乘可到达的路线
        /// </summary>
        /// <param name="originID">始发站</param>
        /// <param name="destID">目的站</param>
        /// <returns>返回经过一次换乘的路线，如果一次换乘无法到达，则返回null</returns>
        private RoutePlanResult GetOneTimeTransferPath(string originID, string destID)
        {
            //假如经过一次换乘后可以到达目的站，下面的两个变量分别保存已换乘站位分割点的两侧所有直达线路
            var planResults1 = new List<RoutePlanResult>();
            var planResults2 = new List<RoutePlanResult>();

            //在始发站和目的站之间可能存在多种换乘一次的方案，就是说有多个换乘站可选
            //也就是说，下面变量的每个元素即时经过每一个换乘站的所有可选路线。
            var transferDatas = new List<TransferData>();

            //只有换乘站才可以换乘，所以只循环换乘站即可。看经过每个换乘站换乘后事后可以到达
            for (var i = 0; i < DalStations.galPathExStations.Count; i++)
            {
                var station = DalStations.galPathExStations[i] as Node;
                //每个换乘站可能换到多个不同的路线上，对每个换乘边进行循环
                var edges = station.EdgeList;
                for (var c = 0; c < edges.Count; c++)
                {
                    var ce = edges[c] as Edge;
                    if (ce.IsStep)
                    {
                        //以换乘站位分割点，分别求最短路径；如果此换乘站可以到达的书啊，则肯定存在从始发站到此换乘站的直达路线
                        //和从换乘站到目标站的直达路线
                        planResults1 = GetDirectPath2(originID, ce.SatrtNodeId);
                        planResults2 = GetDirectPath2(ce.EndNodeId, destID);

                        //如果此分割点可以分别到达起始点和终点站，则此站点可以最为换乘一次的中间站点
                        //经过此站点的路线也为可选路线
                        if (planResults1 != null && planResults1.Count > 0 && planResults2 != null &&
                            planResults2.Count > 0)
                        {
                            var transferData = new TransferData();
                            //此结构保存此分割点两边的路线（每边可能是多条路线）
                            transferData.TransferPaths = new List<RoutePlanResult>[2];
                            transferData.TransferPaths[0] = planResults1;
                            transferData.TransferPaths[1] = planResults2;

                            //换乘路线（是哪条换乘边）及换乘步行时间
                            transferData.TransferOriginId = ce.SatrtNodeId; //换乘边的换乘出发站ID
                            transferData.TransferDestId = ce.EndNodeId; //换乘边的换乘目的站ID
                            transferData.TransferEdgeWeight = ce.Weight;
                            transferDatas.Add(transferData);
                        }
                    }
                }
            }

            if (transferDatas.Count > 0)
            {
                //此刻完整路线以换乘站分成了两段，所以分别求的这两段中的最短路径，然后合成一条完整路线
                //合并路线时，要注意换乘站及换乘时间
                RoutePlanResult split1 = null;
                RoutePlanResult split2 = null;
                var TransferOriginID = "";
                var TransferDestID = "";
                double TransferEdgeWeight = 0;

                for (var i = 0; i < transferDatas.Count; i++)
                {
                    var tmpTp1 = GetShortestTimepath(transferDatas[i].TransferPaths[0]);
                    var tmpTp2 = GetShortestTimepath(transferDatas[i].TransferPaths[1]);

                    if (split1 == null)
                    {
                        split1 = tmpTp1;
                        split2 = tmpTp2;

                        //换乘路线及换乘步行时间
                        TransferOriginID = transferDatas[i].TransferOriginId;
                        TransferDestID = transferDatas[i].TransferDestId;
                        TransferEdgeWeight = transferDatas[i].TransferEdgeWeight;
                    }
                    else
                    {
                        if (split1.weight + split2.weight + TransferEdgeWeight >
                            tmpTp1.weight + tmpTp2.weight + transferDatas[i].TransferEdgeWeight)
                        {
                            split1 = tmpTp1;
                            split2 = tmpTp2;
                            //换乘路线及换乘时间
                            TransferOriginID = transferDatas[i].TransferOriginId;
                            TransferDestID = transferDatas[i].TransferDestId;
                            TransferEdgeWeight = transferDatas[i].TransferEdgeWeight;
                        }
                    }
                }

                //合并路线；要注意换乘站以及换乘时间
                var lastRs = new RoutePlanResult();
                lastRs.AddPassedNodes(split1.passedNodeIds, split1.weight);
                lastRs.AddPassedNodes(new string[2] { TransferOriginID, TransferDestID }, TransferEdgeWeight);
                lastRs.AddPassedNodes(split2.passedNodeIds, split2.weight);
                return lastRs;
            }

            return null;
        }

        #endregion

        #region 换乘2次的算法

        /// <summary>
        ///     获得经过2次换乘才能到达的路线
        /// </summary>
        /// <param name="originID">始发站</param>
        /// <param name="destID">目的站</param>
        /// <returns>经过2次换乘的路线，若2次换乘不能到达，则返回null</returns>
        private RoutePlanResult GetTwoTimeTransferPath(string originID, string destID)
        {
            //保存中间结果的结构
            var ResultLines = new Dictionary<string, ArrayList>();
            var OriginLineTransferStations = GetTransferStationsByLine(originID.Trim().Substring(0, 2));
            var DestLineTransferStations = GetTransferStationsByLine(destID.Trim().Substring(0, 2));

            //得到所有换乘2次可到达的路线
            foreach (var OriginLineTransferStation in OriginLineTransferStations)
                //每一个换乘站点的每一个可换路线
                for (var i = 0; i < OriginLineTransferStation.EdgeList.Count; i++)
                {
                    var originEdge = OriginLineTransferStation.EdgeList[i] as Edge;
                    //是换乘边
                    if (originEdge.IsStep)
                        foreach (var DestLinTransferStation in DestLineTransferStations)
                            for (var j = 0; j < DestLinTransferStation.EdgeList.Count; j++)
                            {
                                var destEdge = DestLinTransferStation.EdgeList[j] as Edge;
                                if (destEdge.IsStep)
                                {
                                    //两个换乘站的直接路线
                                    var directLns = GetDirectPath2(originEdge.EndNodeId, destEdge.EndNodeId);
                                    //是可选路线
                                    if (directLns != null)
                                    {
                                        var tmpResult = new ArrayList();
                                        //始发站和换乘站的路线
                                        var r1 = GetDirectPath2(originID, originEdge.SatrtNodeId);
                                        //增加始发站到一个换乘站的路线
                                        tmpResult.Add(r1); //0
                                        //增加换乘站
                                        tmpResult.Add(originEdge); //1
                                        tmpResult.Add(directLns); //2
                                        //构造新的边，由于destEdge方向不对
                                        var e = new Edge();
                                        e.SatrtNodeId = destEdge.EndNodeId;
                                        e.EndNodeId = destEdge.SatrtNodeId;
                                        e.Weight = destEdge.Weight;
                                        e.IsStep = destEdge.IsStep;
                                        tmpResult.Add(e); //3

                                        var r2 = GetDirectPath2(destEdge.SatrtNodeId, destID);
                                        tmpResult.Add(r2); //4
                                        ResultLines.Add(ResultLines.Count.ToString(), tmpResult);
                                    }
                                }
                            }
                }

            //计算所有可到达路线中用时最少的路线
            RoutePlanResult lastResult = null;
            foreach (var lines in ResultLines)
            {
                var tmpResult = new RoutePlanResult();

                var lineArrys = lines.Value;
                //始发站到第一个换乘站的最短路线
                var r1 = lineArrys[0] as List<RoutePlanResult>;
                var directR1 = GetShortestTimepath(r1);
                tmpResult.AddPassedNodes(directR1.passedNodeIds, directR1.weight);
                //第一个换乘站
                var oE = lineArrys[1] as Edge;
                tmpResult.AddPassedNodes(new string[2] { oE.SatrtNodeId, oE.EndNodeId }, oE.Weight);
                //换乘站点之间的直接路径
                var r2 = lineArrys[2] as List<RoutePlanResult>;
                var directR2 = GetShortestTimepath(r2);
                tmpResult.AddPassedNodes(directR2.passedNodeIds, directR2.weight);
                //第二个换乘站
                var tE = lineArrys[3] as Edge;
                tmpResult.AddPassedNodes(new string[2] { tE.SatrtNodeId, tE.EndNodeId }, tE.Weight);
                //第二个换乘站和目的站的直接路径
                var r3 = lineArrys[4] as List<RoutePlanResult>;
                var directR3 = GetShortestTimepath(r3);
                tmpResult.AddPassedNodes(directR3.passedNodeIds, directR3.weight);

                if (lastResult == null)
                {
                    lastResult = tmpResult;
                }
                else
                {
                    if (lastResult.weight > tmpResult.weight) lastResult = tmpResult;
                }
            }

            return lastResult;
        }

        #endregion

        #region 换乘N次的算法

        /// <summary>
        ///     查找从始发站到目的站换乘制定次数的出行路线
        /// </summary>
        /// <param name="origionID">始发站ID</param>
        /// <param name="destID">目的站ID</param>
        /// <param name="times">换乘次数</param>
        /// <returns>换乘times次的最优路线</returns>
        private RoutePlanResult GetNTimeTransferPathQ(string originID, string destID, int times)
        {
            //如果换乘次数操作了所有的换乘站，直接返回null
            if (times > DalStations.galPathExStations.Count) return null;
            var targetNode = dalStations.GetNode(destID);

            //从所有换乘站中任意去times个换乘站的拍了；
            //这样就排列出了换乘times次所有可能的换乘方案
            var allTransferLines = GetTransferStationArrange(times);

            //存储路线的中间结构
            var ReslutPaths = new List<ArrayList>();

            //对于经过排列的所有换乘方案进行判断，即从始发站到P1，P1到P2，Pn-1到Pn，Pn到目的站是否能直达
            //其中P1，P2.......是排列后的换乘站
            foreach (var transferNodes in allTransferLines)
            {
                //由于我们已经采用排列组合的方法，对所有可能换乘times次的换乘站点进行了排列
                //所以这里我们可以把从originID到的destID的路线分成times+1段路线
                //然后分别计算出每段路线中用时最小的路线后，在组合成一条完整的路线

                #region

                var transferPath = transferNodes.Value;
                var ResultPath = new ArrayList();

                //第一步，计算开始节点和第一个换乘节点的路线
                var begin = GetDirectPath2(originID, transferPath.Peek().Id);
                if (begin == null) continue;
                var beginMinPath = GetShortestTimepath(begin);

                ResultPath.Add(beginMinPath);

                //第二步，计算各换乘站之间的路线
                Node priv = null;
                while (transferPath.Count > 0)
                {
                    priv = transferPath.Dequeue();
                    if (transferPath.Count > 0)
                    {
                        var next = transferPath.Peek();

                        //计算在priv所有可能换乘路线和next的最短路线
                        GetTransferStationToStationPath(ResultPath, priv, next);
                    }
                }

                //第三步，计算最后一个换乘站和目的站的最短路径
                GetTransferStationToStationPath(ResultPath, priv, targetNode);

                #endregion

                ReslutPaths.Add(ResultPath);
            }

            var finnalPath = new List<RoutePlanResult>();
            //查询一个经过times换乘的最短路径
            foreach (var onePath in ReslutPaths)
            {
                //按照times分段后，应该是times+1个段，每个段的路线分别是onePath中的一项
                //如果onePath不等于times+1，则说明有的路线是不能到达的，即有的换乘方案走不通

                #region

                if (onePath.Count == times + 1)
                {
                    var rpr = new RoutePlanResult();

                    for (var i = 0; i < onePath.Count; i++)
                        //第一个对象时RoutePlanResult类型
                        if (i == 0)
                        {
                            var tm = onePath[i] as RoutePlanResult;
                            rpr.AddPassedNodes(tm.passedNodeIds, tm.weight);
                        }
                        else
                        {
                            var trd = onePath[i] as TransferData;
                            rpr.AddPassedNodes(new string[2] { trd.TransferOriginId, trd.TransferDestId },
                                trd.TransferEdgeWeight);
                            rpr.AddPassedNodes(trd.TransferPath.passedNodeIds, trd.TransferPath.weight);
                        }

                    finnalPath.Add(rpr);
                }

                #endregion
            }

            RoutePlanResult finRet;
            if (finnalPath.Count == 0)
            {
                finRet = new RoutePlanResult();
                finRet.weight = double.MaxValue;
            }
            else
            {
                var minWeight = finnalPath.Min(p => p.weight);
                finRet = finnalPath.Where(p => p.weight == minWeight).First();
            }

            return finRet;
        }

        /// <summary>
        ///     获取从一个可换乘站点到下一个站点的最短路线
        /// </summary>
        /// <param name="ResultPath">最短路线</param>
        /// <param name="transferStation">可换乘站点</param>
        /// <param name="targetNode">目标站点</param>
        private void GetTransferStationToStationPath(ArrayList ResultPath, Node transferStation, Node targetNode)
        {
            var edges = transferStation.EdgeList;
            var trsferPath = new List<TransferData>();

            //对于transferStation可能存在多个换乘边，分别对每个换乘边计算到targetNode的最短路径
            for (var i = 0; i < edges.Count; i++)
            {
                var e = edges[i] as Edge;

                if (e.IsStep)
                {
                    var tmp1 = GetDirectPath2(e.EndNodeId, targetNode.Id);

                    if (tmp1 != null && tmp1.Count > 0)
                    {
                        var minTmp = GetShortestTimepath(tmp1);

                        var td = new TransferData();
                        td.TransferOriginId = e.SatrtNodeId;
                        td.TransferDestId = e.EndNodeId;
                        td.TransferEdgeWeight = e.Weight;
                        td.TransferPath = minTmp;

                        trsferPath.Add(td);
                    }
                }
            }

            //对于transferStation可能存在多个换乘边，计算用时最少路径
            TransferData tmpTd = null;
            if (trsferPath.Count > 0)
            {
                foreach (var td in trsferPath)
                    if (tmpTd == null)
                    {
                        tmpTd = td;
                    }
                    else
                    {
                        if (tmpTd.TransferEdgeWeight + tmpTd.TransferPath.weight
                            > td.TransferEdgeWeight + td.TransferPath.weight)
                            tmpTd = td;
                    }

                ResultPath.Add(tmpTd);
            }
        }

        /// <summary>
        ///     获得从所有换乘站中选择times个站点进行全排列的序列
        ///     假设换乘站供应N个，此结果返回P（N，times）个序列
        ///     考虑到数据量较少，我们可以忽略算法的性能
        /// </summary>
        /// <param name="times">换乘次数</param>
        /// <returns>所有可换乘的序列</returns>
        private Dictionary<int, Queue<Node>> GetTransferStationArrange(int times)
        {
            var finnalResult = new Dictionary<int, Queue<Node>>();

            //由于公共排列组合类是基于有序数字的，所有我们这里必须以集合的有序索引来代替，
            //这样我们才可以对排列结果和站进行一一对应
            var iTransferCount = DalStations.galPathExStations.Count;

            //对所有可换乘站点取times个进行组合
            var cRet = Combinatorics.Combination(times, iTransferCount);
            //以表格行的格式生成所有的组合
            var dtRet = Combinatorics.TwoDemisionIntArrayToDataTable(cRet);

            //对每一组的换乘站点，分别生成全排列的路线
            for (var i = 0; i < dtRet.Rows.Count; i++)
            {
                var tmpNode = new List<Node>();

                for (var j = 0; j < dtRet.Columns.Count; j++)
                {
                    int posi = Convert.ToInt16(dtRet.Rows[i][j]);
                    tmpNode.Add(DalStations.galPathExStations[posi - 1] as Node);
                }

                //对tmpNode换乘站生成全排列换乘路线
                var tmpNodeArrange = Combinatorics.Arrange(times, times);
                var arrangeLines = Combinatorics.TwoDemisionIntArrayToDataTable(tmpNodeArrange);

                for (var k = 0; k < arrangeLines.Rows.Count; k++)
                {
                    var tmpLine = new Queue<Node>();

                    for (var j = 0; j < arrangeLines.Columns.Count; j++)
                    {
                        int posi = Convert.ToInt16(arrangeLines.Rows[k][j]);
                        tmpLine.Enqueue(tmpNode[posi - 1]);
                    }

                    finnalResult.Add(finnalResult.Count, tmpLine);
                }
            }

            return finnalResult;
        }

        #endregion

        #region 公共方法

        /// <summary>
        ///     从所有给定的路线中得到用时最小的一条线路
        /// </summary>
        /// <param name="routeResults">多条路小的集合</param>
        /// <returns>用时最少的一条路线</returns>
        private RoutePlanResult GetShortestTimepath(List<RoutePlanResult> routeResults)
        {
            var minWeight = routeResults.Min(p => p.weight);
            var minRoute = routeResults.Where(p => p.weight == minWeight).First();
            return minRoute;
        }

        /// <summary>
        ///     获取此线路上的所有换乘站
        /// </summary>
        /// <param name="lineID"></param>
        /// <returns></returns>
        private List<Node> GetTransferStationsByLine(string lineID)
        {
            var nodes = new List<Node>();
            for (var i = 0; i < DalStations.galPathExStations.Count; i++)
            {
                var tNode = DalStations.galPathExStations[i] as Node;
                if (tNode.PathId == lineID)
                    nodes.Add(tNode);
            }

            return nodes;
        }

        #endregion
    }
}