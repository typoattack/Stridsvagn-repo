using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMainMenu : MonoBehaviour
{
    private int gameMode;
    public int tankSelector = 0;
    private int totalTanks = 8;
    private float detectDistance;
    private int tankHP;

    public int[] tankLocks = { 2, 0, 0, 0, 0, 0, 0, 0 };

    void Start()
    {
        if (!PlayerPrefs.HasKey("gameMode"))
        {
            PlayerPrefs.SetInt("gameMode", 3);
        }
        else
        {
            gameMode = PlayerPrefs.GetInt("gameMode");
        }

        if (!PlayerPrefs.HasKey("tankModel"))
        {
            PlayerPrefs.SetInt("tankModel", 0);
        }
        else
        {
            tankSelector = PlayerPrefs.GetInt("tankModel");
        }

        if (!PlayerPrefs.HasKey("tankHP"))
        {
            PlayerPrefs.SetInt("tankHP", 10);
        }
        else
        {
            tankHP = PlayerPrefs.GetInt("tankHP");
        }

        if (!PlayerPrefs.HasKey("detectDistance"))
        {
            PlayerPrefs.SetFloat("detectDistance", 14.0f);
        }
        else
        {
            detectDistance = PlayerPrefs.GetFloat("detectDistance");
        }

        if (!PlayerPrefs.HasKey("tankLocks"))
        {
            PlayerPrefsX.SetIntArray("tankLocks", tankLocks);
        }
        else
        {
            tankLocks = PlayerPrefsX.GetIntArray("tankLocks");
        }
    }

    void Update()
    {
        tankSelector = tankSelector % totalTanks;
    }

    public void AddToAbilities()
    {
        tankSelector++;
        if (tankSelector == totalTanks) tankSelector = 0;
    }

    public void SubtractFromAbilities()
    {
        tankSelector--;
        if (tankSelector < 0) tankSelector = totalTanks - 1;
    }

    public void ConfirmAbility()
    {
        PlayerPrefs.SetInt("tankModel", tankSelector);

        if (tankSelector == 1 || tankSelector == 2 || tankSelector == 7) PlayerPrefs.SetFloat("detectDistance", 10.0f);
        else PlayerPrefs.SetFloat("detectDistance", 14.0f);

        if (tankSelector == 3 || tankSelector == 4) PlayerPrefs.SetInt("tankHP", 15);
        else if (tankSelector == 5 || tankSelector == 7) PlayerPrefs.SetInt("tankHP", 20);
        else PlayerPrefs.SetInt("tankHP", 10);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void GoToLevelSelect()
    {
        if (gameMode == 3) SceneManager.LoadScene("LevelSelectSP", LoadSceneMode.Single);
        if (gameMode == 4) SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}