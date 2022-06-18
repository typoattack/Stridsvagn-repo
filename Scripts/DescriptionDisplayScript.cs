using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionDisplayScript : MonoBehaviour
{
    CharacterMainMenu menu;
    private int tankSelector;
    private int currentTank = 0;
    private int maxNumberOfTanks = 8;

    // Use this for initialization
    void Start()
    {
        menu = GameObject.Find("CharacterMainMenu").GetComponent<CharacterMainMenu>();
        tankSelector = menu.tankSelector;
        currentTank = tankSelector;
        displayImage(currentTank);
    }

    // Update is called once per frame
    void Update()
    {
        tankSelector = menu.tankSelector;

        if (currentTank != tankSelector)
        {
            currentTank = tankSelector;
            displayImage(currentTank);
        }
    }

    void displayImage(int ability)
    {
        for (int a = 0; a < maxNumberOfTanks; a++)
        {
            if (a == ability) gameObject.transform.GetChild(a).gameObject.SetActive(true);
            else gameObject.transform.GetChild(a).gameObject.SetActive(false);
        }
    }
}
