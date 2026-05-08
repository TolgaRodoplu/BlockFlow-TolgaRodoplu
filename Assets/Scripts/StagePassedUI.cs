using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagePassedUI : UIContainer
{
    void Start()
    {
        LevelManager.OnLevelComplete += OpenUI;
        LevelManager.OnLevelStarted += CloseUI;
        LevelManager.OnGameComplated += CloseUI;
    }
}
