using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    #region Variables
    public static AudioController Instance { get; private set; }
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource[] sources; //0 = BGM, 1 = BGA, 2 = SFX
    [SerializeField] private AudioSource specialSource; //For looping SFX ONLY - only ONE CLIP CAN PLAY AT A TIME IN THIS SOURCE
    [SerializeField] private AudioClip[] musics; //0 = Main, 1 = Generic, 2 = Warsan, 3 = Lucus
    [SerializeField] private AudioClip[] ambiences; //0 = Trunk, 1 = Woods, 2 = Ruins

    [SerializeField] private float randomPitchMin = 0.875f; //Lower bounds for random pitch shifting
    [SerializeField] private float randomPitchMax = 1.175f; //Upper bounds for random pitch shifting

    private Coroutine BGMRoutine; //Current BGM fade (make sure multiple don't happen at same time)
    private Coroutine BGARoutine; //Current BGA fade
   

    public enum BGM
    {
        Main, Warsan, Lucus
    }

    public enum BGA
    {
        Trunk, Woods, Ruins
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
        BGMRoutine = null;
        BGARoutine = null;
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

    //Play looping sound, return the audiosource currently playing
    public AudioSource PlayLoopingSound(AudioClip clip)
    {
        //Ignore if multiple calls are being made to this
        if (!specialSource.isPlaying)
        {
            specialSource.clip = clip;
            specialSource.loop = true;
            specialSource.Play();
        }
        return specialSource;
    }

    //Stop sound currently looping in an AudioSource
    public void StopLoopingSound(AudioSource source)
    {
        source.Stop();
    }

    public void PlayMusic(BGM music)
    {
        //Stop coroutine if it is playing so we can apply new state
        if (BGMRoutine != null)
        {
            StopCoroutine(BGMRoutine);
            BGMRoutine = null;
        }
        //Fade between current BGM
        switch (music)
        {
            case BGM.Main:
                BGMRoutine = StartCoroutine(SwapLoopingTrack(sources[0], musics[0], 2f, 2f));
                break;
            case BGM.Warsan:
                BGMRoutine = StartCoroutine(SwapLoopingTrack(sources[0], musics[1], 2f, 2f));
                break;
            case BGM.Lucus:
                BGMRoutine = StartCoroutine(SwapLoopingTrack(sources[0], musics[2], 2f, 2f));
                break;
            default:
                break;
        }


    }

    public void PlayAmbience(BGA ambience)
    {
        //Stop coroutine if it is playing so we can apply new state
        if (BGARoutine != null)
        {
            StopCoroutine(BGARoutine);
            BGARoutine = null;
        }
        //Fade between current ambiences
        switch (ambience)
        {
            case BGA.Trunk:
                BGARoutine = StartCoroutine(SwapLoopingTrack(sources[1], ambiences[0],1f,1f));
                break;
            case BGA.Woods:
                BGARoutine = StartCoroutine(SwapLoopingTrack(sources[1], ambiences[1], 1f, 1f));
                break;
            case BGA.Ruins:
                BGARoutine = StartCoroutine(SwapLoopingTrack(sources[1], ambiences[2], 1f, 1f));
                break;
            default:
                break;
        }
    }

    //Fade out, swap track in channel with selected, fade in of x seconds
    //Adapted from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/#second_method
    private IEnumerator SwapLoopingTrack(AudioSource track, AudioClip clipToPlay, float fadeOutTime, float fadeInTime)
    {
        //Fade out
        float currentTime = 0;
        float currentVol = track.volume;
        
        while (currentTime < fadeOutTime)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, 0f, currentTime / fadeOutTime);
            track.volume = newVol;
            yield return null;
        }


        //Change audio clip
        track.Stop();
        track.clip = clipToPlay;
        track.Play();


        //Fade in
        currentTime = 0f; //Reset variables
        currentVol = 0f;

        while (currentTime < fadeInTime)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, 1f, currentTime / fadeInTime);
            track.volume = newVol;
            yield return null;
        }
        yield break;
    }

    #endregion
}
