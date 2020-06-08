using System.Collections.Generic;

namespace Assets.PathfindingAlgorithm
{
    public interface IGraph<TVertexId>
    {
        IEnumerable<TVertexId> GetNeighbors(TVertexId vertex);
        bool AreVerticesEqual(TVertexId first, TVertexId second);
        float EdgeLength(TVertexId start, TVertexId end);
        bool IsPassableVertex(TVertexId vertex);

        float Heuristic(TVertexId source, TVertexId destination);

        int CalcLastmove(TVertexId vertex, TVertexId predecessor);
        bool HasCorner(TVertexId vertex, TVertexId predecessor, int predecesorLastmove);
    }
}
