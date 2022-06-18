using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public bool isRandom;
    public int isActive;
    public float shieldUptime;
    public float shieldDowntime;
    private WaitForSeconds shieldActive;
    private WaitForSeconds shieldInactive;

    // Start is called before the first frame update
    void Start()
    {
        shieldActive = new WaitForSeconds(shieldUptime);
        shieldInactive = new WaitForSeconds(shieldDowntime);
        if (isRandom) isActive = Random.Range(0, 2);
        if (isActive == 1) StartCoroutine(cycleShield());
        else this.transform.GetChild(3).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator cycleShield()
    {
        while (true)
        {
            this.transform.GetChild(3).gameObject.SetActive(true);
            yield return shieldActive;
            this.transform.GetChild(3).gameObject.SetActive(false);
            yield return shieldInactive;
        }
    }
}
