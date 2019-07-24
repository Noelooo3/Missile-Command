using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource AudioSource;
    public AudioClip MissileLaunch, MissileExplosion, BombExplosion;


    private void Awake()
    {
        Instance = this;
    }

    public void DoMissileLaunch()
    {
        AudioSource.PlayOneShot(MissileLaunch);
    }

    public void DoMissileExplosion()
    {
        AudioSource.PlayOneShot(MissileExplosion);
    }

    public void DoBombExplosion()
    {
        AudioSource.PlayOneShot(BombExplosion);
    }
}
