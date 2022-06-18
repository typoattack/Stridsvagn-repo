using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankModelDisplay : MonoBehaviour
{
    CharacterMainMenu menu;
    public int tankSelector;
    private int currentTank;
    private int maxNumberOfTanks = 8;

    // Start is called before the first frame update
    void Start()
    {
        menu = GameObject.Find("CharacterMainMenu").GetComponent<CharacterMainMenu>();
        if (!PlayerPrefs.HasKey("tankModel"))
        {
            PlayerPrefs.SetInt("tankModel", 0);
        }
        else
        {
            tankSelector = PlayerPrefs.GetInt("tankModel");
        }
        currentTank = tankSelector;
        //this.transform.GetChild(0).GetChild(tankSelector).gameObject.SetActive(true);
        //this.transform.GetChild(1).GetChild(0).GetChild(tankSelector).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        tankSelector = menu.tankSelector;

        if (currentTank != tankSelector)
        {
            currentTank = tankSelector;
            displayTank(currentTank);
        }
    }

    void displayTank(int tankModel)
    {
        for (int a = 0; a < maxNumberOfTanks; a++)
        {
            if (a == tankModel)
            {
                this.transform.GetChild(0).GetChild(a).gameObject.SetActive(true);
                this.transform.GetChild(1).GetChild(0).GetChild(a).gameObject.SetActive(true);
            }
            else
            {
                this.transform.GetChild(0).GetChild(a).gameObject.SetActive(false);
                this.transform.GetChild(1).GetChild(0).GetChild(a).gameObject.SetActive(false);
            }
        }
    }
}
