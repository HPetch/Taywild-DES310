using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    #region Variables
    public static AudioController Instance { get; private set; }
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource[] sources; //0 = BGM, 1 = BGA, 2 = SFX

    [SerializeField] private float randomPitchMin = 0.875f; //Lower bounds for random pitch shifting
    [SerializeField] private float randomPitchMax = 1.175f; //Upper bounds for random pitch shifting
   

    public enum BGM
    {
        Main, Generic, Warsan, Lucus
    }

    public enum SOUND_TYPE
    {
        BGM, BGA, SFX
    }
    #endregion

    #region Methods
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        PlayBGM(BGM.Main);
    }

    private void Update()
    {
        //DEBUG
        if (Input.GetKeyDown(KeyCode.T)) FadeMixerGroup.StartFade(mixer, "MainVol", 5f, 1f);
        if (Input.GetKeyDown(KeyCode.Y)) { FadeMixerGroup.StartFade(mixer, "WarsanVol", 5f, 1f); FadeMixerGroup.StartFade(mixer, "MainVol", 5.0f, 0f); }
    }

    #region PlaySound
    public void PlaySound(AudioClip sound)
    {
        //Play a oneshot sound effect (switch out clips in the audio source)
        //Assume volume is default (1.0)
        sources[2].PlayOneShot(sound, 1.0f);
    }
    public void PlaySound(AudioClip sound, bool varyPitch)
    {
        if (varyPitch)
            sources[2].pitch = Random.Range(randomPitchMin, randomPitchMax);
        else sources[2].pitch = 1.0f;
        //Play a oneshot sound effect (switch out clips in the audio source)
        //Assume volume is default (1.0)
        sources[2].PlayOneShot(sound, 1.0f);
    }

    public void PlaySound(AudioClip sound, float volume)
    {
        //Play a oneshot sound effect (switch out clips in the audio source)
        sources[2].PlayOneShot(sound, volume);
        
    }

    public void PlaySound(AudioClip sound, float volume, bool varyPitch)
    {
        if (varyPitch)
            sources[2].pitch = Random.Range(randomPitchMin, randomPitchMax);
        else sources[2].pitch = 1.0f;
        //Play a oneshot sound effect (switch out clips in the audio source)
        sources[2].PlayOneShot(sound, volume);
    }

    #endregion
    public void PlayBGM(BGM music)
    {
        //Fade between current BGM
        switch (music)
        {
            case BGM.Main:
                FadeMixerGroup.StartFade(mixer, "MainVol", 5f, 1.0f);
                break;
            case BGM.Generic:
                FadeMixerGroup.StartFade(mixer, "GenericVol", 5f, 1.0f);
                break;
            case BGM.Warsan:
                FadeMixerGroup.StartFade(mixer, "WarsanVol", 5f, 1.0f);
                break;
            case BGM.Lucus:
                FadeMixerGroup.StartFade(mixer, "LucusVol", 5f, 1.0f);
                break;
            default:
                Debug.LogWarning("Invalid music switch detected!");
                break;
        }


    }

    public void PlayAmbience()
    {
        //Fade between current ambiences
    }

    #endregion
}
