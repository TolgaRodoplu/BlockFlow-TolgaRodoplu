using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public ColorPalette.PaletteColor color = ColorPalette.PaletteColor.Color1;
    public int iceCounter = 0;
    public bool isIced => iceCounter > 0;
    public RigidbodyConstraints constarint { get; private set; }

    void Start()
    {
    }

    public void SetColor(ColorPalette.PaletteColor paletteColor)
    {
        color = paletteColor;
        transform.GetComponentInChildren<MeshRenderer>().material.color = ColorPalette.GetColor(color);
    }

    public void SetConstraints(RigidbodyConstraints c)
    {
        constarint = c;
    }
}
