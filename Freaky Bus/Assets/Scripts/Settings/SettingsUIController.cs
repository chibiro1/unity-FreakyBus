using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
   // public Slider brightnessSlider;

    //public Image brightnessOverlay;

    void Start()
    {
        var sm = SettingsManager.Instance;

        masterSlider.value = sm.Master;
        musicSlider.value = sm.Music;
        sfxSlider.value = sm.SFX;
       


        masterSlider.onValueChanged.AddListener(sm.SetMaster);
        musicSlider.onValueChanged.AddListener(sm.SetMusic);
        sfxSlider.onValueChanged.AddListener(sm.SetSFX);

       // brightnessSlider.onValueChanged.AddListener((value) =>
       // {
       //     sm.SetBrightness(value);
        //    ApplyBrightness(value);
       // });
    }

   // void ApplyBrightness(float value)
   // {
//        Color c = brightnessOverlay.color;
   //     c.a = 1 - value;
   //     brightnessOverlay.color = c;
   // }
}