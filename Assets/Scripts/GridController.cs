using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    private Grid<GridObject> grid;
    [SerializeField] private Transform floorPrefab;
    [SerializeField] private PlacedObjectTypeSO[] placedObjectTypeSO;
    public ColorPalette colorPalette;

    private Dictionary<string, PlacedObjectTypeSO> soByName;

    public static event Action OnBlockExit;

    private void Awake()
    {
        Instance = this;

        soByName = new Dictionary<string, PlacedObjectTypeSO>();
        foreach (var so in placedObjectTypeSO)
            if (so != null) soByName[so.nameString] = so;

        int gridWidth = 5;
        int gridHeight = 5;
        float cellSize = 1f;
        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, transform.position, (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));
        Camera.main.transform.position = new Vector3(gridWidth / 2 * cellSize, gridHeight / 2 * cellSize, -10f);
    }

    void Start()
    {
    }

    public void LoadLevel(LevelData data)
    {
        foreach (var e in data.floors ?? new FloorEntry[0])
            SpawnFloor(grid.GetWorldPosition(e.x, e.y));

        foreach (var e in data.walls ?? new PlacedObjectEntry[0])
            SpawnFromEntry(e);

        foreach (var e in data.grinders ?? new ColoredObjectEntry[0])
        {
            PlacedObject po = SpawnFromEntry(e);
            if (po != null && !string.IsNullOrEmpty(e.color))
                po.GetComponent<Grinder>()?.SetColor(ParsePaletteColor(e.color));
        }

        foreach (var e in data.blocks ?? new BlockEntry[0])
        {
            PlacedObject po = SpawnFromEntry(e);
            if (po == null) continue;
            Block block = po.GetComponent<Block>();
            if (block == null) continue;
            if (!string.IsNullOrEmpty(e.color))
                block.SetColor(ParsePaletteColor(e.color));
            block.SetIcedCounter(e.iceCounter);
            if (!string.IsNullOrEmpty(e.restrictedAxis))
                block.SetConstraints((RigidbodyConstraints)Enum.Parse(typeof(RigidbodyConstraints), e.restrictedAxis));
        }
    }
    public void BlockExit()
    {
        OnBlockExit?.Invoke();
    }
    private PlacedObject SpawnFromEntry(PlacedObjectEntry e)
    {
        if (!soByName.TryGetValue(e.typeName, out PlacedObjectTypeSO so))
        {
            Debug.LogWarning($"LoadLevel: unknown typeName '{e.typeName}', skipping.");
            return null;
        }
        PlacedObjectTypeSO.Dir dir = PlacedObjectTypeSO.Dir.Down;
        if (!string.IsNullOrEmpty(e.direction))
            dir = (PlacedObjectTypeSO.Dir)Enum.Parse(typeof(PlacedObjectTypeSO.Dir), e.direction);
        return SpawnBlock(so, new Vector2Int(e.x, e.y), dir);
    }

    private ColorPalette.PaletteColor ParsePaletteColor(string s) =>
        (ColorPalette.PaletteColor)Enum.Parse(typeof(ColorPalette.PaletteColor), s);

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

    public void GetGridXY(Vector3 worldPos, out int x, out int y) => grid.GetXY(worldPos, out x, out y);
    public void GetGridXYRounded(Vector3 worldPos, out int x, out int y) => grid.GetXYRounded(worldPos, out x, out y);

    public Vector3 GetWorldPosition(int x, int y) => grid.GetWorldPosition(x, y);

    public float GetCellSize() => grid.GetCellSize();

    public void RemoveBlock(PlacedObject block)
    {
        foreach (Vector2Int cell in block.GetGridPositionList())
            grid.GetGridObject(cell.x, cell.y)?.ClearPlacedObject();
    }

    public void RegisterBlock(PlacedObject block)
    {
        foreach (Vector2Int cell in block.GetGridPositionList())
            grid.GetGridObject(cell.x, cell.y)?.SetPlacedObject(block);
    }

    public bool CanPlaceAt(PlacedObjectTypeSO type, Vector2Int origin, PlacedObjectTypeSO.Dir dir)
    {
        List<Vector2Int> cells = type.GetGridPositionList(origin, dir);
        foreach (Vector2Int cell in cells)
        {
            GridObject go = grid.GetGridObject(cell.x, cell.y);
            if (go == null || go.isOccupied()) return false;
        }
        return true;
    }

    public Vector2Int? GetNearestFreeOrigin(PlacedObjectTypeSO type, Vector2Int target, PlacedObjectTypeSO.Dir dir)
    {
        for (int radius = 0; radius <= 4; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;
                Vector2Int candidate = target + new Vector2Int(dx, dy);
                if (CanPlaceAt(type, candidate, dir)) return candidate;
            }
        }
        return null;
    }

    public void PlaceExistingBlock(PlacedObject block, Vector2Int newOrigin)
    {
        PlacedObjectTypeSO type = block.GetPlacedObjectTypeSO();
        PlacedObjectTypeSO.Dir dir = block.GetDir();
        block.SetOrigin(newOrigin);
        Vector2Int rotOffset = type.GetRotationOffset(dir);
        Vector3 worldPos = grid.GetWorldPosition(newOrigin.x, newOrigin.y)
                         + new Vector3(rotOffset.x, rotOffset.y) * grid.GetCellSize();
        block.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
        foreach (Vector2Int cell in block.GetGridPositionList())
            grid.GetGridObject(cell.x, cell.y)?.SetPlacedObject(block);
    }
}
public class GridObject
{
    private Grid<GridObject> grid; 
    private int x; 
    private int y; 
    public PlacedObject placedObject; 

    
    public GridObject(Grid<GridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        placedObject = null;
    }
    public override string ToString() => $"{x}, {y}\n{placedObject}";

    
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


    
    public bool isOccupied() => placedObject != null;
}
