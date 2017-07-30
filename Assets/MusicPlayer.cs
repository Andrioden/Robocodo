using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MusicPlayer : MonoBehaviour
{
    public List<AudioClip> songs;

    private int currentSong = 0;
    private AudioSource audioSource;

    private void Start()
    {
        songs.Shuffle();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (audioSource.isPlaying == false)
        {
            currentSong++;
            if (currentSong >= songs.Count)
            {
                currentSong = 0;
                audioSource.clip = songs[currentSong];
                audioSource.Play();
            }
        }
    }
}
