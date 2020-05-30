using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    public static readonly int maxSoundsPlaying = 10;
    public Sound[] sounds;
    internal List<AudioSource> audioPool = new List<AudioSource>();

    Pool<AudioSource> sources;

    protected override void Awake()
    {
        base.Awake();
        sources = new Pool<AudioSource>(3, maxSoundsPlaying, () => gameObject.AddComponent<AudioSource>(), audio => audio.isPlaying);
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // opgevraagde geluid bestaat
        if (s != null)
        {
            var aud = sources.Acquire();
            // aantal spelende geluiden is lager dan de bovengrens
            if (aud != null)
            {
                // kopieer instellingen en speel het geluid af
                aud.clip = s.clip;
                aud.volume = s.volume;
                aud.pitch = s.pitch;
                aud.loop = s.loop;
                aud.Play();
            }
        }

        // opgevraagde geluid bestaat niet
        else
        {
            Debug.LogError("ERROR: Sound " + name + " not found");
        }
    }
    
    public void StopAllSounds()
    {
        AudioSource[] sounds = GetComponents<AudioSource>();
        foreach (AudioSource s in sounds)
        {
            s.Stop();
        }
    }
}