using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public AudioMixer mixer;

    float master = 1f;
    float music = 1f;
    float sfx = 1f;
    float brightness = 1f;

    public float Master => master;
    public float Music => music;
    public float SFX => sfx;
    public float Brightness => brightness;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    // 🎵 AUDIO
    public void SetMaster(float value)
    {
        master = value;
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MasterVolume", value);
        Debug.Log("SLIDER WORKS: " + value);
    }

    public void SetMusic(float value)
    {
        music = value;
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFX(float value)
    {
        sfx = value;
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // ☀️ BRIGHTNESS
    public void SetBrightness(float value)
    {
        brightness = value;
        PlayerPrefs.SetFloat("Brightness", value);
    }

    void LoadSettings()
    {
        master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
    }
}