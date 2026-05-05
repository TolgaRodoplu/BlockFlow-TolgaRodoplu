using UnityEngine;

public class DragDropController : MonoBehaviour
{
    [SerializeField] private float followSpeed = 20f;
    private PlacedObject dragging;
    private Vector2Int originalOrigin;
    private Vector3 dragOffset;
    private Rigidbody dragRb;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryBeginDrag();
        else if (Input.GetMouseButtonUp(0) && dragging != null) EndDrag();
    }

    private void FixedUpdate()
    {
        if (dragRb == null) return;

        Vector3 target = GetMouseWorldPos() + dragOffset;
        target.z = 0f;
        dragRb.velocity = (target - dragRb.position) * followSpeed;
    }

    private void TryBeginDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        PlacedObject po = hit.transform.GetComponentInParent<PlacedObject>();
        if (po == null || po.GetObjectType() != PlacedObjectTypeSO.ObjectType.Block) return;

        dragging = po;
        originalOrigin = po.GetOrigin();
        dragOffset = po.transform.position - GetMouseWorldPos();
        dragOffset.z = 0f;

        GridController.Instance.RemoveBlock(po);

        dragRb = po.gameObject.AddComponent<Rigidbody>();
        dragRb.useGravity = false;
        dragRb.drag = 15f;
        dragRb.constraints = RigidbodyConstraints.FreezePositionZ
                           | RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationY
                           | RigidbodyConstraints.FreezeRotationZ;
    }

    private void EndDrag()
    {
        dragRb.velocity = Vector3.zero;
        Destroy(dragRb);
        dragRb = null;

        PlacedObjectTypeSO type = dragging.GetPlacedObjectTypeSO();
        PlacedObjectTypeSO.Dir dir = dragging.GetDir();
        Vector2Int rotOffset = type.GetRotationOffset(dir);
        float cellSize = GridController.Instance.GetCellSize();
        Vector3 originWorldPos = dragging.transform.position
            - new Vector3(rotOffset.x, rotOffset.y) * cellSize;
        GridController.Instance.GetGridXYRounded(originWorldPos, out int x, out int y);
        Vector2Int dropOrigin = new Vector2Int(x, y);

        Vector2Int? valid = GridController.Instance.GetNearestFreeOrigin(
            dragging.GetPlacedObjectTypeSO(), dropOrigin, dragging.GetDir());

        GridController.Instance.PlaceExistingBlock(dragging, valid ?? originalOrigin);
        dragging = null;
    }

    private static Vector3 GetMouseWorldPos()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = -Camera.main.transform.position.z;
        Vector3 world = Camera.main.ScreenToWorldPoint(mp);
        world.z = 0f;
        return world;
    }
}
