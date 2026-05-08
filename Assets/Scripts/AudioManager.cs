using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private Sound[] sounds;
    private bool musicOn = true;
    private bool sfxOn = true;

    [SerializeField] private AudioMixer mainMixer;

    [SerializeField] private AudioSource audioSourcePrefab;

    private readonly List<AudioSource> activeSounds = new();

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private AudioSource Play(Sound sound, Vector3 spawnPos)
    {
        if (sound.audioClip == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Attempted to play a sound with no audio clip assigned.");
#endif
            return null;
        }
        AudioSource audioSource = Instantiate(audioSourcePrefab, spawnPos, Quaternion.identity, transform);
        audioSource.clip = sound.audioClip;
        audioSource.volume = sound.volume;
        audioSource.loop = sound.loop;
        audioSource.outputAudioMixerGroup = sound.soundMixerType;

        audioSource.Play();
        activeSounds.Add(audioSource);

        if (!sound.loop)
        {
            float clipLength = audioSource.clip.length;
            StopSoundInTime(audioSource, clipLength);
        }
        
        return audioSource;
    }

    public void StopSoundInTime(AudioSource source, float time) =>
        StartCoroutine(StopSoundTime(source, time));

    private IEnumerator StopSoundTime(AudioSource source, float time)
    {
        yield return new WaitForSeconds(time);

        if (activeSounds.Remove(source))
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }
    public AudioSource PlaySound(Sound sound)
    {
        return Play(sound, Vector3.zero);
    }
    public void PlaySoundByName(string name)
    {
        var sound = GetSoundByName(name);

        if (sound != null)
            Play(sound, Vector3.zero);
    }

    public void StopSound(Sound sound)
    {
        for (int i = activeSounds.Count - 1; i >= 0; i--)
        {
            AudioSource src = activeSounds[i];
            if (src.clip == sound.audioClip)
            {
                src.Stop();
                activeSounds.RemoveAt(i);
                Destroy(src.gameObject);
            }
        }
    }
    public bool ToggleSFX()
    {
        sfxOn = !sfxOn;

        mainMixer.SetFloat("SFX", sfxOn ? 0f : -80f);

        return sfxOn;
    }
    public bool ToggleMusic()
    {
        musicOn = !musicOn;

        mainMixer.SetFloat("Music", musicOn ? 0f : -80f);

        return musicOn;
    }

    public void StopAllSound()
    {
        for (int i = activeSounds.Count - 1; i >= 0; i--)
        {
            AudioSource src = activeSounds[i];
            src.Stop();
            Destroy(src.gameObject);
        }
        activeSounds.Clear();
    }

    public Sound GetSoundByName(string name)
    {
        return sounds.FirstOrDefault(s => s._name == name);
    }
}
