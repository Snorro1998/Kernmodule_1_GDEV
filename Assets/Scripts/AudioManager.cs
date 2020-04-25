using UnityEngine;
using System;
using System.Collections.Generic;
//using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public int maxSoundsPlaying = 10;
    public Sound[] sounds;
    internal List<AudioSource> audioPool = new List<AudioSource>();

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < maxSoundsPlaying; i++)
        {
            audioPool.Add(gameObject.AddComponent<AudioSource>());
        }
    }

    public AudioSource GetPooledAudioSource()
    {
        for (int i = 0; i < audioPool.Count; i++)
        {
            if (!audioPool[i].isPlaying)
            {
                return audioPool[i];
            }
        }
        return null;
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        //geluid bestaat
        if (s != null)
        {
            AudioSource aud = GetPooledAudioSource();
            //er spelen momenteel minder geluiden dan het maximum
            if (aud != null)
            {
                //kopieer instellingen en speel het geluid af
                aud.clip = s.clip;
                aud.volume = s.volume;
                aud.pitch = s.pitch;
                aud.loop = s.loop;
                aud.Play();
            }
        }
        //geluid bestaat niet
        else
        {
            Debug.LogError("ERROR: Sound " + name + " not found");
        }
    }
}