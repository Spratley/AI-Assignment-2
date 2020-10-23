using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937    
// October 22nd 2020

// Simple component for enabling a GameObject and playing a sound at the same time
public class LightingAnimator : MonoBehaviour
{
    // Prevents the animation from replaying
    public bool isDone;
    // Should the delay also be before the first light is turned on?
    public bool warmupDelay;

    public List<GameObject> lights;
    public List<AudioClip> sounds;

    // How long the system should wait before turning on the next light
    public float delay;
    public AudioSource soundSource;

    // Disables all the lights and ensures they all have matching sounds
    public void Start()
    {
        Debug.Assert(lights.Count == sounds.Count);

        foreach (var light in lights)
        {
            light.SetActive(false);
        }
    }

    public void StartLightAnimation()
    {
        StartCoroutine("AnimateLights");
    }

    // Enables each light one at a time down the list, plays the associated sound, and then waits for the delay
    // A coroutine can be paused for a certain amount of time using yield return new WaitForSeconds()
    IEnumerator AnimateLights()
    {
        if (!isDone)
        {
            if (warmupDelay)
            {
                yield return new WaitForSeconds(delay);
            }

            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].SetActive(true);
                soundSource.PlayOneShot(sounds[i], 0.5f);
                yield return new WaitForSeconds(delay);
            }

            isDone = true;
        }
    }
}
