using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailedUI : UIContainer
{
    void Start()
    {
        LevelManager.OnLevelFailed += OpenUI;
        LevelManager.OnLevelStarted += CloseUI;
    }

}
