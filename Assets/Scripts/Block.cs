using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    ColorPalette.PaletteColor color = ColorPalette.PaletteColor.Color1;
    bool isIced = false;
    int icedCounter = 0;
    public RigidbodyConstraints constarint {get; private set;}


    void Start()
    {
        


    }
}

