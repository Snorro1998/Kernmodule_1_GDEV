using UnityEngine;
using System;
using System.Collections.Generic;
//using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public static readonly int maxSoundsPlaying = 10;
    public Sound[] sounds;
    internal List<AudioSource> audioPool = new List<AudioSource>();

    public class Pool<T>
    {
        Stack<T> unclaimed, claimed;
        Func<T, bool> checkInUse;
        Action<T> use;
        int minClaim; // minimum number of items that have to be used before recycling any.

        public Pool(int minClaim, int size, Func<T> factory, Func<T, bool> inUse)
        {
            this.minClaim = minClaim;
            unclaimed = new Stack<T>(size);
            claimed = new Stack<T>();
            checkInUse = inUse;

            // populate unclaimed stack
            for (int i = 0; i < size; ++i)
            {
                unclaimed.Push(factory());
            }
        }

        public T Acquire()
        {
            // reuse any unused items
            while (claimed.Count > minClaim)
            {
                if (checkInUse(claimed.Peek()))
                    break;

                unclaimed.Push(claimed.Pop());
            }

            // use any unclaimed items
            if (unclaimed.Count > 0)
            {
                T top = unclaimed.Pop();
                claimed.Push(top);
                return top;
            }

            // nothing is available
            return default(T);
        }
    }

    Pool<AudioSource> sources;

    protected override void Awake()
    {
        base.Awake();

        sources = new Pool<AudioSource>(3, maxSoundsPlaying, () => gameObject.AddComponent<AudioSource>(), audio => audio.isPlaying);
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        //geluid bestaat
        if (s != null)
        {
            var aud = sources.Acquire();
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