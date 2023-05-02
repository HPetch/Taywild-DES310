using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance { get; private set; }


    [SerializeField] private AudioMixer mixer;
    private float masterSliderVal = 1f;
    private float musicSliderVal = 1f;
    private float ambientSliderVal = 1f;
    private float sfxSliderVal = 1f;

    void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }

    //Set a certain mixer group to a certain volume, built for sliders and handles logarithmic stuff
    public void SetVolume(AudioController.SOUND_TYPE type, float sliderValue) //sliderValue should be between 0.0001 and 1.25.
    {
        float scaledValue = Mathf.Log10(sliderValue) * 20; //Logarithmic scale of value.

        switch (type)
        {
            case AudioController.SOUND_TYPE.Master:
                mixer.SetFloat("MasterVolume", scaledValue);
                masterSliderVal = sliderValue; //Hold this to update sliders when we go into a menu.
                break;
            case AudioController.SOUND_TYPE.BGM:
                mixer.SetFloat("MusicVolume", scaledValue);
                musicSliderVal = sliderValue; //Hold this to update sliders when we go into a menu.
                break;
            case AudioController.SOUND_TYPE.BGA:
                mixer.SetFloat("AmbienceVolume", scaledValue);
                ambientSliderVal = sliderValue; //Hold this to update sliders when we go into a menu.
                break;
            case AudioController.SOUND_TYPE.SFX:
                mixer.SetFloat("SFXVolume", scaledValue);
                sfxSliderVal = sliderValue; //Hold this to update sliders when we go into a menu.
                break;
        }
    }

    //Get value that a slider should show - a value between 0.0001 to 1.25.
    public float GetVolumeSlider(AudioController.SOUND_TYPE type)
    {
        switch (type)
        {
            case AudioController.SOUND_TYPE.Master:
                return masterSliderVal;
            case AudioController.SOUND_TYPE.BGM:
                return musicSliderVal;
            case AudioController.SOUND_TYPE.BGA:
                return ambientSliderVal;
            case AudioController.SOUND_TYPE.SFX:
                return sfxSliderVal;
            default:
                return 0f;
        }
    }
}