using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwakeSliderValues : MonoBehaviour
{
    [SerializeField] private AudioController.SOUND_TYPE soundType;
    private Slider slider;
    private void Start()
    {
        slider = GetComponent<Slider>();
        RefreshSlider();
    }

    //Call this each time the menu is opened.
    public void RefreshSlider()
    {
        if (slider != null)
            slider.value = SettingsController.Instance.GetVolumeSlider(soundType);
    }

    //Set the mixer to a new value of something
    public void UpdateVolumeMixer()
    {
        SettingsController.Instance.SetVolume(soundType, slider.value);
    }

}
