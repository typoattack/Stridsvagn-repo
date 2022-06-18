using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAbilityMenu : MonoBehaviour
{

    CharacterMainMenu menu;
    public int[] tankLocks = new int[8];
    private int[] tankUnlockThreshold = { 0, 10, 20, 30, 40, 50, 70, 80 };
    private int tankSelector;
    private int currentTank;
    private int totalTanks = 8;
    public int isTankValid = 0;

    //public Text tankUnlockText;

    void Start()
    {
        menu = GameObject.Find("CharacterMainMenu").GetComponent<CharacterMainMenu>();
        tankLocks = menu.tankLocks;
        if (!PlayerPrefs.HasKey("tankLocks"))
        {
            PlayerPrefsX.SetIntArray("tankLocks", tankLocks);
        }
        else
        {
            tankLocks = PlayerPrefsX.GetIntArray("tankLocks");
        }
        tankSelector = menu.tankSelector;
        currentTank = tankSelector;
        displayButton(currentTank);
    }

    void Update()
    {
        tankSelector = menu.tankSelector;

        if (currentTank != tankSelector)
        {
            currentTank = tankSelector;
            displayButton(currentTank);
        }
    }

    void displayButton(int ability)
    {
        //tankUnlockText.text = abilityUnlockThreshold[ability] + " coins needed to unlock";
        /*
        if (tankLocks[ability] == 2)
        {
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
            gameObject.transform.GetChild(5).gameObject.SetActive(false);
            //gameObject.transform.GetChild(3).gameObject.SetActive(true);
            isTankValid = 1;
        }
        else
        {
            tankLocks[ability] = 0;
            PlayerPrefsX.SetIntArray("tankLocks", tankLocks);
            //gameObject.transform.GetChild(3).gameObject.SetActive(false);
            gameObject.transform.GetChild(4).gameObject.SetActive(true);
            gameObject.transform.GetChild(5).gameObject.SetActive(false);
            isTankValid = 0;
        }
        */
    }
}