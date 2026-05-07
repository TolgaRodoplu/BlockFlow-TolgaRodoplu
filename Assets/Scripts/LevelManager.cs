using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public static event Action OnLevelComplete;
    private int currentLevelIndex;
    private int remainingBlocks;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GridController.OnBlockExit += OnBlockConsumed;
        LoadLevel(1);       
    }

    private void OnDestroy()
    {
        GridController.OnBlockExit -= OnBlockConsumed;
    }

    public void LoadLevel(int index)
    {
        GridController.Instance.ClearLevel();

        TextAsset json = Resources.Load<TextAsset>($"Levels/Level_{index:D2}");

        if (json == null)
        {
            Debug.Log($"LevelManager: no Level_{index:D2}.json found — game complete.");
            OnLevelComplete?.Invoke();
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(json.text);
        GridController.Instance.ReinitGrid(data.gridWidth, data.gridHeight);
        GridController.Instance.LoadLevel(data);

        remainingBlocks = data.blocks?.Length ?? 0;
        currentLevelIndex = index;
    }

    public void RestartLevel() => LoadLevel(currentLevelIndex);

    public void NextLevel() => LoadLevel(currentLevelIndex + 1);

    private void OnBlockConsumed()
    {
        remainingBlocks--;
        if (remainingBlocks <= 0)
        {
            OnLevelComplete?.Invoke();
            NextLevel();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
        if (Input.GetKeyDown(KeyCode.N)) NextLevel();
    }
}