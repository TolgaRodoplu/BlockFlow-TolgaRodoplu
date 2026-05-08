using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;
    [SerializeField] private Image sfxToggleBtn;
    [SerializeField] private Image musicToggleBtn;

    void Start()
    {
        LevelManager.OnSecondsUpdated += UpdateTime;
        LevelManager.OnStageUpdated += UpdateStage;
    }

    private void UpdateTime(int seconds)
    {
        
        int min = seconds / 60;
        int sec = seconds % 60;

        string formattedTime = $"{min}:{sec:D2}";
        timeText.text = formattedTime.ToString();
    }

    private void UpdateStage(int stage)
    {
        stageText.text = "Stage - " + stage.ToString();
    }

    public void ToggleSFXBtn()
    {
        sfxToggleBtn.sprite = AudioManager.instance.ToggleSFX() ? sfxOnSprite : sfxOffSprite;
    }
    public void ToggleMusicBtn()
    {
        musicToggleBtn.sprite = AudioManager.instance.ToggleMusic() ? musicOnSprite : musicOffSprite;
    }
    
}
public interface IUIWindow
{
    public void OpenUI();
    public void CloseUI();
}
public class UIContainer : MonoBehaviour, IUIWindow
{
    [SerializeField] protected GameObject panel;
    public virtual void OpenUI()
    {
        panel.SetActive(true);
    }
    public virtual void CloseUI()
    {
        panel.SetActive(false);
    }
}