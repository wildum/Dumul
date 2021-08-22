using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorsAudio : MonoBehaviour
{
    public AudioSource backSound1;
    public AudioSource backSound2;
    public AudioSource cheering1;
    public AudioSource cheering2;
    public AudioSource booing1;
    public AudioSource booing2;
    public AudioSource applause1;
    public AudioSource applause2;

    public void startBackSound()
    {
        backSound1.Play();
        backSound2.Play();
    }

    public void startCheeringEndGame()
    {
        booing1.Stop();
        booing2.Stop();
        applause1.Play();
        applause2.Play();
    }

    public void startBooingEndGame()
    {
        cheering1.Stop();
        cheering2.Stop();
    }

    public void startCheering()
    {
        if (!cheering1.isPlaying)
        {
            cheering1.Play();
            cheering2.Play();
        }
        if (booing1.isPlaying)
        {
            booing1.Stop();
            booing2.Stop();
        }
    }

    public void startBooing()
    {
        if (!booing1.isPlaying)
        {
            booing1.Play();
            booing2.Play();
        }
        if (cheering1.isPlaying)
        {
            cheering1.Stop();
            cheering2.Stop();
        }
    }
}
