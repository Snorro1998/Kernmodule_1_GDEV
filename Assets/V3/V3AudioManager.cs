using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class V3AudioManager : Singleton<V3AudioManager>
{
    internal AudioSource musicSlot;
    internal Queue<AudioSource> soundSlots = new Queue<AudioSource>();
    public int maxSoundsPlaying = 10;

    public Sound[] sounds;

    protected override void Awake()
    {
        base.Awake();
        musicSlot = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < maxSoundsPlaying; i++)
        {
            AudioSource aud = gameObject.AddComponent<AudioSource>();
            soundSlots.Enqueue(aud);
        }
    }

    private void SetSoundSlotAndPlay(AudioSource aud, Sound s)
    {
        aud.clip = s.clip;
        aud.volume = s.volume;
        aud.pitch = s.pitch;
        aud.loop = s.loop;
        aud.Play();
    }

    internal void PlayMusic(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s != null)
        {
            SetSoundSlotAndPlay(musicSlot, s);
        }
    }

    internal void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // verzochte geluid bestaat
        if (s != null)
        {
            for (int i = 0; i < soundSlots.Count + 1; i++)
            {
                AudioSource aud = soundSlots.Dequeue();
                soundSlots.Enqueue(aud);

                // geluidelement speelt momenteel niets af
                if (!aud.isPlaying)
                {
                    SetSoundSlotAndPlay(aud, s);
                    break;
                }
            }
        }

        else
        {
            Debug.LogError("ERROR: Sound " + name + " not found");
        }
    }
}
