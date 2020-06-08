using System.Collections.Generic;

namespace Assets.PathfindingAlgorithm
{
    /// <summary>
    /// Класс для поиска пути по графу, алгоритмом А*
    /// </summary>
    public class Pathfinder<TVertexId>
    {
        public bool TryFindPath(IGraph<TVertexId> graph, TVertexId source, TVertexId destination, List<TVertexId> outPath, 
            int maxCorners = -1, int limit = 0)
        {
            // Очистим хранилище
            m_nodesStore.Clear();
            
            // Создадим ноду старта
            int nodeId = m_nodesStore.CreateStartNode(source);
            m_nodesStore.InsertNodeToHeap(nodeId);

            int blackNodesCount = 0;
            while (!m_nodesStore.EmptyHeap)
            {
                if (limit != 0 && blackNodesCount >= limit)
                {
                    m_nodesStore.Clear();
                    return false;
                }

                // Получим еще непосещенную вершину, ближайшую к концу пути
                Node<TVertexId> currentNode = m_nodesStore.ExtractMin();

                // Если мы ее посетили, то переходим к следующей ноде
                if (currentNode.Color == NodeColor.Black)
                {
                    continue;
                }

                // Отметим рассматриваемую ноду как посещенную
                m_nodesStore.SetNodeColor(currentNode.Id, NodeColor.Black);
                ++blackNodesCount;

                // Если вершина рассматриваемой ноды совпадает с целевой вершиной, то завершаем поиск
                if (graph.AreVerticesEqual(currentNode.VertexId, destination))
                {
                    MakePath(currentNode, outPath);
                    m_nodesStore.Clear();
                    return true;
                }

                // Извлечем соседей рассматриваемой ноды
                FillNeighbours(currentNode, graph, destination, maxCorners);

                for (int i = 0; i < m_neighbours.Count; ++i)
                {
                    var neighbor = m_neighbours[i];

                    if (neighbor.Color == NodeColor.Black)
                    {
                        // Нас не интересуют уже посещенные соседи
                        continue;
                    }

                    if (neighbor.Color == NodeColor.Gray)
                    {
                        float edgeLength = graph.EdgeLength(currentNode.VertexId, neighbor.VertexId);
                        float newDistance = currentNode.DistanceFromStart + edgeLength;

                        // Если нода в куче, то обновим расстояние до соседа,
                        // если через рассматриваемую вершину, до него можно добраться быстрее
                        if (newDistance < neighbor.DistanceFromStart)
                        {
                            m_nodesStore.RelaxNode(neighbor.Id, currentNode.Id, newDistance, graph.Heuristic(neighbor.VertexId, destination));
                        }
                    }
                    else
                    {
                        // Если нода не в куче, то запихнем ее в кучу
                        m_nodesStore.InsertNodeToHeap(neighbor.Id);
                    }
                }
            }

            m_nodesStore.Clear();
            return false;
        }

        private void MakePath(Node<TVertexId> to, List<TVertexId> outPath)
        {
            outPath.Clear();

            Node<TVertexId> node = to;
            outPath.Add(node.VertexId);

            while (node.PredecessorNodeId >= 0)
            {
                node = m_nodesStore.GetNode(node.PredecessorNodeId);
                outPath.Add(node.VertexId);
            }

            outPath.Reverse();
        }

        private void FillNeighbours(Node<TVertexId> node, IGraph<TVertexId> graph, TVertexId destination, int maxCorners)
        {
            m_neighbours.Clear();

            foreach (var vertex in graph.GetNeighbors(node.VertexId))
            {
                // Нас не интересуют соседи, через которые путешественник не может пройти
                if (!graph.IsPassableVertex(vertex))
                {
                    continue;
                }

                int lastmove = graph.CalcLastmove(vertex, node.VertexId);
                int cornersCount = node.CurrentCornersCount + (graph.HasCorner(vertex, node.VertexId, node.Lastmove) ? 1 : 0);

                if (maxCorners >= 0 && cornersCount > maxCorners)
                {
                    continue;
                }

                if (maxCorners < 0)
                {
                    lastmove = 0;
                    cornersCount = 0;
                }
                
                if (m_nodesStore.HasNode(vertex, lastmove, cornersCount))
                {
                    m_neighbours.Add(m_nodesStore.GetNode(vertex, lastmove, cornersCount));
                }
                else
                {
                    float edgeLength = graph.EdgeLength(node.VertexId, vertex);
                    float distanceFromStart = node.DistanceFromStart + edgeLength;

                    float heuristic = graph.Heuristic(vertex, destination);

                    int nodeId = m_nodesStore.CreateNode(vertex, distanceFromStart, node.Id, distanceFromStart + heuristic,
                        cornersCount, lastmove);

                    m_neighbours.Add(m_nodesStore.GetNode(nodeId));
                }
            }
        }

        private readonly List<Node<TVertexId>> m_neighbours = new List<Node<TVertexId>>();
        private readonly NodesStore<TVertexId> m_nodesStore = new NodesStore<TVertexId>();
    }
}
