using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlacedObject : MonoBehaviour {

    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, PlacedObjectTypeSO.Dir dir, PlacedObjectTypeSO placedObjectTypeSO) {
        Transform placedObjectTransform = Instantiate(placedObjectTypeSO.prefab, worldPosition, Quaternion.Euler(0, 0, placedObjectTypeSO.GetRotationAngle(dir)));

        PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();
        placedObject.Setup(placedObjectTypeSO, origin, dir);

        return placedObject;
    }

    private static PlacedObject _currentSelected;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private Vector2Int origin;
    private PlacedObjectTypeSO.Dir dir;

    private void Setup(PlacedObjectTypeSO placedObjectTypeSO, Vector2Int origin, PlacedObjectTypeSO.Dir dir) {
        this.placedObjectTypeSO = placedObjectTypeSO;
        this.origin = origin;
        this.dir = dir;
    }

    public PlacedObjectTypeSO GetPlacedObjectTypeSO()
    {
        return placedObjectTypeSO;
    }

    

    public static void DeselectCurrent()
    {
        _currentSelected = null;
    }

    public void DemolishSelf()
    {
        _currentSelected = null;
    }


    public Vector2Int GetOrigin()
    {
        return origin;
    }

    public PlacedObjectTypeSO.Dir GetDir()
    {
        return dir;
    }

    public List<Vector2Int> GetGridPositionList() 
    {
        return placedObjectTypeSO.GetGridPositionList(origin, dir);
    }

    public void DestroySelf() 
    {
        Destroy(gameObject);
    }

    public override string ToString() 
    {
        return placedObjectTypeSO.nameString;
    }

}
