using System.Collections.Generic;
using UnityEngine;

public class Grinder : MonoBehaviour
{
    public ColorPalette.PaletteColor color = ColorPalette.PaletteColor.Color1;
    public List<Vector2Int> entryCells;
    public List<PlacedObject> objectsToCheck;
    
    void Start()
    {
        DragDropController.OnPosChanged += CheckEntry;
    }

    public void SetColor(ColorPalette.PaletteColor paletteColor)
    {
        color = paletteColor;
        transform.GetComponentInChildren<MeshRenderer>().material.color = GridController.Instance.colorPalette.GetColor(color);
    }


    
    public void CheckEntry(object sender, PlacedObject placedObject)
    {
        if(!objectsToCheck.Contains(placedObject)) return;

        Debug.Log("EnteredFunc");
        
        Block block = placedObject.GetComponent<Block>();
        if (block == null || block.color != color) return;

        Debug.Log("isBlock and color matches");

        PlacedObject thisPlacedSO = GetComponent<PlacedObject>();

        PlacedObjectTypeSO blockSO = placedObject.GetPlacedObjectTypeSO();
        PlacedObjectTypeSO.Dir blockDir = placedObject.GetDir();
        int requiredSize = entryCells.Count;
        if(((thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Down || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Up) && (blockSO.GetRotatedHeight(blockDir) > requiredSize)) ||
            (thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Left || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Right) && (blockSO.GetRotatedWidth(blockDir) > requiredSize))
            return;

        Debug.Log("size matches");

        
        PlacedObject grinderPO = GetComponent<PlacedObject>();
        Vector2Int grinderOrigin = grinderPO.GetOrigin();
        PlacedObjectTypeSO.Dir grinderDir = grinderPO.GetDir();

        List<Vector2Int> absoluteEntryCells = new List<Vector2Int>();
        foreach (Vector2Int cell in entryCells)
            absoluteEntryCells.Add(grinderOrigin + PlacedObjectTypeSO.RotateVector(cell, grinderDir));

        
        List<Vector2Int> blockPositions = placedObject.GetGridPositionList();
        foreach (Vector2Int entryCell in absoluteEntryCells)
        {
            Debug.Log(entryCell);

            if (!blockPositions.Contains(entryCell)) return;
        }

        Debug.Log("Fills");

        GridController.Instance.RemoveBlock(placedObject);
        placedObject.DestroySelf();
        GridController.Instance.BlockExit();
        return;
    }

    void OnTriggerEnter(Collider other)
    {
        var block = other.GetComponent<Block>();

        if(block == null) return;

        objectsToCheck.Add(block.GetComponent<PlacedObject>());
    }

    void OnTriggerExit(Collider other)
    {
        var po = other.GetComponent<PlacedObject>();

        if(po == null) return;

        if(!objectsToCheck.Contains(po)) return;

        objectsToCheck.Remove(po);
    }
}
