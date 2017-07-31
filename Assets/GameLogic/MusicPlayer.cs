using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MusicPlayer : MonoBehaviour
{
    /* This is a really basic music player. We would need to create something more polished before shipping. */

    public AudioSource audioSource;
    public List<AudioClip> songs;

    private int currentSong = 0;
    private bool songIsQueued = false;

    private void Start()
    {
        songs.Shuffle();
    }

    private void Update()
    {
        if (!audioSource.isPlaying && !songIsQueued)
        {
            currentSong++;
            if (currentSong >= songs.Count)            
                currentSong = 0;            

            audioSource.clip = songs[currentSong];
            StartCoroutine(PlayAfterDelay());
        }
    }

    /* This is used to create a small pause before the next track start. */
    private IEnumerator PlayAfterDelay()
    {
        songIsQueued = true;
        yield return new WaitForSecondsRealtime(1f);

        audioSource.Play();
        songIsQueued = false;
    }
}
