using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    private Grid<GridObject> grid;
    [SerializeField] private Transform floorPrefab;
    [SerializeField] private PlacedObjectTypeSO[] placedObjectTypeSO;
    private PlacedObject currentDragging = null;
    
    private void Awake()
    {
        Instance = this;
        
        int gridWidth = 5;
        int gridHeight = 5;
        float cellSize = 1f;
        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, transform.position, (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));
        Camera.main.transform.position = new Vector3(gridWidth/2 * cellSize, gridHeight/2 * cellSize, -10f);
    }

    void Start()
    {
        SpawnBlock(placedObjectTypeSO[0], new Vector2Int(2, 2), PlacedObjectTypeSO.Dir.Down);
        SpawnBlock(placedObjectTypeSO[4], new Vector2Int(0, 0), PlacedObjectTypeSO.Dir.Down);
        SpawnBlock(placedObjectTypeSO[3], new Vector2Int(0, 1), PlacedObjectTypeSO.Dir.Down);
    }

    public void SpawnFloor(Vector3 pos)
    {
        Instantiate(floorPrefab, pos, Quaternion.identity, transform);

        Instantiate(floorPrefab, pos, Quaternion.identity, transform);
    }

    public PlacedObject SpawnBlock(PlacedObjectTypeSO type, Vector2Int origin, PlacedObjectTypeSO.Dir dir)
    {
        List<Vector2Int> cells = type.GetGridPositionList(origin, dir);

        foreach (Vector2Int cell in cells)
        {
            GridObject gridObject = grid.GetGridObject(cell.x, cell.y);

            if (gridObject == null || gridObject.isOccupied())
            {
                Debug.LogWarning($"SpawnBlock: cannot place {type.nameString} at {cell} — cell is out of bounds or occupied.");
                return null;
            }
        }

        Vector2Int rotOffset = type.GetRotationOffset(dir);
        Vector3 worldPos = grid.GetWorldPosition(origin.x, origin.y)
                         + new Vector3(rotOffset.x, rotOffset.y) * grid.GetCellSize();

        PlacedObject placedObject = PlacedObject.Create(worldPos, origin, dir, type);

        foreach (Vector2Int cell in cells)
            grid.GetGridObject(cell.x, cell.y).SetPlacedObject(placedObject);

        return placedObject;
    }
}
public class GridObject
{
    private Grid<GridObject> grid; // Reference to parent grid
    private int x; // Grid X coordinate
    private int y; // Grid Y coordinate
    public PlacedObject placedObject; // Building placed on this cell (null if floor)

    /// <summary>
    /// Creates a new GridObject for a specific grid position.
    /// </summary>
    public GridObject(Grid<GridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        placedObject = null;
    }
    public override string ToString() => $"{x}, {y}\n{placedObject}";

    /// <summary>
    /// Assigns a building to this cell and notifies the grid of the change.
    /// </summary>


    /// <summary>
    /// Removes the building from this cell and notifies the grid.
    /// </summary>
    public void SetPlacedObject(PlacedObject placedObject)
    {
        this.placedObject = placedObject;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void ClearPlacedObject()
    {
        placedObject = null;
        grid.TriggerGridObjectChanged(x, y);
    }


    /// <summary>
    /// Returns true if no building is placed on this cell.
    /// </summary>
    public bool isOccupied() => placedObject != null;
}
