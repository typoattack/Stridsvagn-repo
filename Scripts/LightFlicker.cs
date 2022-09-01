using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float timer;
    public Light light;
    public bool alwaysOn;
    private int alwaysOnRandom;
    public bool canBeRandom;
    private int randomNumber = 0;
    private int randomOffOn = 1;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (!alwaysOn) alwaysOnRandom = Random.Range(0, 2);
        if (alwaysOnRandom == 1 && canBeRandom) randomNumber = Random.Range(0, 2);
        while (true)
        {
            if (randomNumber == 1) randomOffOn = Random.Range(0, 4);
            if ((!alwaysOn || alwaysOnRandom == 1) && (!canBeRandom || randomOffOn == 0)) light.enabled = !(light.enabled); //toggle on/off the enabled property
            yield return new WaitForSeconds(timer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
