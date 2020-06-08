namespace Assets.PathfindingAlgorithm
{
    /// <summary>
    /// Нода в графе, на котором ищется путь
    /// </summary>
    public struct Node<TVertexId>
    {
        /// <summary>
        /// Идентификатор вершины, ассоциированой с нодой
        /// </summary>
        public TVertexId VertexId { get; private set; }

        /// <summary>
        /// Идентификатор ноды
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Идентификатор в куче
        /// </summary>
        public int Handle { get; set; }

        /// <summary>
        /// Предшественник в пути
        /// </summary>
        public int PredecessorNodeId { get; set; }

        /// <summary>
        /// Расстояние от начала пути
        /// </summary>
        public float DistanceFromStart { get; set; }

        /// <summary>
        /// Цвет вершины в алгоритмическом смысле
        /// </summary>
        public NodeColor Color { get; set; }

        /// <summary>
        /// Приоритет для хранения в куче
        /// </summary>
        public float Priority { get; set; }

        /// <summary>
        /// Текущее количество поворотов
        /// </summary>
        public int CurrentCornersCount { get; set; }

        /// <summary>
        /// Направление предыдущей ноды
        /// </summary>
        public int Lastmove { get; set; }
        

        public Node(
            TVertexId vertexId, 
            int nodeId, 
            float distanceFromStart, 
            int predecessor, 
            float priority,
            int currentCornersCount,
            int lastmove)
        {
            VertexId = vertexId;
            Id = nodeId;
            Handle = -1;

            DistanceFromStart = distanceFromStart;
            PredecessorNodeId = predecessor;
            Color = NodeColor.White;
            Priority = priority;

            CurrentCornersCount = currentCornersCount;
            Lastmove = lastmove;
        }
    }
}
