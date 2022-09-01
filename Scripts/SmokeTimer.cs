using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeTimer : MonoBehaviour
{
    public float timer;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(smoke());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator smoke()
    {
        yield return new WaitForSeconds(timer);
        this.transform.GetChild(2).gameObject.SetActive(true);
    }
}
