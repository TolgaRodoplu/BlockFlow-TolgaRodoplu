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

        Block block = placedObject.GetComponent<Block>();
        if (block == null || block.color != color) return;

        PlacedObject thisPlacedSO = GetComponent<PlacedObject>();

        PlacedObjectTypeSO blockSO = placedObject.GetPlacedObjectTypeSO();
        PlacedObjectTypeSO.Dir blockDir = placedObject.GetDir();
        int requiredSize = entryCells.Count;
        if (((thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Down || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Up) && (blockSO.GetRotatedHeight(blockDir) > requiredSize)) ||
            (thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Left || thisPlacedSO.GetDir() == PlacedObjectTypeSO.Dir.Right) && (blockSO.GetRotatedWidth(blockDir) > requiredSize))
            return;

        PlacedObject grinderPO = GetComponent<PlacedObject>();
        Vector2Int grinderOrigin = grinderPO.GetOrigin();
        PlacedObjectTypeSO.Dir grinderDir = grinderPO.GetDir();
        PlacedObjectTypeSO grinderSO = grinderPO.GetPlacedObjectTypeSO();
        int W = grinderSO.GetRotatedWidth(PlacedObjectTypeSO.Dir.Down);
        int H = grinderSO.GetRotatedHeight(PlacedObjectTypeSO.Dir.Down);
        List<Vector2Int> absoluteEntryCells = new List<Vector2Int>();
        foreach (Vector2Int cell in entryCells)
        {
            Vector2Int rotated;
            switch (grinderDir)
            {
                default:
                case PlacedObjectTypeSO.Dir.Down:  rotated = cell; break;
                case PlacedObjectTypeSO.Dir.Left:  rotated = new Vector2Int(H - 1 - cell.y, cell.x); break;
                case PlacedObjectTypeSO.Dir.Up:    rotated = new Vector2Int(W - 1 - cell.x, H - 1 - cell.y); break;
                case PlacedObjectTypeSO.Dir.Right: rotated = new Vector2Int(cell.y, W - 1 - cell.x); break;
            }
            absoluteEntryCells.Add(grinderOrigin + rotated);
        }

        bool isOk = false;
        List<Vector2Int> blockPositions = placedObject.GetGridPositionList();

        foreach (Vector2Int entryCell in absoluteEntryCells)
        {
            if (blockPositions.Contains(entryCell))
            {
                isOk = true;
                break;
            }
        }
        if (!isOk) return;

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


        int bMinX = blockPositions.Min(v => v.x);
        int bMaxX = blockPositions.Max(v => v.x);
        int bMinY = blockPositions.Min(v => v.y);
        int bMaxY = blockPositions.Max(v => v.y);
        for (int bx = bMinX; bx <= bMaxX; bx++)
        {
            for (int by = bMinY; by <= bMaxY; by++)
            {
                Vector2Int cell = new Vector2Int(bx, by);
                if (!blockPositions.Contains(cell) && GridController.Instance.IsCellOccupied(bx, by))
                    return;
            }
        }

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
                                      placedObject.transform.rotation,
                                      GridController.Instance.transform);
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
