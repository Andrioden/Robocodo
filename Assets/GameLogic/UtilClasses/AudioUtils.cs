using System.Collections;
using UnityEngine;


public class AudioUtils
{
    public static IEnumerator RepeatAudioClipCoroutine(AudioSource audioSource, AudioClip audioClip, int numberOfRepeats, float delayBetween, float volumeScale = 1f)
    {
        for (int i = 0; i < numberOfRepeats; i++)
        {
            audioSource.PlayOneShot(audioClip, volumeScale);

            float delay = delayBetween == 0 ? audioClip.length : delayBetween;
            yield return new WaitForSeconds(delay);
        }
    }
}

