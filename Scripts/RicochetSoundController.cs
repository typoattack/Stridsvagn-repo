using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RicochetSoundController : MonoBehaviour {

    public AudioClip expsound;

    private AudioSource audio;

    private void Start()
    {
        audio = GetComponent<AudioSource>();

        audio.PlayOneShot(expsound, 1.0f);
        Destroy(gameObject, expsound.length);
    }
}
