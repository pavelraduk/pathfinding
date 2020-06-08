using System.Collections.Generic;

namespace Assets.PathfindingAlgorithm
{
    public class NodesInVertex
    {
        public void InsertNode(int nodeId, int lastmove, int cornersCount)
        {
            if (!m_nodesByCornersCount.ContainsKey(cornersCount))
            {
                m_nodesByCornersCount.Add(cornersCount, new Dictionary<int, int>());
            }

            var nodeByLastmove = m_nodesByCornersCount[cornersCount];
            nodeByLastmove.Add(lastmove, nodeId);
        }

        public bool HasNode(int lastmove, int cornersCount)
        {
            if (!m_nodesByCornersCount.ContainsKey(cornersCount))
            {
                return false;
            }

            return m_nodesByCornersCount[cornersCount].ContainsKey(lastmove);
        }

        public int GetNodeId(int lastmove, int cornersCount)
        {
            return m_nodesByCornersCount[cornersCount][lastmove];
        }

        // Corners count => lastmove => node id
        private readonly Dictionary<int, Dictionary<int , int>> m_nodesByCornersCount = new Dictionary<int, Dictionary<int, int>>();
    }
}