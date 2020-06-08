using System;
using System.Collections.Generic;

namespace Assets.PathfindingAlgorithm
{
    public class NodesStore<TVertexId> : IComparer<int>
    {
        public int Compare(int firstNodeId, int secondNodeId)
        {
            var firstNode = GetNode(firstNodeId);
            var secondNode = GetNode(secondNodeId);

            if (firstNode.Priority == secondNode.Priority)
            {
                return 0;
            }

            return firstNode.Priority < secondNode.Priority ? -1 : 1;
        }

        public bool EmptyHeap => m_heap.Empty;

        public NodesStore()
        {
            m_heap = new Heap<int>(this);
        }

        public void Clear()
        {
            m_heap.Clear();
            m_nodes.Clear();
            m_nodesInVertex.Clear();
        }

        public Node<TVertexId> GetNode(int nodeId)
        {
            return m_nodes[nodeId];
        }

        public Node<TVertexId> GetNode(TVertexId vertex, int lastmove, int cornersCount)
        {
            return GetNode(m_nodesInVertex[vertex].GetNodeId(lastmove, cornersCount));
        }

        public bool HasNode(TVertexId vertex, int lastmove, int cornersCount)
        {
            if (!m_nodesInVertex.ContainsKey(vertex))
            {
                return false;
            }

            return m_nodesInVertex[vertex].HasNode(lastmove, cornersCount);
        }

        public int CreateStartNode(TVertexId vertex)
        {
            return CreateNode(vertex, 0, -1, 0, 0, 0);
        }

        public int CreateNode(TVertexId vertex, float distanceFromStart, int predecessor, float priority, int currentCornersCount, int lastmove)
        {
            var node = new Node<TVertexId>(vertex, m_nodes.Count, distanceFromStart, predecessor, priority, currentCornersCount, lastmove);
            m_nodes.Add(node);

            if (!m_nodesInVertex.ContainsKey(vertex))
            {
                NodesInVertex nodesInVertex = new NodesInVertex();
                nodesInVertex.InsertNode(node.Id, node.Lastmove, node.CurrentCornersCount);

                m_nodesInVertex.Add(vertex, nodesInVertex);
            }
            else
            {
                NodesInVertex nodesInVertex = m_nodesInVertex[vertex];
                nodesInVertex.InsertNode(node.Id, node.Lastmove, node.CurrentCornersCount);
            }

            return node.Id;
        }

        public void InsertNodeToHeap(int nodeId)
        {
            int handle = m_heap.Insert(nodeId);

            var node = m_nodes[nodeId];
            node.Handle = handle;
            node.Color = NodeColor.Gray;

            m_nodes[nodeId] = node;
        }

        public void RelaxNode(int nodeId, int predecessorId, float newDistanceFromStart, float heuristic)
        {
            var node = m_nodes[nodeId];

            node.DistanceFromStart = newDistanceFromStart;
            node.PredecessorNodeId = predecessorId;
            node.Priority = newDistanceFromStart + heuristic;

            m_nodes[nodeId] = node;

            m_heap.SieveUp(node.Handle);
        }

        public Node<TVertexId> ExtractMin()
        {
            return m_nodes[m_heap.ExtractMin()];
        }

        public void SetNodeColor(int nodeId, NodeColor color)
        {
            var node = m_nodes[nodeId];
            node.Color = color;

            m_nodes[nodeId] = node;
        }

        private readonly Heap<int> m_heap;
        private readonly List<Node<TVertexId>> m_nodes = new List<Node<TVertexId>>();

        private readonly Dictionary<TVertexId, NodesInVertex> m_nodesInVertex = new Dictionary<TVertexId, NodesInVertex>();
    }
}
