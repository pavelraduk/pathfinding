using System.Collections.Generic;
using System.Globalization;
using Assets.PathfindingAlgorithm;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    private Vector2Int InvalidBrush => new Vector2Int(m_field.XCount, m_field.YCount);

    private void Start()
    {
        CheckParametersAndFixIfNecessary();
        CreateField();
    }

    private void CheckParametersAndFixIfNecessary()
    {
        if (m_Xcount <= 1)
        {
            m_Xcount = 2;
        }
        if (m_Ycount <= 1)
        {
            m_Ycount = 2;
        }
    }

    private void CreateField()
    {
        m_field.Create(m_Xcount, m_Ycount);

        PlaceStart(Vector2Int.zero);
        PlaceFinish(new Vector2Int(m_field.XCount - 1, m_field.YCount - 1));

        m_path.Clear();
    }

    private void Update()
    {
        CheckParametersAndFixIfNecessary();
        
        if (m_Xcount != m_field.XCount || m_Ycount != m_field.YCount)
        {
            CreateField();
        }

        Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
        if (EventSystem.current.IsPointerOverGameObject() || !Physics.Raycast(ray, out var hit))
        {
            return;
        }

        Vector2Int cell = GetHitCell(hit);

        if (Input.GetMouseButton(0))
        {
            DrawObstacle(cell);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ToggleObstacle(cell);
        }

        if (Input.GetMouseButtonUp(1))
        {
            PlaceStart(cell);
        }

        if (Input.GetMouseButtonUp(2))
        {
            PlaceFinish(cell);
        }
    }

    private Vector2Int GetHitCell(RaycastHit hit)
    {
        Bounds bounds = GetComponent<MeshRenderer>().bounds;
        Vector3 offset = hit.point - bounds.min;

        float size = bounds.size.x / m_Xcount;
        int x = Mathf.FloorToInt(offset.x / size);
        int y = Mathf.FloorToInt(offset.z / size);

        return new Vector2Int(x, y);
    }

    private void PlaceStart(Vector2Int cell)
    {
        if (cell != m_start && cell != m_end)
        {
            m_field.SetPassable(cell);
            m_start = cell;

            m_path.Clear();
        }

        m_brashCell = InvalidBrush;
    }

    private void PlaceFinish(Vector2Int cell)
    {
        if (cell != m_start && cell != m_end)
        {
            m_field.SetPassable(cell);
            m_end = cell;
        }

        m_brashCell = InvalidBrush;
    }

    private void ToggleObstacle(Vector2Int cell)
    {
        if (cell != m_start && cell != m_end && cell != m_brashCell)
        {
            if (m_field.IsPassableVertex(cell))
            {
                m_field.SetImpassable(cell);
            }
            else
            {
                m_field.SetPassable(cell);
            }
        }

        m_brashCell = InvalidBrush;
    }

    private void DrawObstacle(Vector2Int cell)
    {
        if (cell != m_start && cell != m_end && cell != m_brashCell)
        {
            ToggleObstacle(cell);
            m_brashCell = cell;
        }
    }
    
    private void OnDrawGizmos()
    {
        CheckParametersAndFixIfNecessary();

        float ratio = (float) m_Xcount / m_Ycount;
        gameObject.transform.localScale = new Vector3(ratio, 1, 1);

        Bounds bounds = GetComponent<MeshRenderer>().bounds;
        float size = bounds.size.x / m_Xcount;

        // Vertical lines
        for (int i = 0; i < m_Xcount; ++i)
        {
            Vector3 start = bounds.min + Vector3.right * size * i + Vector3.up * 0.01f;
            Vector3 end = new Vector3(bounds.min.x, 0, bounds.max.z) + Vector3.right * size * i + Vector3.up * 0.01f;

            Debug.DrawLine(start, end, Color.red / 2);
        }

        // Horizontal lines
        for (int i = 0; i < m_Ycount; ++i)
        {
            Vector3 start = bounds.min + Vector3.forward * size * i + Vector3.up * 0.01f;
            Vector3 end = new Vector3(bounds.max.x, 0, bounds.min.z) + Vector3.forward * size * i + Vector3.up * 0.01f;

            Debug.DrawLine(start, end, Color.red / 2);
        }

        Vector3 cubeSize = new Vector3(size, 0.02f, size);

        // Draw obstacles
        for (int x = 0; x < m_field.XCount; ++x)
        {
            for (int y = 0; y < m_field.YCount; ++y)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!m_field.IsPassableVertex(cell))
                {
                    Vector3 center = CoordToMap(cell, bounds, size);

                    //var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //go.transform.position = center;

                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(center, cubeSize);
                }
            }
        }
        
        // Draw start
        Vector3 startCenter = CoordToMap(m_start, bounds, size);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(startCenter, cubeSize);

        // Draw end
        Vector3 endCenter = CoordToMap(m_end, bounds, size);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(endCenter, cubeSize);

        // Draw path
        for (int i = 0; i < m_path.Count - 1; ++i)
        {
            Vector2Int cell = m_field.GetVertexCoordinates(m_path[i]);
            Vector2Int nextCell = m_field.GetVertexCoordinates(m_path[i + 1]);

            Vector3 cellCenter = CoordToMap(cell, bounds, size);
            Vector3 nextCoordCenter = CoordToMap(nextCell, bounds, size);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cellCenter, nextCoordCenter);
        }
    }

    public void TryFindPath()
    {
        if (!int.TryParse(m_maxCornersInputField.text, out int maxCorners))
        {
            Debug.LogError("Incorrect max corners count");
            return;
        }

        m_path.Clear();
        if (!m_pathfinder.TryFindPath(m_field, m_field.GetVertex(m_start), m_field.GetVertex(m_end), m_path, maxCorners))
        {
            m_path.Clear();
            m_outputText.text = "Path is not found";
            m_outputText.color = Color.red;
            Debug.Log("Path is not found");
        }
        else
        {
            m_outputText.color = Color.green;
            m_outputText.text = "Path length = " + m_field.CalcPathLength(m_path).ToString(CultureInfo.InvariantCulture);
        }
    }

    public void ClearObstacles()
    {
        for (int x = 0; x < m_field.XCount; ++x)
        {
            for (int y = 0; y < m_field.YCount; ++y)
            {
                m_field.SetPassable(new Vector2Int(x, y));
            }
        }
    }

    private static Vector3 CoordToMap(Vector2Int cell, Bounds bounds, float size)
    {
        return bounds.min + new Vector3(cell.x * size + 0.5f * size, 0, cell.y * size + 0.5f * size);
    }

    private readonly Graph m_field = new Graph();
    private readonly Pathfinder<int> m_pathfinder = new Pathfinder<int>();
    private readonly List<int> m_path = new List<int>();

    private Vector2Int m_start = Vector2Int.zero;
    private Vector2Int m_end = Vector2Int.zero;
    private Vector2Int m_brashCell = Vector2Int.zero;
    
    [SerializeField]
    private InputField m_maxCornersInputField;

    [SerializeField]
    private Text m_outputText;

    [SerializeField]
    private int m_Xcount;

    [SerializeField]
    private int m_Ycount;

    [SerializeField]
    private Camera m_camera;
}
