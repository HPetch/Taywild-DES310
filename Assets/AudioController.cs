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
            sources[2].pitch = Random.Range(0.875f, 1.0f);
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
            sources[2].pitch = Random.Range(0.875f, 1.0f);
        else sources[2].pitch = 1.0f;
        //Play a oneshot sound effect (switch out clips in the audio source)
        sources[2].PlayOneShot(sound, volume);
    }

    #endregion
    public void PlayBGM(BGM music)
    {
        //Fade between current BGM
    }

    public void PlayAmbience()
    {
        //Fade between current ambiences
    }

    #endregion
}
