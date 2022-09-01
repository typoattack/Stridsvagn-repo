using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public bool isRandomPickup;
    public int ammo;
    //public static int numPickups = 0;

    // Start is called before the first frame update
    void Start()
    {
        //numPickups += 1;
        if (isRandomPickup) ammo = Random.Range(1, 6);
        this.transform.GetChild(ammo).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyPickup()
    {
        //numPickups -= 1;
        if (transform.parent) Destroy(transform.parent.gameObject);
        else Destroy(gameObject);
    }
}
