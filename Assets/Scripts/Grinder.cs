using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Grinder : MonoBehaviour
{
    [HideInInspector] public ColorPalette.PaletteColor color = ColorPalette.PaletteColor.Color1;
    [SerializeField] private List<Vector2Int> entryCells;
    private List<PlacedObject> objectsToCheck = new List<PlacedObject>();
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private Vector3 blockStartPos;
    [SerializeField] private Vector3 blockEndPos;

    void Start()
    {
        DragDropController.OnPosChanged += CheckEntry;
    }

    public void SetColor(ColorPalette.PaletteColor paletteColor)
    {
        color = paletteColor;
        var newColor = GridController.Instance.colorPalette.GetColor(color);
        transform.GetComponentInChildren<MeshRenderer>().material.color = newColor;

        if (particle != null)
        {
            var main = particle.main;
            main.startColor = newColor;
        }
    }



    public void CheckEntry(PlacedObject placedObject)
    {
        if (!objectsToCheck.Contains(placedObject)) return;

        Debug.Log("EnteredFunc");

        Block block = placedObject.GetComponent<Block>();
        if (block == null || block.color != color) return;

        Debug.Log("isBlock and color matches");

        PlacedObject thisPlacedSO = GetComponent<PlacedObject>();

        PlacedObjectTypeSO blockSO = placedObject.GetPlacedObjectTypeSO();
        PlacedObjectTypeSO.Dir blockDir = placedObject.GetDir();
        int requiredSize = entryCells.Count;
        if (((thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Down || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Up) && (blockSO.GetRotatedHeight(blockDir) > requiredSize)) ||
            (thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Left || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Right) && (blockSO.GetRotatedWidth(blockDir) > requiredSize))
            return;

        Debug.Log("size matches");


        PlacedObject grinderPO = GetComponent<PlacedObject>();
        Vector2Int grinderOrigin = grinderPO.GetOrigin();
        PlacedObjectTypeSO.Dir grinderDir = grinderPO.GetDir();
        Debug.Log("size matches1");
        List<Vector2Int> absoluteEntryCells = new List<Vector2Int>();
        foreach (Vector2Int cell in entryCells)
            absoluteEntryCells.Add(grinderOrigin + PlacedObjectTypeSO.RotateVector(cell, grinderDir));

        bool isOk = false;
        List<Vector2Int> blockPositions = placedObject.GetGridPositionList();
        Debug.Log("size matches2");

        foreach (Vector2Int entryCell in absoluteEntryCells)
        {
            if (blockPositions.Contains(entryCell))
            {
                isOk = true;
                break;
            }
        }
        Debug.Log("size matches3");
        if (!isOk) return;

        Debug.Log("size matches4");
        if ((thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Down || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Up))
        {
            int min = absoluteEntryCells.Min(v => v.y);
            int max = absoluteEntryCells.Max(v => v.y);

            foreach (var cell in blockPositions)
            {
                if (cell.y > max || cell.y < min)
                {
                    return;
                }
            }
        }

        else if ((thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Right || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Left))
        {
            int min = absoluteEntryCells.Min(v => v.x);
            int max = absoluteEntryCells.Max(v => v.x);

            foreach (var cell in blockPositions)
            {
                if (cell.x > max || cell.x < min)
                {
                    return;
                }
            }
        }


        Debug.Log("Fills");
        this.GetComponentInChildren<Animator>().Play("Grind");
        particle.Play();
        SpawnEntryVisual(placedObject);
        GridController.Instance.RemoveBlock(placedObject);
        AudioManager.instance.PlaySoundByName("Grind");
        placedObject.DestroySelf();
        GridController.Instance.BlockExit();
        return;
    }

    private void SpawnEntryVisual(PlacedObject placedObject)
    {
        DragDropController.instance.EndDrag();
        GameObject copy = Instantiate(placedObject.gameObject,
                                      placedObject.transform.position,
                                      placedObject.transform.rotation);
        Destroy(copy.GetComponent<Block>());
        Destroy(copy.GetComponent<PlacedObject>());
        foreach (var col in copy.GetComponentsInChildren<Collider>())
            Destroy(col);

        PlacedObjectTypeSO.Dir grinderDir = GetComponent<PlacedObject>().GetDir();
        Vector3 add = Vector3.zero;
        if (grinderDir == PlacedObjectTypeSO.Dir.Down)
        {
            add = new Vector3(-5f, 0f, 0f);
        }
        else if (grinderDir == PlacedObjectTypeSO.Dir.Up)
        {
            add = new Vector3(5f, 0f, 0f);
        }
        else if (grinderDir == PlacedObjectTypeSO.Dir.Left)
        {
            add = new Vector3(0f, -5f, 0f);
        }
        else if (grinderDir == PlacedObjectTypeSO.Dir.Right)
        {
            add = new Vector3(0f, 5f, 0f);
        }

        Vector3 target = copy.transform.position + add;
        StartCoroutine(MoveBlock(copy, copy.transform.position, target, 1f));
    }

    void OnTriggerEnter(Collider other)
    {
        var block = other.GetComponent<Block>();

        if (block == null) return;

        objectsToCheck.Add(block.GetComponent<PlacedObject>());
    }

    private IEnumerator MoveBlock(GameObject copy, Vector3 snapPos, Vector3 targetPos, float duration)
    {

        copy.transform.position = snapPos;
        Vector3 originalScale = copy.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            copy.transform.position = Vector3.Lerp(snapPos, targetPos, t);
            yield return null;
        }
        Destroy(copy);
    }

    void OnTriggerExit(Collider other)
    {
        var po = other.GetComponent<PlacedObject>();

        if (po == null) return;

        if (!objectsToCheck.Contains(po)) return;

        objectsToCheck.Remove(po);
    }
}
