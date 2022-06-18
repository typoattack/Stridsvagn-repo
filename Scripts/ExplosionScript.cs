using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ExplosionScript : MonoBehaviour {

    public AudioClip expsound;
    private AudioSource audio;

    public GameObject destroyedTank;
    public GameObject pickup;

    private int decider;

    private void Start()
    {
        audio = GetComponent<AudioSource>();

        audio.PlayOneShot(expsound, 1.0f);
        Destroy(gameObject, expsound.length);
        CameraShake.Shake(0.25f, 0.25f);
        //yield return StartCoroutine(spawnRemains(expsound.length));
    }
    
    IEnumerator spawnRemains(float waitTime)
    {
        decider = Random.Range(0, 10);
        yield return new WaitForSeconds(waitTime);
        if (decider >= 1)
        {
            //GameObject dT = Instantiate(destroyedTank, transform.position, transform.rotation) as GameObject;
        }
        else
        {
            GameObject dT = Instantiate(pickup, transform.position, transform.rotation) as GameObject;
        }
        Destroy(gameObject);
    }
}
