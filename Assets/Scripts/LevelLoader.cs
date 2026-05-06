using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private TextAsset levelJson;

    void Start()
    {
        if (levelJson == null)
        {
            Debug.LogWarning("LevelLoader: no level JSON assigned.");
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(levelJson.text);
        GridController.Instance.LoadLevel(data);
    }
}
