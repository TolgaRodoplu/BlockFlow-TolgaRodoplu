using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameComplatedUI : UIContainer
{
    void Start()
    {
        LevelManager.OnGameComplated += OpenUI;
        LevelManager.OnLevelStarted += CloseUI;
    }
}
