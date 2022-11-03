using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhaustSmokeScript : MonoBehaviour
{
    private float lts;
    private float rts;

    private ParticleSystem exhaust;
    // Start is called before the first frame update
    void Start()
    {
        exhaust = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        lts = Input.GetAxis("Vertical");
        rts = Input.GetAxis("Vertical2");

        var exhaustSpeed = exhaust.main;
        if (Mathf.Abs(rts) > 0f || Mathf.Abs(lts) > 0f) exhaustSpeed.simulationSpeed = 1.0f;
        else exhaustSpeed.simulationSpeed = 0.25f;
    }
}
