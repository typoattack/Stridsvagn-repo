using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullDisplay : MonoBehaviour
{
    private int tankSelector;

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("tankModel"))
        {
            PlayerPrefs.SetInt("tankModel", 0);
        }
        else
        {
            tankSelector = PlayerPrefs.GetInt("tankModel");
        }

        this.transform.GetChild(0).GetChild(tankSelector).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
