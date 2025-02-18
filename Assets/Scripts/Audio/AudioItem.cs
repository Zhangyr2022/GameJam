using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioItem
{
    public AudioClip clip;
    public float volume = 1f;
    public float pitchVariance = 0.1f;
    public AudioItem(AudioClip clip)
    {
        this.clip = clip;
    }
    public void PlayOn(AudioSource source)
    {
        source.pitch = 1f + Random.Range(-pitchVariance / 2f, pitchVariance / 2f);
        source.PlayOneShot(clip, volume);
    }
}
