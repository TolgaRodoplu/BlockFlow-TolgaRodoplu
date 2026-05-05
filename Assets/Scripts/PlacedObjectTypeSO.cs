using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlacedObjectTypeSO : ScriptableObject
{

    public static Dir GetNextDir(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Down: return Dir.Left;
            case Dir.Left: return Dir.Up;
            case Dir.Up: return Dir.Right;
            case Dir.Right: return Dir.Down;
        }
    }

    public enum Dir
    {
        Down,
        Left,
        Up,
        Right,
    }

    public string nameString;
    public Transform prefab;
    public int width;
    public int height;
    public List<Vector2Int> cells;

    private int GetCellsBoundsWidth()
    {
        int maxX = 0;
        foreach (Vector2Int cell in cells)
            if (cell.x > maxX) maxX = cell.x;
        return maxX + 1;
    }

    private int GetCellsBoundsHeight()
    {
        int maxY = 0;
        foreach (Vector2Int cell in cells)
            if (cell.y > maxY) maxY = cell.y;
        return maxY + 1;
    }

    public int GetRotationAngle(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Down: return 0;
            case Dir.Left: return 90;
            case Dir.Up: return 180;
            case Dir.Right: return 270;
        }
    }

    public Vector2Int GetRotationOffset(Dir dir)
    {
        int W = GetCellsBoundsWidth();
        int H = GetCellsBoundsHeight();
        switch (dir)
        {
            default:
            case Dir.Down:  return new Vector2Int(0, 0);
            case Dir.Left:  return new Vector2Int(0, W);
            case Dir.Up:    return new Vector2Int(W, H);
            case Dir.Right: return new Vector2Int(H, 0);
        }
    }

    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir)
    {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        int W = GetCellsBoundsWidth();
        int H = GetCellsBoundsHeight();

        foreach (Vector2Int cell in cells)
        {
            Vector2Int rotated;
            switch (dir)
            {
                default:
                case Dir.Down:
                    rotated = new Vector2Int(cell.x, cell.y);
                    break;
                case Dir.Left:
                    rotated = new Vector2Int(H - 1 - cell.y, cell.x);
                    break;
                case Dir.Up:
                    rotated = new Vector2Int(W - 1 - cell.x, H - 1 - cell.y);
                    break;
                case Dir.Right:
                    rotated = new Vector2Int(cell.y, W - 1 - cell.x);
                    break;
            }
            gridPositionList.Add(offset + rotated);
        }
        return gridPositionList;
    }

    [ContextMenu("Debug Print All Rotations")]
    private void DebugPrintAllRotations()
    {
        foreach (Dir dir in System.Enum.GetValues(typeof(Dir)))
        {
            var positions = GetGridPositionList(Vector2Int.zero, dir);
            Debug.Log($"Dir.{dir}: [{string.Join(", ", positions)}]");
        }
    }

}

