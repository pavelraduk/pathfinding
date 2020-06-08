using System;
using System.Collections;
using System.Collections.Generic;
using Assets.PathfindingAlgorithm;
using UnityEngine;
using UnityEngine.Assertions;

public class Graph : IGraph<int>
{

    public int XCount { get; private set; }

    public int YCount { get; private set; }

    public void Create(int xCount, int yCount)
    {
        XCount = xCount;
        YCount = yCount;

        m_vertices.Clear();

        for (int i = 0; i < XCount * YCount; ++i)
        {
            m_vertices.Add(true);
        }
    }
    
    public IEnumerable<int> GetNeighbors(int vertex)
    {
        Vector2Int coords = GetVertexCoordinates(vertex);

        for (int i = 0; i < s_directions.Length; i++)
        {
            Vector2Int neighbor = coords + s_directions[i];

            if (HasVertex(neighbor) && IsPassableVertex(neighbor))
            {
                yield return GetVertex(neighbor);
            }
        }
    }

    public bool AreVerticesEqual(int first, int second)
    {
        return first == second;
    }

    public bool IsPassableVertex(int vertex)
    {
        return m_vertices[vertex];
    }

    public bool IsPassableVertex(Vector2Int coord)
    {
        return m_vertices[GetVertex(coord)];
    }

    public void SetPassable(Vector2Int coord)
    {
        m_vertices[GetVertex(coord)] = true;
    }

    public void SetImpassable(Vector2Int coord)
    {
        m_vertices[GetVertex(coord)] = false;
    }

    public float EdgeLength(int start, int end)
    {
        if (!IsPassableVertex(start) || !IsPassableVertex(end) || !AreNeighbors(start, end))
        {
            return float.MaxValue;
        }

        Vector2Int startCoords = GetVertexCoordinates(start);
        Vector2Int endCoords = GetVertexCoordinates(end);

        Vector2 offset = endCoords - startCoords;
        return offset.magnitude;
    }

    public float Heuristic(int source, int destination)
    {
        Vector2Int sourceCoords = GetVertexCoordinates(source);
        Vector2Int destinationCoords = GetVertexCoordinates(destination);

        Vector2 offset = destinationCoords - sourceCoords;
        return offset.magnitude;
    }

    public int CalcLastmove(int vertex, int predecessor)
    {
        Vector2Int vertexCoords = GetVertexCoordinates(vertex);
        Vector2Int predecessorCoords = GetVertexCoordinates(predecessor);

        Vector2Int direction = vertexCoords - predecessorCoords;

        for (int i = 0; i < s_directions.Length; ++i)
        {
            if (direction == s_directions[i])
            {
                return i + 1;
            }
        }

        throw new Exception("Direction is not found");
    }

    public bool HasCorner(int vertex, int predecessor, int predecessorLastmove)
    {
        if (predecessorLastmove == 0)
        {
            return false;
        }

        return CalcLastmove(vertex, predecessor) != predecessorLastmove;
    }

    public Vector2Int GetVertexCoordinates(int vertex)
    {
        return new Vector2Int (vertex % XCount, vertex / XCount);
    }

    public int GetVertex(Vector2Int coords)
    {
        return coords.y * XCount + coords.x;
    }

    public float CalcPathLength(List<int> path)
    {
        float length = 0;
        for (int i = 0; i < path.Count - 1; ++i)
        {
            length += EdgeLength(path[i], path[i + 1]);
        }

        return length;
    }

    private bool AreNeighbors(int first, int second)
    {
        Assert.IsTrue(first != second);

        Vector2Int firstCoords = GetVertexCoordinates(first);
        Vector2Int secondCoords = GetVertexCoordinates(second);

        Vector2Int diff = firstCoords - secondCoords;
        return Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1;
    }

    private bool HasVertex(int vertex)
    {
        return HasVertex(GetVertexCoordinates(vertex));
    }

    private bool HasVertex(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < XCount && coords.y >= 0 && coords.y < YCount;
    }

    private static Vector2Int[] s_directions =
    {
        Vector2Int.up ,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.right,
        Vector2Int.down + Vector2Int.right,
        Vector2Int.down,
        Vector2Int.down + Vector2Int.left,
        Vector2Int.left,
        Vector2Int.up + Vector2Int.left,
    };

    private readonly List<bool> m_vertices = new List<bool>();
}
