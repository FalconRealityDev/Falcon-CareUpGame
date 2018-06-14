﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Narrator class, that plays audiofile as playing 'thinks' about smth
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Narrator : MonoBehaviour {

    private static AudioSource audioSource;

    void Start() {

        if (audioSource == null) {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) Debug.LogError("No AudioSource on narrator found");
        }
    }

    /// <summary>
    /// Plays sound clip
    /// </summary>
    /// <param name="sound">sound name</param>
    /// <returns>True if played</returns>
    public static bool PlaySound(string sound)
    {
        if (audioSource.isPlaying)
        {
            return false;
        }
        else
        {
            if (sound == "WrongAction")
            {
                sound = "WA1-1";
            }

            AudioClip clip = Resources.Load<AudioClip>("Audio/" + sound);
            if (clip == null)
            {
                Debug.LogWarning("No audio clip " + sound + " found!");
            }
            else
            {
                audioSource.PlayOneShot(clip);
            }
            return true;
        }
    }

    /// <summary>
    /// Plays sound clip
    /// </summary>
    /// <param name="sound">sound name</param>
    /// <returns>True if played</returns>
    public static bool PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
        return true;
    } 

    public static void PlaySystemSound(string sound, float volume)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + sound);
        if (clip == null)
        {
            Debug.LogWarning("No audio clip " + sound + " found!");
        }
        else
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
