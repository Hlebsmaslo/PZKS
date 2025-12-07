using System;
using System.Collections.Generic;
using System.Linq;

namespace PZKS2
{

    public static class ParallelExecutionModel
    {
        private static readonly Dictionary<string, int> OpTimes = new Dictionary<string, int>()
        {
            { "+", 1 }, 
            { "-", 1 },
            { "*", 2 }, 
            { "/", 3 } 
        };

        public static int MaxParallelismDegree { get; private set; } = 0;

        public static int CalculateParallelTime(Node node)
        {
            if (node is NumberNode || node is IdentifierNode)
            {
                return 0;
            }

            if (node is OperatorNode opNode)
            {
                var childrenFinishTimes = new List<int>();
                foreach (var child in opNode.Children)
                {
                    childrenFinishTimes.Add(CalculateParallelTime(child));
                }

                int latestOperandAvailable = childrenFinishTimes.Any() ? childrenFinishTimes.Max() : 0;

                if (!OpTimes.TryGetValue(opNode.Op, out int executionTime))
                {
                    executionTime = 1;
                }
                int startTime = latestOperandAvailable;
                int finishTime = startTime + executionTime;
                opNode.FinishTime = finishTime;

                return finishTime;
            }
            return 0;
        }
        public static int CalculateSequentialTime(Node node)
        {
            if (node is NumberNode || node is IdentifierNode)
            {
                return 0;
            }

            int totalTime = 0;
            if (node is OperatorNode opNode)
            {
                if (OpTimes.TryGetValue(opNode.Op, out int executionTime))
                {
                    totalTime += executionTime;
                }
                else
                {
                    totalTime += 1;
                }

                foreach (var child in opNode.Children)
                {
                    totalTime += CalculateSequentialTime(child);
                }
            }
            return totalTime;
        }
        public static int FindMaxParallelism(Node root)
        {
            var opLoadAtTime = new Dictionary<int, int>(); 
            Action<Node> findLoad = null;
            findLoad = (node) =>
            {
                if (node is OperatorNode opNode)
                {
                    int finishTime = opNode.FinishTime;
                    if (OpTimes.TryGetValue(opNode.Op, out int executionTime))
                    {
                        for (int t = finishTime - executionTime; t < finishTime; t++)
                        {
                            if (t >= 0)
                            {
                                if (opLoadAtTime.ContainsKey(t))
                                {
                                    opLoadAtTime[t]++;
                                }
                                else
                                {
                                    opLoadAtTime.Add(t, 1);
                                }
                            }
                        }
                    }

                    foreach (var child in opNode.Children)
                    {
                        findLoad(child);
                    }
                }
            };

            findLoad(root);

            if (opLoadAtTime.Any())
            {
                MaxParallelismDegree = opLoadAtTime.Values.Max();
                return MaxParallelismDegree;
            }
            return 1;
        }
    }
}