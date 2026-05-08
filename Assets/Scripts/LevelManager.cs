using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public static event Action OnLevelComplete;
    public static event Action OnLevelFailed;

    public static event Action OnGameComplated;


    public static event Action<int> OnSecondsUpdated;
    public static event Action OnLevelStarted;
    public static event Action<int> OnStageUpdated;
    private int currentLevelIndex;
    private int remainingBlocks;
    private int secondsRemaining;
    private Coroutine countdownRoutine = null;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GridController.OnBlockExit += OnBlockConsumed;
        AudioManager.instance.PlaySoundByName("Background");
        StartGame();
    }

    public void StartGame()
    {
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
            OnGameComplated?.Invoke();
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(json.text);
        GridController.Instance.ReinitGrid(data.gridWidth, data.gridHeight);
        GridController.Instance.LoadLevel(data);

        remainingBlocks = data.blocks?.Length ?? 0;
        currentLevelIndex = index;
        OnStageUpdated?.Invoke(currentLevelIndex);
        OnLevelStarted?.Invoke();
        secondsRemaining = data.seconds;
        StartCountdown();
    }

    public void RestartLevel() => LoadLevel(currentLevelIndex);

    public void NextLevel() => LoadLevel(currentLevelIndex + 1);
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    private void OnBlockConsumed()
    {
        remainingBlocks--;

        if (remainingBlocks <= 0)
        {

            StopCountdown();
            OnLevelComplete?.Invoke();
        }
    }

    private void StartCountdown()
    {
        StopCountdown();

        countdownRoutine = StartCoroutine(Countdown());
    }

    private void StopCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }
        countdownRoutine = null;
    }

    IEnumerator Countdown()
    {
        OnSecondsUpdated?.Invoke(secondsRemaining);

        while (secondsRemaining >= 0)
        {
            yield return new WaitForSeconds(1f);

            secondsRemaining--;

            OnSecondsUpdated?.Invoke(secondsRemaining);
        }

        OnLevelFailed?.Invoke();
    }
}