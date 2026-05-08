using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public ColorPalette.PaletteColor color = ColorPalette.PaletteColor.Color1;
    public int iceCounter = 0;
    public bool isIced => iceCounter > 0;
    public RigidbodyConstraints constarint { get; private set; }
    public GameObject ArrowX;
    public GameObject ArrowY;
    public TextMeshPro iceText;
    public MeshRenderer mesh;

    void Start()
    {
        SetMat();
        if (isIced && iceText != null)
            iceText.transform.rotation = Quaternion.identity;
    }

    public void PickedUp()
    {
        mesh.transform.position += new Vector3(0f, 0f, -1f);
    }
    public void PutDown()
    {
        mesh.transform.position += new Vector3(0f, 0f, 1f);
    }

    public void SetColor(ColorPalette.PaletteColor paletteColor)
    {
        color = paletteColor;
    }

    public void SetConstraints(RigidbodyConstraints c)
    {
        constarint = c;
        var dir = this.GetComponent<PlacedObject>().GetDir();
        if (dir == PlacedObjectTypeSO.Dir.Down || dir == PlacedObjectTypeSO.Dir.Up)
        {
            if ((c & RigidbodyConstraints.FreezePositionX) != 0)
            {
                ArrowY.SetActive(true);
            }
            else if ((c & RigidbodyConstraints.FreezePositionY) != 0)
            {
                ArrowX.SetActive(true);
            }
        }
        else if (dir == PlacedObjectTypeSO.Dir.Left || dir == PlacedObjectTypeSO.Dir.Right)
        {
            if ((c & RigidbodyConstraints.FreezePositionX) != 0)
            {
                ArrowX.SetActive(true);
            }
            else if ((c & RigidbodyConstraints.FreezePositionY) != 0)
            {
                ArrowY.SetActive(true);
            }
        }

        
    }
    public void SetIcedCounter(int cnt)
    {
        iceCounter = cnt;

        if(iceText != null)
            iceText.text = iceCounter.ToString();
    }

    private void UpdateIceCounter()
    {
        iceCounter--;
        AudioManager.instance.PlaySoundByName("Ice");
        if (!isIced)
            SetMat();
        else if(iceText != null)
            iceText.text = iceCounter.ToString();
        
    }

    private void SetMat()
    {
        var mat = mesh.material;
        if (isIced)
        {
            mat.SetTexture("_BaseMap", GridController.Instance.colorPalette.iceTexture);
            mat.color = Color.white;
            GridController.OnBlockExit += UpdateIceCounter;
            Debug.Log("Sa1");
        }
        else
        {
            Debug.Log("Sa2");
            mat.SetTexture("_BaseMap", null);
            Debug.Log("Sa3");
            mat.color = GridController.Instance.colorPalette.GetColor(color);
            Debug.Log("Sa4");
            if(iceText != null)
                Destroy(iceText.gameObject);
                Debug.Log("Sa5");
            GridController.OnBlockExit -= UpdateIceCounter;
        }
    }

}
