using UnityEngine;

public class SettingsLoader : MonoBehaviour
{
    public GameObject settingsPrefab;

    void Awake()
    {
        if (SettingsManager.Instance == null)
        {
            Instantiate(settingsPrefab);
        }
    }
}