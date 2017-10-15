using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> songs;
    private float originalVolume;

    private int currentSong = 0;
    private bool isFadingOut = false;

    private void Start()
    {
        if (!Settings.Sound_EnableMusic)
        {
            audioSource.enabled = false;
            enabled = false;
            return;
        }

        songs.Shuffle();
        originalVolume = audioSource.volume;
    }

    private void Update()
    {
        if (!LobbyManager.HasGameStarted)
            return;

        if (!audioSource.isPlaying)
        {
            currentSong++;
            if (currentSong >= songs.Count)
                currentSong = 0;

            audioSource.clip = songs[currentSong];
            StartCoroutine(FadeIn(audioSource, 3f));
        }
        else
        if (!isFadingOut && (audioSource.clip.length - audioSource.time < 5))
            StartCoroutine(FadeOut(audioSource, 5f));
    }

    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        isFadingOut = true;
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        isFadingOut = false;

        yield return null;
    }

    public IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        float startVolume = 0.2f;
        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < originalVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }

        audioSource.volume = originalVolume;

        yield return null;
    }
}
