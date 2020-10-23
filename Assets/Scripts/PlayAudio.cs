using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937    
// October 22nd 2020

// A simple audio class used to play audio in an animation sequence
// Holds a list of clips and plays one when Play() is called
[RequireComponent(typeof(AudioSource))]
public class PlayAudio : MonoBehaviour
{
    AudioSource audioSource;

    public List<AudioClip> clips;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(int index)
    {
        if(index < clips.Count)
        {
            audioSource.PlayOneShot(clips[index]);
        }
    }
}
